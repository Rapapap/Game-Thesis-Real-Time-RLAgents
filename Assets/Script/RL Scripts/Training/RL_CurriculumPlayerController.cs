using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Curriculum-aware player controller for RL training.
/// 
/// Reads the "player_difficulty" environment parameter from ML-Agents Academy
/// and adjusts player behavior accordingly:
/// 
///   Stage 1 (0.0) — Static:          No movement, attacks only at very close range (1.5m)
///   Stage 2 (1.0) — Passive Mobile:  Random wandering, attacks at normal range
///   Stage 3 (2.0) — Defensive:       Retreats when agent is close, attacks, respawns on death
///   Stage 4 (3.0) — Aggressive:      Moves toward nearest agent, attacks frequently, respawns on death
/// 
/// Attach this component to the training player prefab ALONGSIDE RL_PlayerController.
/// This script controls MOVEMENT only; combat is handled by RL_PlayerController's auto-attack.
/// </summary>
[RequireComponent(typeof(RL_PlayerController))]
public class RL_CurriculumPlayerController : MonoBehaviour
{
    public enum PlayerDifficultyStage
    {
        Static = 0,
        PassiveMobile = 1,
        Defensive = 2,
        Aggressive = 3
    }

    [Header("Curriculum Configuration")]
    [Tooltip("Enable curriculum control. When FALSE (default), full control is handled dynamically by RL_PlayerController.")]
    [SerializeField] private bool enableCurriculumControl = false;
    [Tooltip("Override the curriculum stage manually (for testing). Set to -1 to use Academy parameter, or 3 for Aggressive.")]
    [SerializeField] private int manualStageOverride = 3;

    [Header("Movement Settings")]
    [SerializeField] private float wanderSpeed = 2.5f;
    [SerializeField] private float aggressiveSpeed = 4.0f;
    [SerializeField] private float retreatSpeed = 3.5f;
    [SerializeField] private float wanderDirectionChangeInterval = 2f;
    [SerializeField] private float wanderRadius = 5f;

    [Header("Behavior Thresholds")]
    [SerializeField] private float fleeRadius = 4f;
    [SerializeField] private float aggressiveApproachRange = 25f;
    [SerializeField] private float aggressiveStopRange = 2f;

    [Header("Attack Range Overrides")]
    [Tooltip("Attack range for Stage 1 (Static). Very close range only.")]
    [SerializeField] private float staticAttackRange = 1.5f;
    [Tooltip("Attack range for Stage 2+ (normal).")]
    [SerializeField] private float normalAttackRange = 5f;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 2f;

    [Header("Arena Bounds")]
    [SerializeField] private float arenaPadding = 1f;

    [Header("Debug")]
    [SerializeField] private bool debugCurriculum = true;

    // Components
    private RL_PlayerController playerController;
    private Rigidbody rb;
    private Animator animator;

    // State
    private PlayerDifficultyStage currentStage = PlayerDifficultyStage.Static;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private Vector3 spawnPosition;
    private Vector3 arenaMin;
    private Vector3 arenaMax;
    private bool arenaBoundsSet;
    private float lastDifficultyCheck;
    private const float DifficultyCheckInterval = 1f; // Check curriculum parameter every 1 second

    private void Awake()
    {
        playerController = GetComponent<RL_PlayerController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;
    }

    private void Start()
    {
        UpdateDifficultyStage();
        ApplyStageSettings();
    }

    private void FixedUpdate()
    {
        if (!enableCurriculumControl) return;

        // Periodically check if the curriculum stage has changed
        if (Time.time - lastDifficultyCheck > DifficultyCheckInterval)
        {
            UpdateDifficultyStage();
            lastDifficultyCheck = Time.time;
        }

        // Execute behavior based on current stage
        switch (currentStage)
        {
            case PlayerDifficultyStage.Static:
                ExecuteStaticBehavior();
                break;
            case PlayerDifficultyStage.PassiveMobile:
                ExecutePassiveMobileBehavior();
                break;
            case PlayerDifficultyStage.Defensive:
                ExecuteDefensiveBehavior();
                break;
            case PlayerDifficultyStage.Aggressive:
                ExecuteAggressiveBehavior();
                break;
        }
    }

    #region Public Interface

    /// <summary>
    /// Set the arena bounds so the player stays within the training area.
    /// Called by the spawner or training manager after instantiation.
    /// </summary>
    public void SetArenaBounds(Vector3 min, Vector3 max)
    {
        arenaMin = min + Vector3.one * arenaPadding;
        arenaMax = max - Vector3.one * arenaPadding;
        arenaBoundsSet = true;
    }

    /// <summary>
    /// Get the current curriculum stage for external queries.
    /// </summary>
    public PlayerDifficultyStage GetCurrentStage() => currentStage;

    /// <summary>
    /// Force a specific difficulty stage (for testing or manual control).
    /// </summary>
    public void SetDifficultyStage(PlayerDifficultyStage stage)
    {
        if (currentStage != stage)
        {
            currentStage = stage;
            ApplyStageSettings();
            LogDebug($"Difficulty stage manually set to: {stage}");
        }
    }

    #endregion

    #region Curriculum Parameter Reading

    private void UpdateDifficultyStage()
    {
        PlayerDifficultyStage newStage;

        if (manualStageOverride >= 0)
        {
            newStage = (PlayerDifficultyStage)Mathf.Clamp(manualStageOverride, 0, 3);
        }
        else
        {
            // Read from ML-Agents Academy environment parameter
            float difficulty = 0f;
            if (Academy.IsInitialized)
            {
                difficulty = Academy.Instance.EnvironmentParameters
                    .GetWithDefault("player_difficulty", 0f);
            }
            newStage = (PlayerDifficultyStage)Mathf.Clamp(Mathf.FloorToInt(difficulty), 0, 3);
        }

        if (newStage != currentStage)
        {
            PlayerDifficultyStage previousStage = currentStage;
            currentStage = newStage;
            ApplyStageSettings();
            LogDebug($"Curriculum stage changed: {previousStage} -> {currentStage}");
        }
    }

    private void ApplyStageSettings()
    {
        // Adjust the RL_PlayerController's combat behavior based on stage.
        if (playerController != null)
        {
            playerController.SetAutoAttackEnabled(true);

            switch (currentStage)
            {
                case PlayerDifficultyStage.Static:
                    playerController.SetAttackRange(staticAttackRange);
                    break;

                case PlayerDifficultyStage.PassiveMobile:
                case PlayerDifficultyStage.Defensive:
                case PlayerDifficultyStage.Aggressive:
                    playerController.SetAttackRange(normalAttackRange);
                    break;
            }
        }
    }

    #endregion

    #region Stage Behaviors

    private void ExecuteStaticBehavior()
    {
        // Stage 1: No movement at all — player stands still
        StopMovement();
        UpdateAnimation(false);
    }

    private void ExecutePassiveMobileBehavior()
    {
        // Stage 2: Wander randomly within the arena
        wanderTimer -= Time.fixedDeltaTime;

        if (wanderTimer <= 0f || HasReachedTarget())
        {
            PickNewWanderTarget();
            wanderTimer = wanderDirectionChangeInterval;
        }

        MoveToward(wanderTarget, wanderSpeed);
        UpdateAnimation(true);
    }

    private void ExecuteDefensiveBehavior()
    {
        // Stage 3: Retreat when enemy is close, otherwise wander
        Transform nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, nearestEnemy.position);

            if (distanceToEnemy < fleeRadius)
            {
                // Retreat: move away from the nearest enemy
                Vector3 retreatDirection = (transform.position - nearestEnemy.position).normalized;
                Vector3 retreatTarget = transform.position + retreatDirection * fleeRadius;
                retreatTarget = ClampToArena(retreatTarget);

                MoveToward(retreatTarget, retreatSpeed);
                FaceDirection(-retreatDirection); // Face the enemy while retreating
                UpdateAnimation(true);
                return;
            }
        }

        // No enemy nearby: wander
        ExecutePassiveMobileBehavior();
    }

    private void ExecuteAggressiveBehavior()
    {
        Transform nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, nearestEnemy.position);
            bool isLowHealth = playerController != null && (playerController.CurrentHealth / 100f < 0.35f);

            // Tactical Retreat: If health is low (< 35%), run away from the enemy!
            if (isLowHealth)
            {
                Vector3 retreatDirection = (transform.position - nearestEnemy.position).normalized;
                Vector3 retreatTarget = transform.position + retreatDirection * fleeRadius;
                retreatTarget = ClampToArena(retreatTarget);

                MoveToward(retreatTarget, retreatSpeed);
                UpdateAnimation(true);
                return;
            }

            // Chasing: Pursue enemy if further than 2.2m
            if (distanceToEnemy > 2.2f && distanceToEnemy < aggressiveApproachRange)
            {
                MoveToward(nearestEnemy.position, aggressiveSpeed);
                UpdateAnimation(true);
                return;
            }
            // Melee engagement: Dynamic strafe movement around enemy (like a real player) instead of standing frozen!
            else if (distanceToEnemy <= 2.2f)
            {
                Vector3 dirToEnemy = (nearestEnemy.position - transform.position).normalized;
                Vector3 strafeDir = Vector3.Cross(dirToEnemy, Vector3.up).normalized;
                float strafeSide = (Mathf.PingPong(Time.time, 2f) > 1f) ? 1f : -1f;
                Vector3 strafeTarget = transform.position + (strafeDir * strafeSide * 1.5f);
                strafeTarget = ClampToArena(strafeTarget);

                MoveToward(strafeTarget, 2.0f);
                FaceDirection(dirToEnemy);
                UpdateAnimation(true);
                return;
            }
        }

        // No enemy in range: wander to find one
        ExecutePassiveMobileBehavior();
    }

    #endregion

    #region Movement Helpers

    private void MoveToward(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.05f)
        {
            StopMovement();
            return;
        }

        direction = direction.normalized;
        FaceDirection(direction);

        Vector3 displacement = direction * speed * Time.fixedDeltaTime;
        
        if (rb != null && !rb.isKinematic)
        {
            Vector3 velocity = direction * speed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }
        else if (rb != null && rb.isKinematic)
        {
            rb.MovePosition(transform.position + displacement);
        }
        
        // Direct transform displacement fallback to guarantee non-frozen movement
        transform.position = ClampToArena(transform.position + displacement);
    }

    /// <summary>
    /// Safely stops movement regardless of whether the Rigidbody is kinematic or dynamic.
    /// </summary>
    private void StopMovement()
    {
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
        }
        // Kinematic bodies don't need velocity reset — they don't have velocity
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * 10f
            );
        }
    }

    private void PickNewWanderTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = spawnPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
        wanderTarget = ClampToArena(wanderTarget);
    }

    private bool HasReachedTarget()
    {
        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(wanderTarget.x, 0, wanderTarget.z)
        );
        return distance < 0.5f;
    }

    private Vector3 ClampToArena(Vector3 position)
    {
        if (!arenaBoundsSet || arenaMin.x >= arenaMax.x || arenaMin.z >= arenaMax.z) return position;

        position.x = Mathf.Clamp(position.x, arenaMin.x, arenaMax.x);
        position.z = Mathf.Clamp(position.z, arenaMin.z, arenaMax.z);
        return position;
    }

    #endregion

    #region Enemy Detection

    private Transform FindNearestEnemy()
    {
        float closestDistance = float.MaxValue;
        Transform closest = null;

        // Find all active enemy agents
        var enemies = FindObjectsByType<NormalEnemyAgent>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDead)
                continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = enemy.transform;
            }
        }

        // Fallback: search by RL_EnemyController
        if (closest == null)
        {
            var controllers = FindObjectsByType<RL_EnemyController>(FindObjectsSortMode.None);
            foreach (var ctrl in controllers)
            {
                if (ctrl == null || !ctrl.isActiveAndEnabled || ctrl.IsDead())
                    continue;

                float dist = Vector3.Distance(transform.position, ctrl.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = ctrl.transform;
                }
            }
        }

        return closest;
    }

    #endregion

    #region Animation

    private void UpdateAnimation(bool isMoving)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isIdle", !isMoving);
    }

    #endregion

    #region Debug

    private void LogDebug(string message)
    {
        if (debugCurriculum)
            Debug.Log($"[CurriculumPlayer] {message}");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw flee radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);

        // Draw aggressive approach range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggressiveApproachRange);

        // Draw wander target
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wanderTarget, 0.3f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }

        // Draw arena bounds
        if (arenaBoundsSet)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = (arenaMin + arenaMax) / 2f;
            Vector3 size = arenaMax - arenaMin;
            Gizmos.DrawWireCube(center, size);
        }
    }

    #endregion
}
