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
    private const float OBSTACLE_AVOIDANCE_STRENGTH = 2.5f;

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
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isInitialized || rl_EnemyController == null || IsDead || !isActiveAndEnabled) return;

        debugDisplay.IncrementSteps();
        ProcessActions(actions);
        UpdateBehaviorAndRewards();
        CheckStuckState();
        CheckEpisodeEnd();
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
        agentRigidbody.linearDamping = 1f;  // Reduced for better responsiveness
        agentRigidbody.angularDamping = 3f; // Reduced for better turning
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

    // ... [Other initialization methods remain the same] ...

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

        // Get obstacle avoidance adjustments
        Vector2 obstacleAvoidance = obstacleDetection.GetAvoidanceDirection();
        
        // Apply obstacle avoidance to movement
        forward += obstacleAvoidance.y * OBSTACLE_AVOIDANCE_STRENGTH;
        right += obstacleAvoidance.x * OBSTACLE_AVOIDANCE_STRENGTH;
        
        // Clamp after obstacle avoidance
        forward = Mathf.Clamp(forward, -1f, 1f);
        right = Mathf.Clamp(right, -1f, 1f);

        // Handle different behavioral states
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
            
            movementController.ProcessMovement(fleeForward, fleeRight, rotation);
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
            movementController.FaceTarget(playerPos);
            ProcessAttackAction(shouldAttack);
        }
        else
        {
            Vector3 directionToPlayer = (playerPos - transform.position).normalized;
            Vector3 localDirection = transform.InverseTransformDirection(directionToPlayer);
            
            float chaseForward = Mathf.Max(forward, localDirection.z * 0.8f);
            float chaseRight = right + localDirection.x * 0.3f;
            
            movementController.ProcessMovement(chaseForward, chaseRight, rotation);
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
            }
            else
            {
                Vector3 directionToTarget = (currentTarget - transform.position).normalized;
                Vector3 localDirection = transform.InverseTransformDirection(directionToTarget);
                
                float patrolForward = Mathf.Max(forward, localDirection.z * 0.6f);
                float patrolRight = right + localDirection.x * 0.4f;
                
                movementController.ProcessMovement(patrolForward, patrolRight, rotation);
            }

            if (patrolSystem.IsIdlingAtSpawn())
            {
                patrolSystem.UpdateIdleTimer();
            }
        }
        else
        {
            movementController.ProcessMovement(forward, right, rotation);
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
                AddReward(-0.1f);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
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
        debugDisplay.UpdateCumulativeReward(GetCumulativeReward());
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
            debugDisplay.DisplayDebugInfo(gameObject.name, currentState, currentAction, debugTextOffset, debugTextColor, debugFontSize, patrolSystem.PatrolLoopsCompleted);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & LayerMask.GetMask("Wall", "Obstacle", "Environment")) != 0)
        {
            rewardConfig.AddObstaclePunishment(this, Time.deltaTime);
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

    public EnhancedMovementController(Rigidbody rigidbody, Transform transform, float moveSpeed, float rotationSpeed)
    {
        agentRigidbody = rigidbody;
        agentTransform = transform;
        this.moveSpeed = moveSpeed; // Use actual moveSpeed value without multiplication
        this.rotationSpeed = rotationSpeed;
        maxVelocity = moveSpeed * 1.5f; // Dynamic max velocity based on move speed
    }

    public void Reset()
    {
        if (agentRigidbody != null)
        {
            agentRigidbody.linearVelocity = Vector3.zero;
            agentRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void ProcessMovement(float forward, float right, float rotation)
    {
        // Enhanced movement with proper force application
        Vector3 moveDirection = (agentTransform.forward * forward + agentTransform.right * right);
        float magnitude = Mathf.Clamp01(moveDirection.magnitude);
        moveDirection = moveDirection.normalized;
        
        // Apply movement force proportional to desired speed
        Vector3 force = moveDirection * moveSpeed * magnitude * 100f; // Increased force multiplier
        agentRigidbody.AddForce(force, ForceMode.Force);
        
        // Limit horizontal velocity only
        Vector3 velocity = agentRigidbody.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        
        if (horizontalVelocity.magnitude > maxVelocity)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxVelocity;
            agentRigidbody.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        }
        
        // Enhanced rotation handling
        HandleRotation(rotation);
    }

    private void HandleRotation(float rotationInput)
    {
        const float rotationDeadzone = 0.05f;
        const float dampingFactor = 0.3f;
        
        if (Mathf.Abs(rotationInput) > rotationDeadzone)
        {
            float targetAngularVelocity = rotationInput * rotationSpeed * Mathf.Deg2Rad;
            float currentAngularVelocity = agentRigidbody.angularVelocity.y;
            float smoothedAngularVelocity = Mathf.Lerp(currentAngularVelocity, targetAngularVelocity, dampingFactor);
            agentRigidbody.angularVelocity = new Vector3(0f, smoothedAngularVelocity, 0f);
        }
        else
        {
            // Apply damping when no rotation input
            Vector3 currentAngVel = agentRigidbody.angularVelocity;
            Vector3 dampedAngVel = Vector3.Lerp(currentAngVel, Vector3.zero, 0.5f);
            agentRigidbody.angularVelocity = new Vector3(0f, dampedAngVel.y, 0f);
        }
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - agentTransform.position);
        direction.y = 0;
        
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            agentTransform.rotation = Quaternion.Slerp(
                agentTransform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime * 0.02f
            );
        }
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
        
        // Cast rays in 4 directions
        float forwardDist = CastRay(rayStart, agentTransform.forward, Color.green);
        float rightDist = CastRay(rayStart, agentTransform.right, Color.blue);
        float leftDist = CastRay(rayStart, -agentTransform.right, Color.blue);
        float backDist = CastRay(rayStart, -agentTransform.forward, Color.yellow);
        
        obstacleDistances = new ObstacleDistances(forwardDist, rightDist, leftDist, backDist);
        
        // Calculate avoidance direction
        CalculateAvoidanceDirection();
    }

    private float CastRay(Vector3 origin, Vector3 direction, Color debugColor)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, detectionDistance, obstacleLayerMask))
        {
            Debug.DrawRay(origin, direction * hit.distance, Color.red);
            Debug.DrawRay(origin + direction * hit.distance, direction * (detectionDistance - hit.distance), debugColor);
            return hit.distance;
        }
        else
        {
            Debug.DrawRay(origin, direction * detectionDistance, debugColor);
            return detectionDistance;
        }
    }

    private void CalculateAvoidanceDirection()
    {
        Vector2 avoidance = Vector2.zero;
        
        // Forward obstacle - move backward and to the side with more space
        if (obstacleDistances.forward < detectionDistance * 0.8f)
        {
            float avoidanceStrength = 1f - (obstacleDistances.forward / detectionDistance);
            avoidance.y -= avoidanceStrength * 0.8f; // Move backward
            
            // Choose side with more space
            if (obstacleDistances.right > obstacleDistances.left)
            {
                avoidance.x += avoidanceStrength * 0.6f; // Move right
            }
            else
            {
                avoidance.x -= avoidanceStrength * 0.6f; // Move left
            }
        }
        
        // Side obstacles
        if (obstacleDistances.right < detectionDistance * 0.7f)
        {
            float avoidanceStrength = 1f - (obstacleDistances.right / detectionDistance);
            avoidance.x -= avoidanceStrength * 0.7f; // Move left
        }
        
        if (obstacleDistances.left < detectionDistance * 0.7f)
        {
            float avoidanceStrength = 1f - (obstacleDistances.left / detectionDistance);
            avoidance.x += avoidanceStrength * 0.7f; // Move right
        }
        
        // Back obstacle - move forward
        if (obstacleDistances.back < detectionDistance * 0.6f)
        {
            float avoidanceStrength = 1f - (obstacleDistances.back / detectionDistance);
            avoidance.y += avoidanceStrength * 0.5f; // Move forward
        }
        
        avoidanceDirection = Vector2.ClampMagnitude(avoidance, 1f);
    }

    public ObstacleDistances GetObstacleDistances() => obstacleDistances;
    public Vector2 GetAvoidanceDirection() => avoidanceDirection;
    
    public bool IsObstacleAhead() => obstacleDistances.forward < detectionDistance * 0.8f;
    public bool IsObstacleWithin(float distance) => obstacleDistances.forward <= distance;
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