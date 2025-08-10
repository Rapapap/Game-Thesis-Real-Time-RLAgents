using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using static NormalEnemyActions;

[RequireComponent(typeof(Rigidbody), typeof(RayPerceptionSensorComponent3D), typeof(RL_EnemyController))]
public class NormalEnemyAgent : Agent
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private RL_EnemyController rl_EnemyController;
    [SerializeField] private NormalEnemyRewards rewardConfig;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody agentRigidbody;

    [Header("Movement Configuration")]
    [SerializeField] public float moveSpeed = 3.5f;
    [SerializeField] public float rotationSpeed = 120f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Vector2 debugTextOffset = new Vector2(10, 10);
    [SerializeField] private Color debugTextColor = Color.white;
    [SerializeField] private int debugFontSize = 14;
    #endregion

    #region Public Properties
    public static bool TrainingActive = true;
    public float CurrentHealth => rl_EnemyController.enemyHP;
    public float MaxHealth => rl_EnemyController.enemyData.enemyHealth;
    public bool IsDead => rl_EnemyController.healthState.IsDead;
    #endregion

    #region Private Variables
    private PlayerDetection playerDetection;
    private PatrolSystem patrolSystem;
    private EnhancedMovementController movementController;
    private DebugDisplay debugDisplay;
    private EnhancedObstacleDetection obstacleDetection;
    
    private const float DISTANCE_NORMALIZATION_FACTOR = 50f;
    private const float VELOCITY_NORMALIZATION_FACTOR = 10f;
    private const float STUCK_THRESHOLD = 0.5f;
    private const float STUCK_TIME_LIMIT = 2f;
    private const float OBSTACLE_DETECTION_DISTANCE = 2f;
    private const float CHASE_MOVEMENT_THRESHOLD = 0.05f; 
    private const float CHASE_REWARD_INTERVAL = 0.5f;

    private Vector3 initialPosition;
    private string currentState = "Idle";
    private string currentAction = "Idle";
    private float previousDistanceToPlayer = float.MaxValue;
    private float lastAttackTime;
    private bool isInitialized = false;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private bool wasPlayerVisible = false;
    private Vector3 lastPositionForChaseReward;
    private float chaseMovementAccumulator = 0f;
    private bool isCurrentlyChasing = false;
    
    // Enhanced obstacle avoidance variables
    private float obstacleCollisionTimer = 0f;
    private const float OBSTACLE_COLLISION_PUNISHMENT_INTERVAL = 0.1f;
    private int consecutiveObstacleHits = 0;
    private const int MAX_CONSECUTIVE_HITS = 5;
    #endregion

    #region Agent Lifecycle
    public override void Initialize()
    {
        rl_EnemyController ??= GetComponent<RL_EnemyController>();
        agentRigidbody ??= GetComponent<Rigidbody>();

        if (rl_EnemyController == null || agentRigidbody == null)
        {
            Debug.LogError("NormalEnemyAgent: Missing required components!", gameObject);
            enabled = false;
            return;
        }

        if (!rl_EnemyController.IsInitialized)
        {
            rl_EnemyController.ForceInitialize();
        }

        InitializeComponents();
        InitializeSystems();
        ConfigureRigidbody();
        DisableConflictingComponents();

        initialPosition = transform.position;
        lastPosition = transform.position;
        lastPositionForChaseReward = transform.position;
        ResetAgentState();
        rl_EnemyController.InitializeHealthBar();
        isInitialized = true;
    }

    public override void OnEpisodeBegin()
    {
        if (!isInitialized)
        {
            Initialize();
            if (!isInitialized) return;
        }

        ResetForNewEpisode();
        RespawnAtRandomLocation();
        ResetTrainingArena();
        rl_EnemyController.ShowHealthBar();

        if (animator != null)
        {
            animator.SetBool("isDead", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
            animator.ResetTrigger("getHit");
        }

        GetComponent<Collider>().enabled = true;
        agentRigidbody.linearVelocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!isInitialized || rl_EnemyController == null) return;
        
        // Health observation (1)
        sensor.AddObservation(CurrentHealth / MaxHealth);
        
        // Player-related observations (4)
        bool playerAvailable = playerDetection.IsPlayerAvailable();
        sensor.AddObservation(playerAvailable ? 1f : 0f);
        
        if (playerAvailable)
        {
            Vector3 playerPos = playerDetection.GetPlayerPosition();
            Vector3 directionToPlayer = (playerPos - transform.position).normalized;
            float distanceToPlayer = playerDetection.GetDistanceToPlayer(transform.position);
            
            Vector3 localPlayerDirection = transform.InverseTransformDirection(directionToPlayer);
            sensor.AddObservation(localPlayerDirection.x);
            sensor.AddObservation(localPlayerDirection.z);
            sensor.AddObservation(distanceToPlayer / DISTANCE_NORMALIZATION_FACTOR);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(1f);
        }
        
        // Agent velocity (local space) (2)
        Vector3 localVelocity = transform.InverseTransformDirection(agentRigidbody.linearVelocity);
        sensor.AddObservation(localVelocity.x / VELOCITY_NORMALIZATION_FACTOR);
        sensor.AddObservation(localVelocity.z / VELOCITY_NORMALIZATION_FACTOR);
        
        // Enhanced obstacle observations (4 directions) (4)
        var obstacleInfo = obstacleDetection.GetObstacleDistances();
        sensor.AddObservation(obstacleInfo.forward / OBSTACLE_DETECTION_DISTANCE);
        sensor.AddObservation(obstacleInfo.right / OBSTACLE_DETECTION_DISTANCE);
        sensor.AddObservation(obstacleInfo.left / OBSTACLE_DETECTION_DISTANCE);
        sensor.AddObservation(obstacleInfo.back / OBSTACLE_DETECTION_DISTANCE);
        
        // State observations (3)
        sensor.AddObservation(IsAgentKnockedBack() ? 1f : 0f);
        sensor.AddObservation(IsAgentFleeing() ? 1f : 0f);
        sensor.AddObservation(ShouldAgentFlee() ? 1f : 0f);
        
        // Patrol-related observations (3)
        if (patrolSystem.HasValidPatrolPoints())
        {
            Vector3 patrolTarget = patrolSystem.GetCurrentPatrolTarget();
            Vector3 directionToPatrol = (patrolTarget - transform.position).normalized;
            Vector3 localPatrolDirection = transform.InverseTransformDirection(directionToPatrol);
            
            sensor.AddObservation(localPatrolDirection.x);
            sensor.AddObservation(localPatrolDirection.z);
            sensor.AddObservation(Vector3.Distance(transform.position, patrolTarget) / DISTANCE_NORMALIZATION_FACTOR);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        // Additional obstacle avoidance observations (2)
        Vector2 avoidanceDirection = obstacleDetection.GetAvoidanceDirection();
        sensor.AddObservation(avoidanceDirection.x);
        sensor.AddObservation(avoidanceDirection.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isInitialized || rl_EnemyController == null || IsDead || !isActiveAndEnabled) return;

        debugDisplay.IncrementSteps();
        ProcessActions(actions);
        UpdateBehaviorAndRewards();
        CheckStuckState();
        CheckEpisodeEnd();
        UpdateObstacleCollisionTimer();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
        continuousActions[2] = GetRotationInputHeuristic();
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
    #endregion

    #region Initialization & Reset Helpers
    private void InitializeComponents()
    {
        var raySensor = GetComponent<RayPerceptionSensorComponent3D>();
        playerDetection = new PlayerDetection(raySensor, LayerMask.GetMask("Wall", "Environment", "Enemy"));
        obstacleDetection = new EnhancedObstacleDetection(transform, LayerMask.GetMask("Wall", "Obstacle", "Environment", "Gate"), OBSTACLE_DETECTION_DISTANCE);
    }

    private void InitializeSystems()
    {
        Transform[] patrolPoints = FindPatrolPoints();
        patrolSystem = new PatrolSystem(patrolPoints);
        movementController = new EnhancedMovementController(agentRigidbody, transform, moveSpeed, rotationSpeed);
        debugDisplay = new DebugDisplay();
        
        if (patrolPoints.Length > 0)
        {
            Debug.Log($"{gameObject.name} initialized with {patrolPoints.Length} patrol points");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no patrol points assigned!");
        }
    }

    private void ConfigureRigidbody()
    {
        if (agentRigidbody == null) return;

        agentRigidbody.mass = 1f;
        agentRigidbody.linearDamping = 2f;  // Increased for better control
        agentRigidbody.angularDamping = 5f; // Increased to prevent over-rotation
        agentRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        agentRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        agentRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | 
                                    RigidbodyConstraints.FreezeRotationZ | 
                                    RigidbodyConstraints.FreezePositionY;
    }

    private void DisableConflictingComponents()
    {
        var components = GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component != this && 
                (component.GetType().Name.Contains("Movement") || 
                 component.GetType().Name.Contains("Controller")) &&
                !component.GetType().Name.Contains("RL_Enemy"))
            {
                component.enabled = false;
                Debug.Log($"Disabled conflicting component: {component.GetType().Name}");
            }
        }
    }

    private Transform[] FindPatrolPoints()
    {
        var spawner = FindFirstObjectByType<RL_TrainingEnemySpawner>();
        if (spawner != null)
        {
            Transform parentTransform = transform.parent;
            if (parentTransform != null)
            {
                Transform[] arenaPatrolPoints = spawner.GetArenaPatrolPoints(parentTransform);
                if (arenaPatrolPoints != null && arenaPatrolPoints.Length > 0)
                {
                    System.Array.Sort(arenaPatrolPoints, (x, y) => string.Compare(x.name, y.name));
                    return arenaPatrolPoints;
                }
            }
        }

        var nearbyPoints = Physics.OverlapSphere(transform.position, 30f, LayerMask.GetMask("Ground"))
            .Where(c => c.CompareTag("Patrol Point"))
            .Select(c => c.transform)
            .OrderBy(p => p.name)
            .Take(4)
            .ToArray();

        return nearbyPoints.Length > 0 ? nearbyPoints : new Transform[0];
    }

    private void ResetForNewEpisode()
    {
        ResetAgentState();
        rl_EnemyController.enemyHP = rl_EnemyController.enemyData.enemyHealth;
        rl_EnemyController.healthState.ResetHealthState();
        rl_EnemyController.InitializeHealthBar();
        rl_EnemyController.fleeState?.Reset();

        currentState = "Idle";
        currentAction = "Idle";
        lastAttackTime = Time.fixedTime - 2f;
        stuckTimer = 0f;
        lastPosition = transform.position;

        lastPositionForChaseReward = transform.position;
        chaseMovementAccumulator = 0f;
        isCurrentlyChasing = false;
        previousDistanceToPlayer = float.MaxValue;

        // Reset obstacle collision tracking
        obstacleCollisionTimer = 0f;
        consecutiveObstacleHits = 0;

        patrolSystem?.Reset();
        movementController?.Reset();
        gameObject.SetActive(true);
        GetComponent<Collider>().enabled = true;
    }

    private void ResetAgentState()
    {
        playerDetection?.Reset();
        patrolSystem?.Reset();
        debugDisplay?.Reset();
        movementController?.Reset();
        
        if (agentRigidbody != null)
        {
            agentRigidbody.linearVelocity = Vector3.zero;
            agentRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void RespawnAtRandomLocation()
    {
        var patrolPoints = patrolSystem.GetPatrolPoints();
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name} has no patrol points for respawning!");
            return;
        }

        int randomIndex = Random.Range(0, patrolPoints.Length);
        Vector3 respawnPosition = patrolPoints[randomIndex].position;
        
        Vector2 randomOffset = Random.insideUnitCircle * 2f;
        respawnPosition += new Vector3(randomOffset.x, 0, randomOffset.y);
        
        transform.position = respawnPosition;
        agentRigidbody.linearVelocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;
        
        patrolSystem.ResetToSpecificPoint(randomIndex);
        lastPosition = respawnPosition;
        lastPositionForChaseReward = respawnPosition;
    }

    private void ResetTrainingArena()
    {
        FindFirstObjectByType<RL_TrainingTargetSpawner>()?.ResetArena();
    }

    private void CheckEpisodeEnd()
    {
        if (StepCount >= MaxStep)
        {
            EndEpisode();
        }
        
        if (patrolSystem.PatrolLoopsCompleted >= 3)
        {
            rewardConfig.AddPatrolReward(this);
            EndEpisode();
        }
    }
    #endregion

    #region Action Processing
    private void ProcessActions(ActionBuffers actions)
    {
        if (!isInitialized || IsDead || !isActiveAndEnabled) return;

        float forward = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float right = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        float rotation = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
        bool shouldAttack = actions.DiscreteActions[0] == 1;

        // Handle different behavioral states with integrated obstacle avoidance
        if (IsAgentKnockedBack())
        {
            HandleKnockbackState();
        }
        else if (IsAgentFleeing())
        {
            HandleFleeingState(forward, right, rotation);
        }
        else if (playerDetection.IsPlayerVisible && !ShouldAgentFlee())
        {
            HandleChaseState(forward, right, rotation, shouldAttack);
        }
        else
        {
            HandlePatrolState(forward, right, rotation);
        }

        UpdateMovementAnimation();
    }

    private void HandleKnockbackState()
    {
        currentState = "Knocked Back";
        currentAction = "Recovering";
        isCurrentlyChasing = false;
        movementController.ProcessMovementWithObstacleAvoidance(0f, 0f, 0f, obstacleDetection);
    }

    private void HandleFleeingState(float forward, float right, float rotation)
    {
        currentState = "Fleeing";
        currentAction = "Fleeing";
        isCurrentlyChasing = false;
        
        if (playerDetection.IsPlayerAvailable())
        {
            Vector3 fleeDirection = (transform.position - playerDetection.GetPlayerPosition()).normalized;
            Vector3 localFleeDir = transform.InverseTransformDirection(fleeDirection);
            
            float fleeForward = Mathf.Max(forward, localFleeDir.z * 1.2f);
            float fleeRight = right + localFleeDir.x * 0.8f;
            
            movementController.ProcessMovementWithObstacleAvoidance(fleeForward, fleeRight, rotation, obstacleDetection);
        }
        else
        {
            movementController.ProcessMovementWithObstacleAvoidance(forward, right, rotation, obstacleDetection);
        }
    }

    private void HandleChaseState(float forward, float right, float rotation, bool shouldAttack)
    {
        currentState = "Chasing";
        currentAction = "Chasing";

        Vector3 playerPos = playerDetection.GetPlayerPosition();
        bool playerInRange = IsPlayerInAttackRange();
        
        if (!isCurrentlyChasing)
        {
            rewardConfig.AddChasePlayerReward(this);
            isCurrentlyChasing = true;
        }

        if (playerInRange)
        {
            movementController.FaceTargetSmoothly(playerPos);
            ProcessAttackAction(shouldAttack);
            // Minimal movement when in attack range to avoid obstacles
            movementController.ProcessMovementWithObstacleAvoidance(0f, 0f, 0f, obstacleDetection);
        }
        else
        {
            Vector3 directionToPlayer = (playerPos - transform.position).normalized;
            Vector3 localDirection = transform.InverseTransformDirection(directionToPlayer);
            
            float chaseForward = Mathf.Max(forward, localDirection.z * 0.8f);
            float chaseRight = right + localDirection.x * 0.3f;
            
            movementController.ProcessMovementWithObstacleAvoidance(chaseForward, chaseRight, rotation, obstacleDetection);
        }
    }

    private void HandlePatrolState(float forward, float right, float rotation)
    {
        currentState = "Patrolling";
        currentAction = "Patrolling";
        isCurrentlyChasing = false;

        if (patrolSystem.HasValidPatrolPoints())
        {
            Vector3 currentTarget = patrolSystem.GetCurrentPatrolTarget();
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
            
            if (distanceToTarget < 2f)
            {
                bool completedLoop = patrolSystem.AdvanceToNextWaypoint();
                if (completedLoop)
                {
                    rewardConfig.AddPatrolReward(this);
                }
                currentAction = patrolSystem.IsIdlingAtSpawn() ? "Idling" : "Patrolling";
                
                // Stop movement when reached target
                movementController.ProcessMovementWithObstacleAvoidance(0f, 0f, 0f, obstacleDetection);
            }
            else
            {
                Vector3 directionToTarget = (currentTarget - transform.position).normalized;
                Vector3 localDirection = transform.InverseTransformDirection(directionToTarget);
                
                float patrolForward = Mathf.Max(forward, localDirection.z * 0.6f);
                float patrolRight = right + localDirection.x * 0.4f;
                
                movementController.ProcessMovementWithObstacleAvoidance(patrolForward, patrolRight, rotation, obstacleDetection);
            }

            if (patrolSystem.IsIdlingAtSpawn())
            {
                patrolSystem.UpdateIdleTimer();
            }
        }
        else
        {
            movementController.ProcessMovementWithObstacleAvoidance(forward, right, rotation, obstacleDetection);
            currentAction = "Exploring";
            rewardConfig.AddPatrolWrongStepPunishment(this);
        }
    }

    private void ExecuteAttack()
    {
        lastAttackTime = Time.fixedTime;
        rewardConfig.AddAttackReward(this);
        rl_EnemyController.AgentAttack();
        currentState = "Attacking";
        currentAction = "Attacking";
    }

    private void ProcessAttackAction(bool shouldAttack)
    {
        bool canAttack = Time.fixedTime - lastAttackTime >= 0.5f;
        bool shouldAttackAnim = canAttack && rl_EnemyController.combatState.IsAttacking;

        if (shouldAttack && canAttack)
        {
            ExecuteAttack();
        }
        else if (canAttack && !shouldAttack)
        {
            rewardConfig.AddDoesntAttackInstantlyPunishment(this);
        }
        else if (shouldAttackAnim && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.Play("Attack", 0, 0f);
        }
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null || IsDead) return;

        bool isMoving = agentRigidbody.linearVelocity.sqrMagnitude > 0.1f;
        bool isAttacking = rl_EnemyController.combatState.IsAttacking;

        animator.SetBool("isWalking", isMoving && !isAttacking);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isIdle", !isMoving && !isAttacking);
    }

    private void CheckStuckState()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        
        if (distanceMoved < STUCK_THRESHOLD)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= STUCK_TIME_LIMIT)
            {
                AddReward(-0.2f); // Increased punishment for being stuck
                stuckTimer = 0f;
                
                // Additional punishment if stuck near obstacles
                if (obstacleDetection.IsObstacleWithin(1.5f))
                {
                    AddReward(-0.1f);
                }
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
    }

    private void UpdateObstacleCollisionTimer()
    {
        if (obstacleCollisionTimer > 0f)
        {
            obstacleCollisionTimer -= Time.fixedDeltaTime;
        }
    }
    #endregion

    #region Reward & Behavior Updates
    private void UpdateBehaviorAndRewards()
    {
        playerDetection.UpdatePlayerDetection(transform.position);
        if (playerDetection.IsPlayerVisible && !wasPlayerVisible)
        {
            rewardConfig.AddDetectionReward(this);
        }
        wasPlayerVisible = playerDetection.IsPlayerVisible;

        obstacleDetection.UpdateObstacleDetection();
        ProcessRewards(Time.deltaTime);
        ProcessObstacleAvoidanceRewards();
        debugDisplay.UpdateCumulativeReward(GetCumulativeReward());
    }

    private void ProcessObstacleAvoidanceRewards()
    {
        // Reward for maintaining good distance from obstacles
        var obstacles = obstacleDetection.GetObstacleDistances();
        float minDistance = Mathf.Min(obstacles.forward, obstacles.right, obstacles.left, obstacles.back);
        
        if (minDistance > OBSTACLE_DETECTION_DISTANCE * 0.8f)
        {
            // Small positive reward for maintaining safe distance
            AddReward(0.001f * Time.fixedDeltaTime);
        }
        else if (minDistance < OBSTACLE_DETECTION_DISTANCE * 0.3f)
        {
            // Punishment for getting too close to obstacles
            AddReward(-0.005f * Time.fixedDeltaTime);
        }
    }

    private void ProcessRewards(float deltaTime)
    {
        if (currentAction == "Chasing")
        {
            float moved = Vector3.Distance(transform.position, lastPositionForChaseReward);

            if (moved > CHASE_MOVEMENT_THRESHOLD)
            {
                chaseMovementAccumulator += deltaTime;

                if (chaseMovementAccumulator >= CHASE_REWARD_INTERVAL)
                {
                    rewardConfig.AddChaseStepReward(this, chaseMovementAccumulator);
                    ProcessChaseRewards(chaseMovementAccumulator);
                    chaseMovementAccumulator = 0f;
                    lastPositionForChaseReward = transform.position;
                }
            }
        }
        else if (currentAction == "Patrolling")
        {
            rewardConfig.AddPatrolStepReward(this, deltaTime);
        }
        else if (currentAction == "Idling")
        {
            if (!patrolSystem.IsIdlingAtSpawn() && agentRigidbody.linearVelocity.magnitude < 0.1f)
            {
                rewardConfig.AddNoMovementPunishment(this, deltaTime);
            }
        }

        ProcessPlayerVisibilityRewards();
    }
    
    private void ProcessChaseRewards(float deltaTimeBucket)
    {
        if (playerDetection.IsPlayerAvailable())
        {
            float currentDistance = playerDetection.GetDistanceToPlayer(transform.position);
            if (currentDistance < previousDistanceToPlayer)
            {
                rewardConfig.AddApproachPlayerReward(this, deltaTimeBucket);
            }
            else
            {
                rewardConfig.AddFailApproachPlayerPunishment(this);
            }
            previousDistanceToPlayer = currentDistance;
        }
    }

    private void ProcessPlayerVisibilityRewards()
    {
        if (playerDetection.IsPlayerVisible && !currentAction.Contains("Chasing"))
            rewardConfig.AddDoesntChasePlayerPunishment(this, chaseMovementAccumulator);
    }

    public void HandleEnemyDeath()
    {
        rewardConfig.AddDeathPunishment(this);
        currentState = "Dead";
        currentAction = "Dead";
        isCurrentlyChasing = false;
        
        agentRigidbody.linearVelocity = Vector3.zero;
        agentRigidbody.angularVelocity = Vector3.zero;
        
        EndEpisode();
    }

    public void HandleDamage()
    {
        rewardConfig.AddDamagePunishment(this);
        currentState = "Taking Damage";
        currentAction = "Reacting";
        isCurrentlyChasing = false;
    }
    
    public void HandleAttackMissed()
    {
        rewardConfig.AddAttackMissedPunishment(this);
    }

    public void HandleKillPlayer()
    {
        rewardConfig.AddKillPlayerReward(this);
    }
    #endregion

    #region Utility & Debug
    private float GetRotationInputHeuristic()
    {
        if (Input.GetKey(KeyCode.Q)) return -1f;
        if (Input.GetKey(KeyCode.E)) return 1f;
        return 0f;
    }

    private bool IsAgentKnockedBack() => rl_EnemyController.IsKnockedBack();
    private bool IsAgentFleeing() => rl_EnemyController.IsFleeing();
    private bool ShouldAgentFlee() => rl_EnemyController.IsHealthLow() && playerDetection.IsPlayerAvailable();
    private bool IsPlayerInAttackRange() =>
        playerDetection.IsPlayerAvailable() &&
        Vector3.Distance(transform.position, playerDetection.GetPlayerPosition()) <= rl_EnemyController.attackRange;
    
    public void SetPatrolPoints(Transform[] points) => patrolSystem?.SetPatrolPoints(points);

    void OnGUI()
    {
        if (showDebugInfo)
        {
            debugDisplay.DisplayDebugInfo(gameObject.name, currentState, currentAction, debugTextOffset, 
                debugTextColor, debugFontSize, patrolSystem.PatrolLoopsCompleted);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Wall", "Obstacle", "Environment", "Gate")) != 0)
        {
            // Enhanced obstacle collision punishment
            if (obstacleCollisionTimer <= 0f)
            {
                consecutiveObstacleHits++;
                
                // Progressive punishment for consecutive hits
                float punishmentMultiplier = 1f + (consecutiveObstacleHits * 0.5f);
                float basePunishment = -0.1f;
                
                rewardConfig.AddObstaclePunishment(this, Time.deltaTime * punishmentMultiplier);
                AddReward(basePunishment * punishmentMultiplier);
                
                // Reset timer to prevent spam punishment
                obstacleCollisionTimer = OBSTACLE_COLLISION_PUNISHMENT_INTERVAL;
                
                // Force the agent to stop and reconsider
                agentRigidbody.linearVelocity *= 0.5f;
                
                Debug.Log($"{gameObject.name} hit obstacle: {collision.gameObject.name}, consecutive hits: {consecutiveObstacleHits}");
            }
        }
        else
        {
            // Reset consecutive hits when touching non-obstacles
            consecutiveObstacleHits = 0;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Wall", "Obstacle", "Environment", "Gate")) != 0)
        {
            // Continuous punishment for staying against obstacles
            if (obstacleCollisionTimer <= 0f)
            {
                AddReward(-0.05f * Time.fixedDeltaTime);
                obstacleCollisionTimer = OBSTACLE_COLLISION_PUNISHMENT_INTERVAL;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Wall", "Obstacle", "Environment", "Gate")) != 0)
        {
            // Small reward for successfully avoiding/leaving obstacles
            AddReward(0.02f);
            consecutiveObstacleHits = Mathf.Max(0, consecutiveObstacleHits - 1);
        }
    }
    #endregion
}

#region Enhanced Helper Classes
public class EnhancedMovementController
{
    private readonly Rigidbody agentRigidbody;
    private readonly Transform agentTransform;
    private readonly float moveSpeed;
    private readonly float rotationSpeed;
    private readonly float maxVelocity;

    // Enhanced rotation control
    private float targetRotationY;
    private bool isRotating = false;
    private const float ROTATION_THRESHOLD = 5f; // Degrees
    private const float ROTATION_COMPLETION_THRESHOLD = 2f; // Degrees

    public EnhancedMovementController(Rigidbody rigidbody, Transform transform, float moveSpeed, float rotationSpeed)
    {
        agentRigidbody = rigidbody;
        agentTransform = transform;
        this.moveSpeed = moveSpeed;
        this.rotationSpeed = rotationSpeed;
        maxVelocity = moveSpeed * 1.2f;
        targetRotationY = transform.eulerAngles.y;
    }

    public void Reset()
    {
        if (agentRigidbody != null)
        {
            agentRigidbody.linearVelocity = Vector3.zero;
            agentRigidbody.angularVelocity = Vector3.zero;
        }
        targetRotationY = agentTransform.eulerAngles.y;
        isRotating = false;
    }

    public void ProcessMovementWithObstacleAvoidance(float forward, float right, float rotation, EnhancedObstacleDetection obstacleDetection)
    {
        // Get obstacle avoidance direction
        Vector2 avoidanceDirection = obstacleDetection.GetAvoidanceDirection();
        var obstacles = obstacleDetection.GetObstacleDistances();
        
        // Determine if we need to prioritize avoidance over intended movement
        bool needsAvoidance = obstacles.forward < 1.5f || obstacles.right < 1.0f || obstacles.left < 1.0f;
        
        Vector3 intendedMovement = Vector3.zero;
        float intendedRotation = rotation;
        
        if (needsAvoidance && avoidanceDirection.magnitude > 0.1f)
        {
            // Prioritize obstacle avoidance
            float avoidanceForward = avoidanceDirection.y;
            float avoidanceRight = avoidanceDirection.x;
            
            // Blend intended movement with avoidance (favor avoidance)
            float avoidanceWeight = Mathf.Clamp01(1f - obstacles.forward / 2f);
            forward = Mathf.Lerp(forward, avoidanceForward, avoidanceWeight * 0.8f);
            right = Mathf.Lerp(right, avoidanceRight, avoidanceWeight * 0.8f);
            
            // Calculate rotation needed for avoidance
            if (avoidanceDirection.magnitude > 0.3f)
            {
                Vector3 worldAvoidanceDir = agentTransform.TransformDirection(new Vector3(avoidanceDirection.x, 0, avoidanceDirection.y));
                float targetAngle = Mathf.Atan2(worldAvoidanceDir.x, worldAvoidanceDir.z) * Mathf.Rad2Deg;
                float angleDifference = Mathf.DeltaAngle(agentTransform.eulerAngles.y, targetAngle);
                
                if (Mathf.Abs(angleDifference) > ROTATION_THRESHOLD)
                {
                    intendedRotation = Mathf.Sign(angleDifference) * Mathf.Clamp01(Mathf.Abs(angleDifference) / 90f);
                }
            }
        }
        
        // Apply movement
        ProcessMovement(forward, right, intendedRotation);
        
        // Additional safety: reduce speed when very close to obstacles
        if (obstacles.forward < 0.8f)
        {
            Vector3 currentVel = agentRigidbody.linearVelocity;
            agentRigidbody.linearVelocity = currentVel * 0.7f;
        }
    }

    public void ProcessMovement(float forward, float right, float rotation)
    {
        // Enhanced movement with better coordination between movement and rotation
        Vector3 moveDirection = Vector3.zero;
        
        // Only apply movement if rotation is not too aggressive
        if (Mathf.Abs(rotation) < 0.7f || !isRotating)
        {
            moveDirection = (agentTransform.forward * forward + agentTransform.right * right);
            float magnitude = Mathf.Clamp01(moveDirection.magnitude);
            
            if (magnitude > 0.1f)
            {
                moveDirection = moveDirection.normalized * magnitude;
                
                // Reduce movement speed during rotation
                if (Mathf.Abs(rotation) > 0.3f)
                {
                    magnitude *= 0.6f;
                }
                
                // Apply movement force
                Vector3 force = moveDirection * moveSpeed * magnitude * 80f;
                agentRigidbody.AddForce(force, ForceMode.Force);
            }
        }
        
        // Limit horizontal velocity
        LimitVelocity();
        
        // Enhanced rotation handling
        HandleRotationSmooth(rotation);
    }

    private void HandleRotationSmooth(float rotationInput)
    {
        const float rotationDeadzone = 0.1f;
        const float maxAngularVelocity = 180f; // degrees per second
        
        if (Mathf.Abs(rotationInput) > rotationDeadzone)
        {
            isRotating = true;
            
            // Calculate target angular velocity
            float targetAngularVel = rotationInput * maxAngularVelocity * Mathf.Deg2Rad;
            
            // Smooth the angular velocity change
            float currentAngularVel = agentRigidbody.angularVelocity.y;
            float smoothedAngularVel = Mathf.Lerp(currentAngularVel, targetAngularVel, 0.4f);
            
            agentRigidbody.angularVelocity = new Vector3(0f, smoothedAngularVel, 0f);
        }
        else
        {
            // Apply strong damping when no rotation input
            Vector3 currentAngVel = agentRigidbody.angularVelocity;
            Vector3 dampedAngVel = Vector3.Lerp(currentAngVel, Vector3.zero, 0.8f);
            agentRigidbody.angularVelocity = new Vector3(0f, dampedAngVel.y, 0f);
            
            // Check if rotation has stopped
            if (Mathf.Abs(dampedAngVel.y) < 0.1f)
            {
                isRotating = false;
            }
        }
    }

    private void LimitVelocity()
    {
        Vector3 velocity = agentRigidbody.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        
        if (horizontalVelocity.magnitude > maxVelocity)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxVelocity;
            agentRigidbody.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        }
    }

    public void FaceTargetSmoothly(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - agentTransform.position);
        direction.y = 0;
        
        if (direction.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float currentAngle = agentTransform.eulerAngles.y;
            float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
            
            if (Mathf.Abs(angleDifference) > ROTATION_COMPLETION_THRESHOLD)
            {
                float rotationDirection = Mathf.Sign(angleDifference);
                float rotationSpeed = Mathf.Clamp01(Mathf.Abs(angleDifference) / 45f);
                
                // Apply smooth rotation
                float targetAngularVelocity = rotationDirection * rotationSpeed * 120f * Mathf.Deg2Rad;
                agentRigidbody.angularVelocity = new Vector3(0f, targetAngularVelocity, 0f);
                isRotating = true;
            }
            else
            {
                agentRigidbody.angularVelocity = Vector3.zero;
                isRotating = false;
            }
        }
    }

    // Legacy method for backward compatibility
    public void FaceTarget(Vector3 targetPosition)
    {
        FaceTargetSmoothly(targetPosition);
    }
}

[System.Serializable]
public struct ObstacleDistances
{
    public float forward;
    public float right;
    public float left;
    public float back;
    
    public ObstacleDistances(float forward, float right, float left, float back)
    {
        this.forward = forward;
        this.right = right;
        this.left = left;
        this.back = back;
    }
}

public class EnhancedObstacleDetection
{
    private readonly Transform agentTransform;
    private readonly LayerMask obstacleLayerMask;
    private readonly float detectionDistance;
    private ObstacleDistances obstacleDistances;
    private Vector2 avoidanceDirection;

    // Enhanced detection with more rays for better accuracy
    private readonly float[] rayOffsets = { -0.3f, 0f, 0.3f }; // Left, center, right offsets

    public EnhancedObstacleDetection(Transform transform, LayerMask obstacleMask, float distance)
    {
        agentTransform = transform;
        obstacleLayerMask = obstacleMask;
        detectionDistance = distance;
        obstacleDistances = new ObstacleDistances(distance, distance, distance, distance);
        avoidanceDirection = Vector2.zero;
    }

    public void UpdateObstacleDetection()
    {
        Vector3 rayStart = agentTransform.position + Vector3.up * 0.5f;
        
        // Cast multiple rays in each direction for better detection
        float forwardDist = GetMinDistanceInDirection(rayStart, agentTransform.forward);
        float rightDist = GetMinDistanceInDirection(rayStart, agentTransform.right);
        float leftDist = GetMinDistanceInDirection(rayStart, -agentTransform.right);
        float backDist = GetMinDistanceInDirection(rayStart, -agentTransform.forward);
        
        obstacleDistances = new ObstacleDistances(forwardDist, rightDist, leftDist, backDist);
        
        // Calculate enhanced avoidance direction
        CalculateEnhancedAvoidanceDirection();
    }

    private float GetMinDistanceInDirection(Vector3 origin, Vector3 direction)
    {
        float minDistance = detectionDistance;
        
        foreach (float offset in rayOffsets)
        {
            Vector3 rayOrigin = origin + agentTransform.right * offset * 0.5f;
            float distance = CastSingleRay(rayOrigin, direction);
            minDistance = Mathf.Min(minDistance, distance);
        }
        
        return minDistance;
    }

    private float CastSingleRay(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, detectionDistance, obstacleLayerMask))
        {
            // Visual debugging
            Debug.DrawRay(origin, direction * hit.distance, Color.red, 0.1f);
            return hit.distance;
        }
        else
        {
            Debug.DrawRay(origin, direction * detectionDistance, Color.green, 0.1f);
            return detectionDistance;
        }
    }

    private void CalculateEnhancedAvoidanceDirection()
    {
        Vector2 avoidance = Vector2.zero;
        
        // More sophisticated avoidance calculation
        float urgencyThreshold = detectionDistance * 0.7f;
        float criticalThreshold = detectionDistance * 0.4f;
        
        // Forward obstacle handling
        if (obstacleDistances.forward < urgencyThreshold)
        {
            float urgency = 1f - (obstacleDistances.forward / urgencyThreshold);
            float backwardForce = urgency * 0.9f;
            
            // Determine best escape direction
            float leftSpace = obstacleDistances.left / detectionDistance;
            float rightSpace = obstacleDistances.right / detectionDistance;
            
            avoidance.y -= backwardForce;
            
            if (leftSpace > rightSpace && leftSpace > 0.5f)
            {
                avoidance.x -= urgency * 0.8f; // Move left
            }
            else if (rightSpace > 0.5f)
            {
                avoidance.x += urgency * 0.8f; // Move right
            }
        }
        
        // Side obstacle handling with different urgency levels
        if (obstacleDistances.right < urgencyThreshold)
        {
            float urgency = 1f - (obstacleDistances.right / urgencyThreshold);
            avoidance.x -= urgency * 0.8f; // Move left
            
            // If critical, also move backward
            if (obstacleDistances.right < criticalThreshold)
            {
                avoidance.y -= urgency * 0.4f;
            }
        }
        
        if (obstacleDistances.left < urgencyThreshold)
        {
            float urgency = 1f - (obstacleDistances.left / urgencyThreshold);
            avoidance.x += urgency * 0.8f; // Move right
            
            // If critical, also move backward
            if (obstacleDistances.left < criticalThreshold)
            {
                avoidance.y -= urgency * 0.4f;
            }
        }
        
        // Back obstacle - less critical but still important
        if (obstacleDistances.back < detectionDistance * 0.6f)
        {
            float urgency = 1f - (obstacleDistances.back / (detectionDistance * 0.6f));
            avoidance.y += urgency * 0.6f; // Move forward
        }
        
        // Clamp and smooth the avoidance direction
        avoidanceDirection = Vector2.ClampMagnitude(avoidance, 1f);
        
        // Apply smoothing to avoid jittery movement
        const float smoothing = 0.3f;
        avoidanceDirection = Vector2.Lerp(avoidanceDirection, avoidance, smoothing);
    }

    public ObstacleDistances GetObstacleDistances() => obstacleDistances;
    public Vector2 GetAvoidanceDirection() => avoidanceDirection;
    
    public bool IsObstacleAhead() => obstacleDistances.forward < detectionDistance * 0.8f;
    public bool IsObstacleWithin(float distance) => 
        obstacleDistances.forward <= distance || 
        obstacleDistances.right <= distance || 
        obstacleDistances.left <= distance;
    
    public bool IsSurrounded() =>
        obstacleDistances.forward < detectionDistance * 0.6f &&
        obstacleDistances.right < detectionDistance * 0.6f &&
        obstacleDistances.left < detectionDistance * 0.6f;
}

public class DebugDisplay
{
    private float cumulativeReward;
    private int episodeSteps;

    public void Reset()
    {
        cumulativeReward = 0f;
        episodeSteps = 0;
    }

    public void IncrementSteps() => episodeSteps++;
    public void UpdateCumulativeReward(float reward) => cumulativeReward = reward;

    public void DisplayDebugInfo(string agentName, string currentState, string currentAction, Vector2 offset, Color textColor, int fontSize, int patrolLoops)
    {
        var labelStyle = new GUIStyle
        {
            fontSize = fontSize,
            normal = { textColor = textColor }
        };

        string debugText = $"{agentName}:\nState: {currentState}\nAction: {currentAction}\nSteps: {episodeSteps}\nCumulative Reward: {cumulativeReward:F3}\nPatrol Loops: {patrolLoops}";
        GUI.Label(new Rect(offset.x, offset.y, 300, 150), debugText, labelStyle);
    }
}
#endregion