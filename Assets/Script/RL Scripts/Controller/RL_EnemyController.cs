using System;
using System.Collections;
using UnityEngine;

public class RL_EnemyController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Combat Configuration")]
    [SerializeField] private float fleeHealthThreshold = 0.2f;
    [SerializeField] private float fleeDistance = 8f;
    [SerializeField] private float fleeDetectionRadius = 10f;
    [SerializeField] private float fleeDuration = 3f;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private BoxCollider attackCollider;

    [Header("Movement Configuration")]
    [SerializeField] public Transform[] waypoints;
    [SerializeField] private float startWaitTime = 4f;

    [Header("Component References")]
    [SerializeField] private Animator animator;
    [SerializeField] private LootManager lootManager;
    [SerializeField] private VFXManager vfxManager;
    [SerializeField] private Transform particlePosition;
    [SerializeField] private EnemyType enemyType;
    #endregion

    #region Public Properties & Variables
    public EnemyData enemyData;
    public bool IsInitialized { get; private set; }
    public CombatState combatState;
    public HealthState healthState;
    public NormalEnemyActions.FleeState fleeState;
    public int enemyHP;
    public float attackRange = 2f;
    #endregion

    #region Private Variables
    private PlayerTrackingState playerTracking;
    private WaypointNavigationState waypointNavigation;
    private EnemyStatDisplay statDisplay;
    private Rigidbody rigidBody;
    private KnockbackState knockbackState;
    private NormalEnemyAgent agent;
    private PlayerController playerController; // Cache player reference

    private const float ATTACK_DURATION = 1f;
    private const float ATTACK_COOLDOWN = 2f;
    private const float KNOCKBACK_FORCE = 3f;
    private const float KNOCKBACK_DURATION = 0.3f;
    private const float DESTROY_DELAY = 8f;
    #endregion

    #region Unity Lifecycle
    private void Awake() => ForceInitialize();

    private void Update()
    {
        if (!IsInitialized) return;
        if (GetComponent<NormalEnemyAgent>()?.enabled == true) return;

        UpdateEnemyStates();
        HandleEnemyBehavior();
        UpdateAnimationStates();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayerCollider(other))
        {
            HandlePlayerEnterCombat(other);
            statDisplay?.ShowEnemyStats();
            return;
        }

        if (ShouldProcessWeaponDamage(other))
        {
            PlayerController player = GetPlayerFromCollider(other);
            if (player != null && player.playerData != null && player.canAttack)
            {
                TakeDamage(player.playerData.playerAttack, player.transform.position);
            }
        }
    }

    private bool ShouldProcessWeaponDamage(Collider other)
    {
        return (other.gameObject.layer == LayerMask.NameToLayer("Weapon")) ||
                (other.CompareTag("Player") &&
                other.gameObject.layer == LayerMask.NameToLayer("Hitbox") &&
                other.gameObject.layer == LayerMask.NameToLayer("Default"));
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayerCollider(other))
        {
            HandlePlayerExitCombat();
            statDisplay?.HideEnemyStats();
        }
    }

    private void OnEnable() => RL_Player.OnPlayerDestroyed += HandlePlayerDestroyed;
    private void OnDisable() => RL_Player.OnPlayerDestroyed -= HandlePlayerDestroyed;
    #endregion

    #region Initialization
    public void ForceInitialize()
    {
        if (IsInitialized) return;

        InitializeComponents();
        InitializeStates();
        SetupEnemyData();
        CachePlayerReference();
        SetAnimationState(idle: true);
        IsInitialized = true;
    }

    private void InitializeComponents()
    {
        statDisplay = GetComponent<EnemyStatDisplay>();
        rigidBody = GetComponent<Rigidbody>();
        agent = GetComponent<NormalEnemyAgent>();
    }

    private void InitializeStates()
    {
        playerTracking = new PlayerTrackingState();
        waypointNavigation = new WaypointNavigationState(waypoints, startWaitTime);
        combatState = new CombatState();
        healthState = new HealthState();
        knockbackState = new KnockbackState();
        fleeState = new NormalEnemyActions.FleeState();
    }

    private void SetupEnemyData()
    {
        enemyData ??= GetEnemyDataByType() ?? CreateDefaultEnemyData();
        enemyHP = enemyData.enemyHealth;
        InitializeHealthBar();
    }

    private void CachePlayerReference()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    private EnemyData GetEnemyDataByType()
    {
        return enemyType switch
        {
            EnemyType.Creep => CreepEnemyData.Instance,
            EnemyType.Medium1 => Medium1EnemyData.Instance,
            EnemyType.Medium2 => Medium2EnemyData.Instance,
            _ => null
        };
    }

    private EnemyData CreateDefaultEnemyData()
    {
        var defaultData = ScriptableObject.CreateInstance<EnemyData>();
        defaultData.enemyHealth = 100;
        defaultData.enemyAttack = 10;
        return defaultData;
    }

    public void ReinitializeData()
    {
        if (enemyData == null) SetupEnemyData();
    }

    public void InitializeHealthBar()
    {
        if (healthBar != null && enemyData != null)
        {
            healthBar.SetMaxHealth(enemyData.enemyHealth);
            healthBar.SetHealth(enemyHP);
        }
    }
    #endregion

    #region Update Methods
    private void UpdateEnemyStates()
    {
        knockbackState.UpdateKnockback();
        fleeState.UpdateTimer();
        UpdatePlayerTracking();
    }

    private void HandleEnemyBehavior()
    {
        if (fleeState.IsFleeing)
        {
            HandleFleeingBehavior();
        }
        else if (ShouldFlee())
        {
            InitiateFlee();
        }
        else if (!knockbackState.IsKnockedBack)
        {
            HandleCombatBehavior();
        }
    }
    #endregion

    #region Player Tracking
    private void UpdatePlayerTracking()
    {
        if (playerTracking.PlayerTransform == null)
        {
            playerTracking.SetInRange(false);
            waypointNavigation.SetPatrolling(true);
            SetAnimationState(walking: true);
        }
    }

    public void SetTarget(Transform target)
    {
        if (target == null || !playerTracking.IsPlayerAlive)
        {
            playerTracking.ClearTarget();
            return;
        }

        playerTracking.SetTarget(target);
        waypointNavigation.SetPatrolling(false);
    }

    private void HandlePlayerDestroyed() => playerTracking.HandlePlayerDestroyed();
    #endregion

    #region Combat System
    private void HandleCombatBehavior()
    {
        if (fleeState.IsFleeing || (knockbackState.IsKnockedBack && knockbackState.KnockbackTimer > KNOCKBACK_DURATION * 0.5f))
            return;

        if (CanEngagePlayer())
        {
            RotateTowardsTarget(playerTracking.PlayerPosition);

            if (combatState.CanAttack && !ShouldFlee())
                StartCoroutine(ExecuteAttackSequence());
        }
    }

    private bool CanEngagePlayer()
    {
        return playerTracking.IsInRange &&
               playerTracking.PlayerTransform != null &&
               playerTracking.IsPlayerAlive;
    }

    private void HandlePlayerEnterCombat(Collider playerCollider)
    {
        combatState.SetAttacking(true);
        combatState.SetCanAttack(true);

        // Cache the player reference for better performance
        if (playerController == null)
        {
            playerController = playerCollider.GetComponent<PlayerController>();
        }
    }

    private void HandlePlayerExitCombat() => combatState.SetAttacking(false);

    private IEnumerator ExecuteAttackSequence()
    {
        combatState.SetCanAttack(false);
        combatState.SetAttacking(true);

        EnableAttackCollider(true);
        SetAnimationState(attacking: true);

        yield return new WaitForSeconds(ATTACK_DURATION);

        EnableAttackCollider(false);
        SetAnimationState(attacking: false, idle: true);

        yield return new WaitForSeconds(ATTACK_COOLDOWN);

        combatState.SetAttacking(false);
        combatState.SetCanAttack(true);
    }

    private void EnableAttackCollider(bool enabled)
    {
        if (attackCollider != null)
            attackCollider.enabled = enabled;
    }

    public void AgentAttack()
    {
        if (combatState.CanAttack)
            StartCoroutine(ExecuteAttackSequence());
    }

    public void AttackEnd()
    {
        ExecuteAttackDamage();
        SetAnimationState(idle: true);
    }

    private void ExecuteAttackDamage()
    {
        if (attackCollider == null) return;

        Collider[] hitTargets = Physics.OverlapBox(
            attackCollider.bounds.center,
            attackCollider.bounds.extents * 1.2f, 
            attackCollider.transform.rotation,
            LayerMask.GetMask("Player", "Default", "Hitbox")); 

        bool playerHit = false;

        foreach (var target in hitTargets)
        {
            if (TryDamagePlayer(target))
            {
                playerHit = true;
                PlayAttackSound();
                break;
            }
        }

        if (!playerHit)
        {
            // Try sphere overlap as backup
            Collider[] sphereTargets = Physics.OverlapSphere(
                transform.position + transform.forward * (attackRange * 0.7f),
                attackRange,
                LayerMask.GetMask("Player", "Default", "Hitbox")
            );

            foreach (var target in sphereTargets)
            {
                if (TryDamagePlayer(target))
                {
                    PlayAttackSound();
                    break;
                }
            }
        }
    }

    private bool TryDamagePlayer(Collider target)
    {
        if (target == null) return false;

        // Try to find an RL_Player first (training player API)
        RL_Player rlPlayer = target.GetComponent<RL_Player>() 
                            ?? target.GetComponentInParent<RL_Player>() 
                            ?? target.GetComponentInChildren<RL_Player>();

        // Try to find a regular PlayerController
        PlayerController player = target.GetComponent<PlayerController>()
                                ?? target.GetComponentInParent<PlayerController>()
                                ?? target.GetComponentInChildren<PlayerController>();

        // If an RL_Player exists, prefer it and call its DamagePlayer signature
        if (rlPlayer != null)
        {
            // Ensure within attack distance (defensive)
            float distance = Vector3.Distance(transform.position, rlPlayer.transform.position);
            if (distance > attackRange * 1.2f) return false;

            float dmg = enemyData != null ? enemyData.enemyAttack : 0f;

            // RL_Player expects Func<bool> for knockback callback: start coroutine and return true
            System.Func<bool> rlKnockback = () =>
            {
                StartCoroutine(CreateKnockbackCoroutine(rlPlayer.transform.position));
                return true;
            };

            rlPlayer.DamagePlayer(dmg, rlKnockback, transform.position);
            return true;
        }

        // If no RL_Player, handle PlayerController safely
        if (player != null)
        {
            // If this PlayerController is a runtime RL clone without playerData, avoid calling it.
            if (player.playerData == null)
            {
                // Prefer a cached playerController if available and valid
                if (playerController != null && playerController.playerData != null)
                {
                    player = playerController;
                }
                else
                {
                    // Nothing valid to damage here
                    return false;
                }
            }

            if (!player.isAlive) return false;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > attackRange * 1.2f) return false;

            int dmg = enemyData != null ? Mathf.RoundToInt(enemyData.enemyAttack) : 0;

            // PlayerController expects Func<IEnumerator> knockback coroutine
            System.Func<IEnumerator> pcKnockback = () => CreateKnockbackCoroutine(player.transform.position);

            player.DamagePlayer(dmg, pcKnockback, transform.position);
            return true;
        }

        // No usable player found on the collider
        return false;
    }

    private IEnumerator CreateSafeKnockbackCoroutine(Vector3 playerPosition)
    {
        try
        {
            // Simple knockback effect on enemy
            Vector3 knockbackDir = (transform.position - playerPosition).normalized;
            if (knockbackDir.sqrMagnitude > 0.01f) // Check for valid direction
            {
                ApplyKnockback(knockbackDir * 0.5f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Knockback coroutine error: {e.Message}");
        }
        
        yield return new WaitForSeconds(0.2f);
    }


    private PlayerController GetPlayerFromCollider(Collider collider)
    {
        // First try direct component
        PlayerController player = collider.GetComponent<PlayerController>();
        if (player != null) return player;

        // Try RL_Player component
        RL_Player rlPlayer = collider.GetComponent<RL_Player>();
        if (rlPlayer != null) return rlPlayer.GetComponent<PlayerController>();

        // Try parent if it's a child collider
        if (collider.transform.parent != null)
        {
            player = collider.transform.parent.GetComponent<PlayerController>();
            if (player != null) return player;

            rlPlayer = collider.transform.parent.GetComponent<RL_Player>();
            if (rlPlayer != null) return rlPlayer.GetComponent<PlayerController>();
        }

        // Use cached reference as fallback
        if (playerController != null &&
            (collider.CompareTag("Player") || collider.name.Contains("Player")))
        {
            return playerController;
        }

        return null;
    }

    private IEnumerator CreateKnockbackCoroutine(Vector3 playerPosition)
    {
        // Simple knockback effect on enemy (optional)
        Vector3 knockbackDir = (transform.position - playerPosition).normalized;
        ApplyKnockback(knockbackDir * 0.5f); // Reduced knockback on successful parry
        yield return new WaitForSeconds(0.2f);
    }
    #endregion

    #region Flee System
    private bool ShouldFlee()
    {
        return IsHealthLow() &&
               playerTracking.IsPlayerAlive &&
               Vector3.Distance(transform.position, playerTracking.PlayerPosition) <= fleeDetectionRadius;
    }

    private void HandleFleeingBehavior()
    {
        if (!playerTracking.IsPlayerAlive || playerTracking.PlayerTransform == null)
        {
            StopFleeing();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTracking.PlayerPosition);

        if (ShouldStopFleeing(distanceToPlayer))
        {
            StopFleeing();
        }
    }

    private bool ShouldStopFleeing(float distanceToPlayer)
    {
        return distanceToPlayer >= fleeDistance ||
               fleeState.FleeTimer >= fleeDuration ||
               !IsHealthLow();
    }

    private void StopFleeing()
    {
        fleeState.StopFleeing();
        waypointNavigation.SetPatrolling(true);
        combatState.SetCanAttack(true);
    }

    private void InitiateFlee()
    {
        Vector3 fleeDirection = CalculateFleeDirection();
        fleeState.StartFleeing(fleeDirection);
        waypointNavigation.SetPatrolling(false);
        combatState.SetCanAttack(false);
    }

    private Vector3 CalculateFleeDirection()
    {
        Vector3 playerDirection = (transform.position - playerTracking.PlayerPosition).normalized;
        Vector3 fleeDirection = playerDirection;

        fleeDirection += UnityEngine.Random.insideUnitSphere * 0.3f;
        fleeDirection.y = 0;
        return fleeDirection.normalized;
    }
    #endregion

    #region Damage System
    public void TakeDamage(int damageAmount, Vector3 attackerPosition = default)
    {
        if (healthState.IsDead) return;

        enemyHP = Mathf.Max(enemyHP - damageAmount, 0);
        GetComponent<NormalEnemyAgent>()?.HandleDamage();
        UpdateHealthBar();

        if (enemyHP > 0)
        {
            HandleDamageReaction(attackerPosition);
        }
        else
        {
            HandleDeath();
        }
    }

    private void HandleDamageReaction(Vector3 attackerPosition)
    {
        PlayHitAnimation();
        PlayHitSound();
        CreateHitEffect();
        ApplyKnockbackFromAttacker(attackerPosition);
        ReactToPlayerAttack();
    }

    private void ApplyKnockbackFromAttacker(Vector3 attackerPosition)
    {
        if (attackerPosition != Vector3.zero)
        {
            Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
            knockbackDirection.y = 0;
            ApplyKnockback(knockbackDirection);
        }
    }

    public void ApplyKnockback(Vector3 direction)
    {
        direction.y = 0;
        direction = direction.normalized;

        knockbackState.ApplyKnockback(direction, KNOCKBACK_DURATION);

        if (rigidBody != null)
        {
            rigidBody.AddForce(direction * KNOCKBACK_FORCE, ForceMode.VelocityChange);
        }

        StartCoroutine(ExecuteKnockbackMovement(direction));
    }

    public void OnKnockbackReceived(Vector3 source)
    {
        Vector3 knockbackDirection = (transform.position - source).normalized;
        ApplyKnockback(knockbackDirection);
    }

    private void ReactToPlayerAttack()
    {
        if (healthState.IsDead) return;

        playerTracking.SetInRange(true);
        waypointNavigation.SetPatrolling(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerTracking.SetPlayerPosition(playerController.transform.position);
                playerTracking.SetTarget(playerController.transform);
                RotateTowardsTarget(playerTracking.PlayerPosition);
            }
        }

        combatState.SetCanAttack(true);
    }
    #endregion

    #region Movement
    private IEnumerator ExecuteKnockbackMovement(Vector3 direction)
    {
        float elapsed = 0f;
        Vector3 initialVelocity = rigidBody.linearVelocity;

        while (elapsed < KNOCKBACK_DURATION && knockbackState.IsKnockedBack)
        {
            float t = elapsed / KNOCKBACK_DURATION;
            float knockbackInfluence = Mathf.Lerp(1f, 0f, t);

            Vector3 knockbackVelocity = direction * KNOCKBACK_FORCE * knockbackInfluence;
            Vector3 currentVelocity = rigidBody.linearVelocity;
            rigidBody.linearVelocity = new Vector3(
                knockbackVelocity.x,
                currentVelocity.y,
                knockbackVelocity.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        knockbackState.ClearKnockback();
    }

    private void RotateTowardsTarget(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0f; 
        
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            float rotSpeed = (agent != null) ? agent.rotationSpeed : 120f;
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotSpeed * Time.deltaTime
            );
        }
    }
    #endregion

    #region Animation & Visuals
    private void UpdateAnimationStates()
    {
        if (combatState.IsAttacking && combatState.CanAttack) return;

        if (playerTracking.IsInRange)
            RotateTowardsTarget(playerTracking.PlayerPosition);
    }

    private void SetAnimationState(bool idle = false, bool walking = false, bool attacking = false, bool dead = false)
    {
        if (animator == null) return;

        animator.SetBool("isIdle", idle);
        animator.SetBool("isWalking", walking);
        animator.SetBool("isAttacking", attacking);
        animator.SetBool("isDead", dead);
    }

    private void PlayHitAnimation() => animator?.SetTrigger("getHit");
    private void UpdateHealthBar() => healthBar?.SetHealth(enemyHP);
    public void ShowHealthBar() => healthBar?.gameObject.SetActive(true);
    #endregion

    #region Death & Loot
    private void HandleDeath()
    {
        healthState.SetDead(true);
        SetAnimationState(dead: true);
        PlayDeathSound();

        GetComponent<Collider>().enabled = false;
        healthBar?.gameObject.SetActive(false);

        NotifyGameProgression();
        SpawnLoot();
        HandleAgentDeath();
    }

    private void HandleAgentDeath()
    {
        var agent = GetComponent<NormalEnemyAgent>();
        if (agent != null)
        {
            agent.HandleEnemyDeath();
        }
        else
        {
            Destroy(gameObject, DESTROY_DELAY);
        }
    }

    private void SpawnLoot()
    {
        lootManager?.SpawnGearLoot(transform);
    }
    #endregion

    #region Audio & VFX
    private void PlayHitSound() => AudioManager.instance?.PlayEnemyGetHitSound(enemyType);
    private void PlayDeathSound() => AudioManager.instance?.PlayEnemyDieSound(enemyType);
    private void PlayAttackSound() => AudioManager.instance?.PlayEnemyAttackSound(enemyType);
    private void CreateHitEffect() => vfxManager?.EnemyGettingHit(particlePosition, enemyType);
    #endregion

    #region Utility Methods
    private bool IsPlayerCollider(Collider collider)
    {
        return (collider.CompareTag("Player") &&
                (collider.gameObject.layer == LayerMask.NameToLayer("Hitbox") ||
                 collider.gameObject.layer == LayerMask.NameToLayer("Default"))) ||
               collider.name.Contains("Player");
    }

    private void NotifyGameProgression() => GameProgression.Instance?.EnemyKill();

    // Public getters for ML Agent
    public float GetHealthPercentage() => (float)enemyHP / enemyData.enemyHealth;
    public bool IsHealthLow() => enemyHP <= enemyData.enemyHealth * fleeHealthThreshold;
    public bool IsFleeing() => fleeState.IsFleeing;
    public bool IsKnockedBack() => knockbackState.IsKnockedBack;
    public bool IsDead() => healthState.IsDead;
    public float GetDistanceToCurrentWaypoint() => waypointNavigation.GetDistanceToCurrentWaypoint(transform.position);
    public Vector3 GetWaypointDirection() => waypointNavigation.GetDirectionToCurrentWaypoint(transform.position);
    #endregion

    public class KnockbackState
    {
        public bool IsKnockedBack { get; private set; }
        public float KnockbackTimer { get; private set; }
        private float knockbackDuration;

        public void ApplyKnockback(Vector3 direction, float duration)
        {
            IsKnockedBack = true;
            knockbackDuration = duration;
            KnockbackTimer = 0f;
        }

        public void UpdateKnockback()
        {
            if (IsKnockedBack)
            {
                KnockbackTimer += Time.deltaTime;
                if (KnockbackTimer >= knockbackDuration)
                {
                    ClearKnockback();
                }
            }
        }

        public void ClearKnockback()
        {
            IsKnockedBack = false;
            KnockbackTimer = 0f;
        }
    }
}

