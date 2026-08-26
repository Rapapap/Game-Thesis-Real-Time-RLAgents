using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Natural Dynamic Combat Player Bot for RL Training.
/// 
/// Behaves like a real, unpredictable human player:
/// - Dynamic Multi-Targeting: Switches targets dynamically after 1-2 hits or randomly among active enemies.
/// - Tactical Post-Attack Maneuvers: Retreats, circle-strafes, or dashes away after striking instead of sticking to one NPC.
/// - Fluid Organic Footwork: Moves with dynamic weaving and curved approach vectors instead of rigid lines.
/// - Reactive Evasive Dash: Performs quick dash bursts to escape pressure or reposition.
/// - Anti-Clipping Spacing: Cleanly maintains personal space and never clips into NPCs.
/// 
/// Preserves 100% public interface compatibility with RL_EnemyController,
/// NormalEnemyAgents, and RL_TrainingPlayerSpawner.
/// </summary>
public class RL_PlayerController : MonoBehaviour
{
    public static RL_PlayerController Instance;
    public static event System.Action OnPlayerDestroyed;

    public enum CombatTacticState
    {
        Hunting,            // Approaching current target with dynamic footwork
        Striking,           // Delivering melee attack swing
        PostAttackManeuver, // Tactical disengage / retreat / strafe after attack
        EvasiveDashing      // Fast dash burst
    }

    [Header("Training Configuration")]
    [SerializeField] public bool isRL_TrainingTarget = false;
    [SerializeField] private bool enableDynamicMovement = true;

    [Header("Combat Settings")]
    [SerializeField] private float maxHealth = 150f;
    [SerializeField] private float attackInterval = 0.75f;
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float stopDistance = 2.0f;
    [SerializeField] private float minPersonalSpace = 1.4f;
    [SerializeField] private float sensorDetectionRadius = 40f;
    [SerializeField] private float invincibilityDuration = 0.3f;

    [Header("Movement & Agility Settings")]
    [SerializeField] private float chaseSpeed = 5.2f;
    [SerializeField] private float retreatSpeed = 4.2f;
    [SerializeField] private float dashSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 16f;
    [SerializeField] private float dashDuration = 0.22f;
    [SerializeField] private float dashCooldown = 3.0f;

    [Header("Dynamic Multi-Targeting")]
    [Tooltip("Max hits on same target before forcing target switch (1-2 hits for fluid hit-and-run)")]
    [SerializeField] private int maxComboHits = 2;
    [SerializeField] private float postAttackManeuverDuration = 0.8f;
    [SerializeField] private float maxTargetCommitDuration = 3.5f;

    [Header("Obstacle & Wall Avoidance")]
    [SerializeField] private float wallDetectionRange = 2.0f;
    [SerializeField] private LayerMask obstacleLayers;

    [Header("UI Components")]
    [SerializeField] private Slider healthBarSlider;

    [Header("Animation & Effects")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem hurtParticle;
    [SerializeField] private ParticleSystem deathParticle;
    [SerializeField] private ParticleSystem dashParticle;

    public float CurrentHealth => currentHealth;
    public bool IsAlive => isAlive;
    public CombatTacticState CurrentState => currentState;

    // Runtime state
    private float currentHealth;
    private bool isInvincible = false;
    private bool isAlive = true;
    private bool attackEnabled = true;
    private float lastAttackTime = -10f;
    private float lastDashTime = -10f;
    private Vector3 initialPosition;
    private Collider[] colliders;
    private Rigidbody rb;
    private CharacterController characterController;
    private float initialAttackRange;

    // Dynamic targeting & maneuver state
    private CombatTacticState currentState = CombatTacticState.Hunting;
    private Transform currentTarget = null;
    private int currentComboHits = 0;
    private float targetAcquireTime = 0f;
    private float maneuverStartTime = 0f;
    private Vector3 currentManeuverVector = Vector3.zero;
    private float weaveSeed;

    #region Unity Lifecycle
    private void Awake()
    {
        Instance = this;
        initialPosition = transform.position;
        colliders = GetComponentsInChildren<Collider>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        initialAttackRange = attackRange;
        weaveSeed = Random.Range(0f, 100f);

        if (obstacleLayers == 0)
        {
            obstacleLayers = LayerMask.GetMask("Wall", "Obstacle", "Environment", "Gate");
        }

        ConfigurePhysicsBody();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        InitializeHealthBar();
        InitializeAnimationState();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;

        if (enableDynamicMovement && currentState != CombatTacticState.Striking && currentState != CombatTacticState.EvasiveDashing)
        {
            UpdateDynamicCombatLoop();
        }
    }
    #endregion

    #region Physics Body Setup
    private void ConfigurePhysicsBody()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearDamping = 1f;
            rb.angularDamping = 3f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ |
                             RigidbodyConstraints.FreezePositionY;
        }
    }
    #endregion

    #region Dynamic Combat AI Engine
    private void UpdateDynamicCombatLoop()
    {
        List<Transform> activeEnemies = FindNearbyEnemies(sensorDetectionRadius);

        if (activeEnemies.Count == 0)
        {
            currentTarget = null;
            ApplyMovementVelocity(Vector3.zero);
            UpdateAnimationMovement(false);
            return;
        }

        // Validate or pick new dynamic target
        ValidateOrAcquireTarget(activeEnemies);
        if (currentTarget == null) return;

        float distToTarget = Vector3.Distance(transform.position, currentTarget.position);

        switch (currentState)
        {
            case CombatTacticState.Hunting:
                ExecuteHuntingBehavior(activeEnemies, distToTarget);
                break;

            case CombatTacticState.PostAttackManeuver:
                ExecutePostAttackManeuver(activeEnemies, distToTarget);
                break;
        }
    }

    /// <summary>
    /// Approaching target with organic curved footwork, ready to strike upon reaching melee range.
    /// </summary>
    private void ExecuteHuntingBehavior(List<Transform> enemies, float distToTarget)
    {
        FaceTarget(currentTarget.position);

        // Check if strike opportunity is reached
        bool isReadyToAttack = (Time.time - lastAttackTime >= attackInterval);
        if (attackEnabled && isReadyToAttack && distToTarget <= attackRange)
        {
            StartCoroutine(ExecuteMeleeStrikeRoutine(currentTarget, enemies));
            return;
        }

        // Check if we should switch target due to timeout
        if (Time.time - targetAcquireTime > maxTargetCommitDuration)
        {
            SwitchToNewTarget(enemies);
            return;
        }

        // Spacing and Movement toward target
        Vector3 dirToTarget = (currentTarget.position - transform.position).normalized;
        dirToTarget.y = 0;

        Vector3 moveDir;
        float speed;

        if (distToTarget > stopDistance)
        {
            // Curved organic approach: add subtle sinusoidal lateral weave
            Vector3 lateralDir = Vector3.Cross(Vector3.up, dirToTarget).normalized;
            float weave = Mathf.Sin((Time.time + weaveSeed) * 3.5f) * 0.45f;
            moveDir = (dirToTarget + lateralDir * weave).normalized;
            speed = chaseSpeed;
        }
        else if (distToTarget < minPersonalSpace)
        {
            // Back up slightly if NPC walks into player
            moveDir = -dirToTarget;
            speed = retreatSpeed;
        }
        else
        {
            // Sweet spot: hold ground facing enemy
            Vector3 lateralDir = Vector3.Cross(Vector3.up, dirToTarget).normalized;
            float weave = Mathf.Sin((Time.time + weaveSeed) * 2.0f) * 0.3f;
            moveDir = lateralDir * weave;
            speed = 1.0f;
        }

        // Apply obstacle avoidance
        Vector3 wallRepulsion = CalculateWallRepulsion();
        if (wallRepulsion.sqrMagnitude > 0.01f)
        {
            moveDir = (moveDir * 0.6f + wallRepulsion * 1.4f).normalized;
            speed = Mathf.Max(speed, chaseSpeed * 0.8f);
        }

        moveDir.y = 0;
        ApplyMovementVelocity(moveDir.normalized * speed);
        UpdateAnimationMovement(speed > 0.5f && moveDir.sqrMagnitude > 0.01f);
    }

    /// <summary>
    /// Executes tactical hit-and-run disengagement (retreating or circle-strafing into open space).
    /// </summary>
    private void ExecutePostAttackManeuver(List<Transform> enemies, float distToTarget)
    {
        if (Time.time - maneuverStartTime >= postAttackManeuverDuration)
        {
            // Maneuver finished: Decide whether to switch target or re-engage
            SwitchToNewTarget(enemies);
            currentState = CombatTacticState.Hunting;
            return;
        }

        // Move along chosen maneuver vector
        Vector3 moveDir = currentManeuverVector;
        Vector3 wallRepulsion = CalculateWallRepulsion();
        if (wallRepulsion.sqrMagnitude > 0.01f)
        {
            moveDir = (moveDir * 0.5f + wallRepulsion * 1.5f).normalized;
        }

        moveDir.y = 0;
        ApplyMovementVelocity(moveDir.normalized * retreatSpeed);
        UpdateAnimationMovement(true);

        if (currentTarget != null)
        {
            FaceTarget(currentTarget.position);
        }
    }

    private IEnumerator ExecuteMeleeStrikeRoutine(Transform target, List<Transform> enemies)
    {
        currentState = CombatTacticState.Striking;
        lastAttackTime = Time.time;
        currentComboHits++;

        ApplyMovementVelocity(Vector3.zero);
        UpdateAnimationMovement(false);

        if (target != null)
        {
            FaceTarget(target.position);
        }

        PlayAttackAnimation();
        yield return new WaitForSeconds(0.16f);

        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange * 1.5f)
        {
            DealDamageToEnemy(target);
        }

        yield return new WaitForSeconds(0.16f);

        // --- POST-ATTACK TACTICAL DECISION ---
        if (currentComboHits >= maxComboHits || Random.value < 0.65f)
        {
            // Hit combo completed or dynamic roll -> DISENGAGE & SWITCH TARGET!
            currentComboHits = 0;
            DecidePostAttackManeuver(target, enemies);
        }
        else
        {
            // Stay in hunting mode to deliver next combo strike
            currentState = CombatTacticState.Hunting;
        }
    }

    private void DecidePostAttackManeuver(Transform target, List<Transform> enemies)
    {
        maneuverStartTime = Time.time;
        Vector3 playerPos = transform.position;

        // Vector away from the attacked target
        Vector3 awayFromTarget = (target != null) ? (playerPos - target.position).normalized : -transform.forward;
        awayFromTarget.y = 0;

        float roll = Random.value;

        if (roll < 0.35f && Time.time - lastDashTime >= dashCooldown)
        {
            // Option 1 (35%): Quick Evasive Dash away to open space
            StartCoroutine(ExecuteEvasiveDashRoutine(awayFromTarget));
        }
        else
        {
            // Option 2 (65%): Fluid Retreat / Lateral Flank Step
            Vector3 lateral = Vector3.Cross(Vector3.up, awayFromTarget).normalized * ((Random.value > 0.5f) ? 1f : -1f);
            currentManeuverVector = (awayFromTarget * 0.7f + lateral * 0.7f).normalized;
            currentState = CombatTacticState.PostAttackManeuver;
        }
    }

    private IEnumerator ExecuteEvasiveDashRoutine(Vector3 dashDirection)
    {
        currentState = CombatTacticState.EvasiveDashing;
        lastDashTime = Time.time;

        PlayParticleEffect(dashParticle);
        StartCoroutine(InvincibilityRoutine(dashDuration + 0.1f));

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            ApplyMovementVelocity(dashDirection.normalized * dashSpeed);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        currentState = CombatTacticState.Hunting;
    }
    #endregion

    #region Multi-Targeting Selection Engine
    private void ValidateOrAcquireTarget(List<Transform> enemies)
    {
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            SwitchToNewTarget(enemies);
        }
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;
        var controller = target.GetComponentInParent<RL_EnemyController>();
        return controller != null && !controller.IsDead();
    }

    /// <summary>
    /// Dynamically selects a new target from active enemies with weighted probabilities
    /// (prevents tunnel vision on one NPC and enables unpredictable multi-directional combat).
    /// </summary>
    private void SwitchToNewTarget(List<Transform> enemies)
    {
        if (enemies.Count == 0)
        {
            currentTarget = null;
            return;
        }

        List<Transform> candidates = new List<Transform>();
        foreach (var e in enemies)
        {
            if (IsTargetValid(e))
            {
                candidates.Add(e);
            }
        }

        if (candidates.Count == 0)
        {
            currentTarget = null;
            return;
        }

        if (candidates.Count == 1)
        {
            currentTarget = candidates[0];
            targetAcquireTime = Time.time;
            currentComboHits = 0;
            return;
        }

        // If switching from an existing target, filter out the old target to ensure variety
        List<Transform> otherTargets = new List<Transform>(candidates);
        if (currentTarget != null && otherTargets.Count > 1)
        {
            otherTargets.Remove(currentTarget);
        }

        // Weighted Selection: 50% Random Variety, 30% Closest Threat, 20% Fragile Creep
        float roll = Random.value;

        if (roll < 0.40f)
        {
            // Random Target: Pick randomly among other enemies for organic spontaneity
            currentTarget = otherTargets[Random.Range(0, otherTargets.Count)];
        }
        else if (roll < 0.75f)
        {
            // Closest Enemy among candidates
            currentTarget = GetClosestEnemy(otherTargets);
        }
        else
        {
            // Priority Target (e.g. Creep or low HP)
            currentTarget = GetPriorityVulnerableEnemy(otherTargets);
        }

        targetAcquireTime = Time.time;
        currentComboHits = 0;
    }

    private Transform GetClosestEnemy(List<Transform> list)
    {
        Transform closest = list[0];
        float minD = float.MaxValue;
        Vector3 p = transform.position;

        foreach (var t in list)
        {
            float d = Vector3.Distance(p, t.position);
            if (d < minD)
            {
                minD = d;
                closest = t;
            }
        }
        return closest;
    }

    private Transform GetPriorityVulnerableEnemy(List<Transform> list)
    {
        Transform best = list[0];
        float bestScore = float.MinValue;
        Vector3 p = transform.position;

        foreach (var t in list)
        {
            float distScore = 1f - Mathf.Clamp01(Vector3.Distance(p, t.position) / sensorDetectionRadius);
            float hpScore = 0.5f;

            var c = t.GetComponentInParent<RL_EnemyController>();
            if (c != null)
            {
                hpScore = 1f - c.GetHealthPercentage();
                if (c.enemyType == EnemyType.Creep) hpScore += 0.35f;
            }

            float score = distScore * 0.5f + hpScore * 0.5f;
            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }
        return best;
    }
    #endregion

    #region Movement Application & Physics
    private void ApplyMovementVelocity(Vector3 velocity)
    {
        if (characterController != null && characterController.enabled)
        {
            Vector3 motion = (velocity + Vector3.down * 9.81f) * Time.fixedDeltaTime;
            characterController.Move(motion);
        }
        else if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
            }
            else
            {
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            }
        }
        else
        {
            transform.position += velocity * Time.fixedDeltaTime;
        }
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void UpdateAnimationMovement(bool isMoving)
    {
        if (animator == null) return;
        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isIdle", !isMoving);
    }

    private Vector3 CalculateWallRepulsion()
    {
        Vector3 repulsion = Vector3.zero;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Vector3[] dirs = new Vector3[]
        {
            transform.forward, -transform.forward, transform.right, -transform.right,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized,
            (-transform.forward + transform.right).normalized,
            (-transform.forward - transform.right).normalized
        };

        foreach (var dir in dirs)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, wallDetectionRange, obstacleLayers))
            {
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Player") ||
                    hit.collider.GetComponentInParent<RL_EnemyController>() != null ||
                    hit.collider.GetComponentInParent<RL_PlayerController>() != null)
                {
                    continue;
                }

                float frac = 1f - (hit.distance / wallDetectionRange);
                repulsion -= dir * frac * 2.5f;
            }
        }

        repulsion.y = 0;
        return repulsion;
    }
    #endregion

    #region Combat Actions
    private void DealDamageToEnemy(Transform enemy)
    {
        var enemyController = enemy.GetComponentInParent<RL_EnemyController>();
        if (enemyController != null)
        {
            enemyController.TakeDamage((int)attackDamage, transform.position);
        }
    }

    private void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("isAttacking");
            StartCoroutine(ResetAttackTriggerAfterFrame());
        }
    }

    private IEnumerator ResetAttackTriggerAfterFrame()
    {
        yield return null;
        if (animator != null)
            animator.ResetTrigger("isAttacking");
    }
    #endregion

    #region Public Interface (100% Backwards Compatible)
    public bool DamagePlayer(float damageAmount, System.Func<bool> knockbackCallback = null, Vector3 hitPosition = default)
    {
        if (!CanTakeDamage()) return false;

        currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0f, maxHealth);
        UpdateHealthBar();

        if (knockbackCallback != null)
        {
            knockbackCallback.Invoke();
        }

        if (currentHealth > 0f)
        {
            HandleNonFatalDamage();
            return false;
        }
        else
        {
            Die();
            return true;
        }
    }

    public void SetAutoAttackEnabled(bool enabled)
    {
        attackEnabled = enabled;
    }

    public void SetAttackRange(float newRange)
    {
        attackRange = Mathf.Max(0.1f, newRange);
    }

    public void ResetAttackRange()
    {
        attackRange = initialAttackRange;
    }

    public void SetDynamicMovementEnabled(bool enabled)
    {
        enableDynamicMovement = enabled;
    }

    public void Respawn()
    {
        isAlive = true;
        currentState = CombatTacticState.Hunting;
        currentTarget = null;
        currentComboHits = 0;
        currentHealth = maxHealth;
        transform.position = initialPosition;

        SetCollidersEnabled(true);
        UpdateHealthBar();
        InitializeAnimationState();
    }

    public List<Transform> FindNearbyEnemies(float radius)
    {
        var enemies = new List<Transform>();

        var allControllers = Object.FindObjectsByType<RL_EnemyController>(FindObjectsSortMode.None);
        foreach (var controller in allControllers)
        {
            if (controller != null && !controller.IsDead() && controller.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(transform.position, controller.transform.position);
                if (dist <= radius && !enemies.Contains(controller.transform))
                {
                    enemies.Add(controller.transform);
                }
            }
        }

        return enemies;
    }
    #endregion

    #region Health, Animation & Death Helpers
    private bool CanTakeDamage() => isAlive && !isInvincible;

    private void HandleNonFatalDamage()
    {
        PlayAnimationTrigger("getHit");
        PlayParticleEffect(hurtParticle);
        StartCoroutine(InvincibilityRoutine(invincibilityDuration));
    }

    private void Die()
    {
        RL_EvalEvents.RaiseEpisodeResult(true);
        isAlive = false;
        attackEnabled = false;
        currentState = CombatTacticState.Hunting;

        GetComponent<NormalEnemyAgent>()?.HandleKillPlayer();
        PlayAnimationBool("isDead", true);
        PlayParticleEffect(deathParticle);
        SetCollidersEnabled(false);
        NotifyDestruction();
        OnPlayerDestroyed?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    private void PlayAnimationTrigger(string triggerName)
    {
        if (animator != null)
            animator.SetTrigger(triggerName);
    }

    private void PlayAnimationBool(string parameterName, bool value)
    {
        if (animator != null)
            animator.SetBool(parameterName, value);
    }

    private void PlayParticleEffect(ParticleSystem particle)
    {
        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play();
        }
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (var collider in colliders)
        {
            if (collider != null)
                collider.enabled = enabled;
        }
    }

    private void NotifyDestruction()
    {
        var target = GetComponent<RL_TrainingPlayer>();
        target?.ForceNotifyDestruction();
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;
    }

    private void InitializeHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.minValue = 0;
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = maxHealth;
        }
    }

    private void InitializeAnimationState()
    {
        if (animator != null)
        {
            animator.SetBool("isIdle", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isDead", false);
            animator.ResetTrigger("getHit");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
    #endregion
}
