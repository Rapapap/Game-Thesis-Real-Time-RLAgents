using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    #region Singleton
    public static PlayerController Instance { get; private set; }
    #endregion

    #region Components
    [Header("Components")]
    public PlayerData playerData;
    public WeaponData weaponData;
    public Animator Animator;
    
    private GameManager gm;
    private CharacterController controller;
    #endregion

    #region Movement Variables
    [Header("Movement")]
    [SerializeField] private float smoothInputSpeed = 0.1f;
    [SerializeField] private float gravityValue = -9.81f;
    
    private Vector3 playerVelocity;
    private Vector3 move;
    public Vector3 currentInputVector;
    private Vector3 smoothInputVelocity;
    private bool groundedPlayer;
    public float velocity;
    #endregion

    #region Combat Variables
    [Header("Combat")]
    public float AssistRange;
    public int dashCount = 50;
    public int MaxHealth;
    public int SkillEnergy = 2;
    public bool canAttack = true;

    public bool canParry = true;
    public bool CanSkillAttack = true;
    public bool canDash = true;
    private bool isInvincible = false;
    #endregion

    #region State Variables
    [Header("State")]
    public PlayerState playerState;
    public int Gear;
    public int Data;
    public bool isAlive = true;
    public bool HasTeleported = false;
    public bool KILLPLAYER = false;
    
    private bool isEnemyDetected = false;
    private Transform enemyTransform = null;
    #endregion

    #region Particles
    [Header("Particles")]
    public Transform ParticleSpawnPoint;
    public ParticleSystem DashParticle;
    public ParticleSystem DeathParticle;
    public ParticleSystem HurtParticle;
    public ParticleSystem ParrySuccessParticle;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        InitializeComponents();
        SetupCursor();
    }

    private void Update()
    {
        HandleDebugKill();
        HandleGravity();
        HandleCursorToggle();
        
        if (ShouldProcessInput())
        {
            HandleCombatAssists();
            HandleMovementInput();
            HandleCombatInput();
            HandleHealthCheck();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldDetectEnemy(other))
        {
            enemyTransform = other.transform;
            isEnemyDetected = true;
        }
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        Data = 0;
        gm = GameManager.Instance;
        Gear = gm.LoadGear();
        controller = GetComponent<CharacterController>();
        playerState = PlayerState.Idle;
        MaxHealth = playerData.playerHealth;
    }

    private void SetupCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion

    #region Input Handling
    private bool ShouldProcessInput()
    {
        Animator.SetBool("IsAlive", isAlive);
        return isAlive && playerState != PlayerState.Interact && HasTeleported;
    }

    private void HandleDebugKill()
    {
        if (KILLPLAYER && playerData.playerHealth != 0)
            playerData.playerHealth = 0;
    }

    private void HandleGravity()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    private void HandleMovementInput()
    {
        move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        currentInputVector = Vector3.SmoothDamp(currentInputVector, move, ref smoothInputVelocity, smoothInputSpeed);

        if (CanMove())
        {
            UpdateAnimationSpeed();
            if (CanProcessMovement())
                ProcessMovement();
        }
    }

    private void HandleCombatInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && CanDash())
            StartDash();

        if (Input.GetMouseButtonDown(0) && CanAttack())
            StartAttack();

        if (Input.GetKeyDown(KeyCode.Space) && CanUseSkill())
            UseSkillAttack();

        HandleParryInput();
    }

    private void HandleParryInput()
    {
        if (Input.GetMouseButtonDown(1) && canParry && !Cursor.visible)
            Animator.SetBool("Parry", true);

        if (Input.GetMouseButtonUp(1) && !Cursor.visible)
            Animator.SetBool("Parry", false);
    }

    private void HandleHealthCheck()
    {
        if (playerData.playerHealth <= 0 && isAlive)
            OnDeath();
    }
    #endregion

    #region Movement System
    private bool CanMove()
    {
        return playerState != PlayerState.Parry && 
               playerState != PlayerState.SkillAttack && 
               playerState != PlayerState.Hurt;
    }

    private bool CanProcessMovement()
    {
        return !IsInCombatState();
    }

    private bool IsInCombatState()
    {
        return playerState == PlayerState.Attack1 || 
               playerState == PlayerState.Attack2 || 
               playerState == PlayerState.Attack3 || 
               playerState == PlayerState.DashAttack || 
               playerState == PlayerState.Dash;
    }

    private void UpdateAnimationSpeed()
    {
        velocity = Mathf.Clamp01(Mathf.Abs(move.x) + Mathf.Abs(move.z));
        Animator.SetFloat("Speed", velocity);
    }

    private void ProcessMovement()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0) 
            playerVelocity.y = 0f;

        if (move != Vector3.zero)
        {
            MovePlayer();
            RotatePlayer();
            playerState = PlayerState.Run;
        }
        else if (!IsInCombatState())
        {
            playerState = PlayerState.Idle;
        }
    }

    private void MovePlayer()
    {
        controller.Move(currentInputVector * Time.deltaTime * playerData.playerSpeed);
    }

    private void RotatePlayer()
    {
        if (enemyTransform == null && !isEnemyDetected)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10);
        }
    }
    #endregion

    #region Combat System
    private bool CanDash()
    {
        return canDash && enemyTransform == null && !isEnemyDetected && playerState != PlayerState.SkillAttack;
    }

    private bool CanAttack()
    {
        return canAttack && playerState != PlayerState.Parry && !Cursor.visible;
    }

    private bool CanUseSkill()
    {
        return CanSkillAttack && playerState != PlayerState.SkillAttack && SkillEnergy >= 2;
    }

    private void StartDash()
    {
        Animator.SetTrigger("Dash");
        StartCoroutine(ExecuteDash());
        canDash = false;
        StartCoroutine(EnableInvincibilityFrames(playerData.playerDashTime));
    }

    private void StartAttack()
    {
        StartCoroutine(ExecuteAttack());
        canAttack = false;
        StartCoroutine(ResetAttack());
    }

    private void UseSkillAttack()
    {
        Animator.SetTrigger("Skill");
        SkillEnergy -= 2;
        CanSkillAttack = false;
    }

    private IEnumerator ExecuteAttack()
    {
        Animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.2f);
        canAttack = true;
    }

    private IEnumerator ResetParry()
    {
        yield return new WaitForSeconds(2f);
        canParry = true;
    }

    private IEnumerator ExecuteDash()
    {
        float startTime = Time.time;
        SpawnDashParticle();
        
        while (Time.time < startTime + playerData.playerDashTime)
        {
            dashCount = 0;
            if (enemyTransform == null && !isEnemyDetected)
            {
                controller.Move(playerData.playerDashSpeed * Time.deltaTime * transform.forward);
                yield return null;
            }
            else
            {
                StartCoroutine(DashCooldown());
                yield break;
            }
        }
        StartCoroutine(DashCooldown());
    }

    private void SpawnDashParticle()
    {
        Vector3 particleRotation = new Vector3(
            transform.rotation.eulerAngles.x, 
            transform.rotation.eulerAngles.y + 180, 
            transform.rotation.eulerAngles.z
        );
        Instantiate(DashParticle, ParticleSpawnPoint.position, Quaternion.Euler(particleRotation), transform);
    }

    private IEnumerator DashCooldown()
    {
        float startTime = Time.time;
        while (Time.time < startTime + 2)
        {
            float elapsedTime = Time.time - startTime;
            dashCount = Mathf.Clamp((int)(elapsedTime / 2 * 50), 0, 50);
            yield return null;
        }
        canDash = true;
    }
    #endregion

    #region Invincibility System
    private IEnumerator EnableInvincibilityFrames(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    private IEnumerator StandardInvincibilityFrames()
    {
        yield return EnableInvincibilityFrames(0.5f);
    }
    #endregion

    #region Combat Assists
    private bool ShouldDetectEnemy(Collider other)
    {
        return other.CompareTag("Enemy") && 
               other.gameObject.layer == LayerMask.NameToLayer("Default") && 
               !isEnemyDetected && 
               enemyTransform == null;
    }

    private void HandleCombatAssists()
    {
        if (!isEnemyDetected || enemyTransform == null) return;

        if (IsEnemyDead())
        {
            ResetEnemyDetection();
            return;
        }

        ProcessEnemyAssist();
    }

    private bool IsEnemyDead()
    {
        var enemyController = enemyTransform.GetComponent<EnemyController>();
        var rlEnemyController = enemyTransform.GetComponent<RL_EnemyController>();
        
        return (enemyController?.enemyHP <= 0) || (rlEnemyController?.enemyHP <= 0);
    }

    private void ProcessEnemyAssist()
    {
        Vector3 direction = enemyTransform.position - transform.position;
        direction.y = 0;

        if (direction.magnitude <= AssistRange)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        }
        else
        {
            ResetEnemyDetection();
        }
    }

    private void ResetEnemyDetection()
    {
        enemyTransform = null;
        isEnemyDetected = false;
    }
    #endregion

    #region Damage System
    public void DamagePlayer(int damage, System.Func<IEnumerator> knockback, Vector3 position)
    {
        if (playerData == null)
        {
            Debug.LogWarning($"DamagePlayer called but playerData is null on '{gameObject.name}'");
            return;
        }

        if (isInvincible) return;

        if (TryParry(position, knockback)) return;

        ApplyDamage(damage);
        StartCoroutine(StandardInvincibilityFrames());
    }

    private bool TryParry(Vector3 position, System.Func<IEnumerator> knockback)
    {
        Vector3 directionToEnemy = position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToEnemy);

        if (angle < 90f && playerState == PlayerState.Parry)
        {
            ExecuteSuccessfulParry(knockback);
            return true;
        }
        return false;
    }

    private void ExecuteSuccessfulParry(System.Func<IEnumerator> knockback)
    {
        Animator?.SetTrigger("ParrySuccess");
        canParry = false;
        ParrySuccessParticle?.Play();

        if (knockback != null)
            StartCoroutine(knockback());

        StartCoroutine(ResetParry());
    }

    private void ApplyDamage(int damage)
    {
        playerData.playerHealth -= damage;

        if (isAlive && !IsInImmuneState())
        {
            Animator?.SetTrigger("Hurt");
            HurtParticle?.Play();
        }
    }

    private bool IsInImmuneState()
    {
        return playerState == PlayerState.SkillAttack ||
               playerState == PlayerState.Dash ||
               IsInCombatState();
    }
    #endregion

    #region Death System
    private void OnDeath()
    {
        GameTimer.Instance.Stop();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isAlive = false;
        Animator.SetTrigger("Death");
        Destroy(gameObject, 6);
    }
    #endregion

    #region Resource Management
    public void AddDataResource()
    {
        Data++;
        SkillEnergy++;
    }

    public void AddGearResource()
    {
        Gear++;
        SkillEnergy++;
    }

    public void AddHealth()
    {
        playerData.playerHealth = Mathf.Min(playerData.playerHealth + 10, MaxHealth);
        SkillEnergy++;
    }
    #endregion
}