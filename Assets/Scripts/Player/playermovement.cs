using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }
    
    public static Vector3 lastCheckpointPosition;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private float normalMoveSpeed;
    private Vector2 moveInput;
    public bool isFacingRight = true;
    public float chargingWalkSpeed = 2f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 0.5f;
    public float dashAnimationLength = 0.6f;
    private bool isDashing = false;
    private bool canDash = true;
    private bool hasAirDashed = false;
    private Coroutine dashCoroutine;

    [Header("Jump")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded = false;
    public int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Wall Interaction")]
    public Transform wallCheck;
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.5f;
    private bool isWallSliding = false;
    public Vector2 wallJumpingForce = new Vector2(10f, 18f);
    public float wallJumpingDuration = 0.4f;
    private bool isWallJumping = false;

    [Header("Attack Combo")]
    private bool isAttacking = false;
    public float attack1Duration = 0.2f;
    public float attack2Duration = 0.2f;
    public float comboResetDelay = 0.2f;
    private int comboStep = 0;

    [Header("Attack Setup")]
    public Transform attackPoint;

    [Header("Crimson Aegis Strike VFX")]
    public bool hasCrimsonAegisStrike = false;
    public GameObject crimsonVFX_Attack1_Prefab;
    public GameObject crimsonVFX_Attack2_Prefab;
    public GameObject crimsonVFX_Combo_Prefab;
    public Transform attackVFXSpawnPoint;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerAttack attackScript;
    private PlayerInputActions playerControls;
    private float initialVFXSpawnPointLocalX;
    private Rigidbody2D platformRb;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            lastCheckpointPosition = transform.position;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        attackScript = GetComponent<PlayerAttack>();

        if (attackVFXSpawnPoint != null)
            initialVFXSpawnPointLocalX = attackVFXSpawnPoint.localPosition.x;

        normalMoveSpeed = moveSpeed;
        RelinkImportantObjects();
    }
    
    public static void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
    }

    void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerInputActions();
            playerControls.Player.Walk.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            playerControls.Player.Walk.canceled += ctx => moveInput = Vector2.zero;
            playerControls.Player.Jump.performed += ctx => TryJump();
            playerControls.Player.Attack.performed += ctx => StartCoroutine(TryAttackCombo());
            playerControls.Player.Dash.performed += ctx => TryDash();
        }
        playerControls.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (playerControls != null)
            playerControls.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }

        if (scene.name == "FirstLevel" && mode == LoadSceneMode.Single)
        {
            ResetAbilities();
        }

        ResetJumpAndDash();
        RelinkImportantObjects();
    }
    
    public void Respawn()
    {
        if (lastCheckpointPosition != Vector3.zero)
        {
            transform.position = lastCheckpointPosition;
        }
        else
        {
            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
            }
            else
            {
                Debug.LogError("No PlayerSpawnPoint or Checkpoint found! Cannot respawn.");
                return;
            }
        }
        
        GetComponent<PlayerHealth>().Heal(GetComponent<PlayerHealth>().maxHealth);
        ResetJumpAndDash();
        RelinkImportantObjects();
    }

    public void SetChargingWalkSpeed(bool isCharging)
    {
        if (isCharging)
        {
            moveSpeed = chargingWalkSpeed;
        }
        else
        {
            moveSpeed = normalMoveSpeed;
        }
    }

    void Update()
    {
        if (groundCheck == null || wallCheck == null)
        {
            RelinkImportantObjects();
        }

        if (groundCheck != null)
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (!wasGrounded && isGrounded)
            {
                jumpCount = 0;
                hasAirDashed = false;
                canDash = true;
            }
        }

        CheckWallSliding();

        if (moveInput.x != 0 && !isAttacking && !isWallSliding)
        {
            bool movingRight = moveInput.x > 0;
            if (movingRight != isFacingRight)
            {
                Flip();
            }
        }

        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f;
        bool isCharging = attackScript.IsCharging();

        animator.SetBool("IsChargingWalk", isMoving && isCharging);
        animator.SetBool("isRunning", isMoving && !isAttacking && !isDashing && !isCharging);
        animator.SetBool("isJumping", !isGrounded && rb.linearVelocity.y > 0.1f);
        animator.SetBool("isFalling", !isGrounded && rb.linearVelocity.y < -0.1f);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    private void CheckWallSliding()
    {
        if (wallCheck == null) return;
        
        isWallSliding = Physics2D.OverlapCircle(wallCheck.position, wallCheckDistance, wallLayer) && !isGrounded && rb.linearVelocity.y < 0;

        if (isWallSliding && !isWallJumping)
        {
            jumpCount = 0;
        }
    }

    void FixedUpdate()
    {
        if (!isDashing && !isAttacking && !isWallJumping)
        {
            Vector2 platformVelocity = (platformRb != null) ? platformRb.linearVelocity : Vector2.zero;
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed + platformVelocity.x, rb.linearVelocity.y);
        }
    }

    void TryJump()
    {
        if (isWallSliding)
        {
            StartCoroutine(WallJump());
        }
        else if (jumpCount < maxJumps && !isAttacking && !isDashing)
        {
            transform.parent = null;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("isJumping");
            jumpCount++;
        }
    }
    
    private IEnumerator WallJump()
    {
        isWallJumping = true;
        isWallSliding = false;
        
        float forceX = isFacingRight ? -wallJumpingForce.x : wallJumpingForce.x;
        rb.linearVelocity = new Vector2(forceX, wallJumpingForce.y);

        Flip();
        
        yield return new WaitForSeconds(wallJumpingDuration);
        
        isWallJumping = false;
    }

    void TryDash()
    {
        if (isAttacking || isDashing) return;
        int direction = isFacingRight ? 1 : -1;
        if (isGrounded && canDash)
        {
            dashCoroutine = StartCoroutine(Dash(direction));
        }
        else if (!isGrounded && !hasAirDashed && jumpCount == maxJumps)
        {
            hasAirDashed = true;
            dashCoroutine = StartCoroutine(Dash(direction));
        }
    }

    IEnumerator Dash(int direction)
    {
        transform.parent = null;
        isDashing = true;
        canDash = false;

        float animationSpeedMultiplier = dashAnimationLength / dashDuration;
        animator.speed = animationSpeedMultiplier;
        
        animator.SetTrigger("Dash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
        
        yield return new WaitForSeconds(dashDuration);
        
        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.speed = 1f;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        
        if (isGrounded)
        {
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
        dashCoroutine = null;
    }

    IEnumerator TryAttackCombo()
    {
        if (isAttacking) yield break;

        if (isDashing)
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }
            animator.speed = 1f;

            isAttacking = true;
            animator.Play("attack1");
            attackScript.Attack(8f);
            attackScript.InstantiateAttackVFX(crimsonVFX_Attack1_Prefab);

            float originalGravity = rb.gravityScale;
            if (originalGravity == 0) originalGravity = 5f; // Fallback gravity if needed
            
            yield return new WaitForSeconds(attack1Duration);

            rb.gravityScale = originalGravity;
            isDashing = false;
            isAttacking = false;
            dashCoroutine = null;
            
            // This is the new, important part that was missing
            if (isGrounded)
            {
                yield return new WaitForSeconds(dashCooldown);
                canDash = true;
            }
            yield break;
        }

        isAttacking = true;
        comboStep = (comboStep % 3) + 1;

        if (comboStep == 1)
        {
            animator.Play("attack1");
            attackScript.Attack(8f); 
            if (hasCrimsonAegisStrike && crimsonVFX_Attack1_Prefab != null)
            {
                attackScript.InstantiateAttackVFX(crimsonVFX_Attack1_Prefab);
            }
            yield return new WaitForSeconds(attack1Duration);
        }
        else if (comboStep == 2)
        {
            animator.Play("attack2");
            attackScript.Attack(8f);
            if (hasCrimsonAegisStrike && crimsonVFX_Attack2_Prefab != null)
            {
                attackScript.InstantiateAttackVFX(crimsonVFX_Attack2_Prefab);
            }
            yield return new WaitForSeconds(attack2Duration);
        }
        else if (comboStep == 3)
        {
            animator.Play("attack2");
            attackScript.Attack(15f);
            if (hasCrimsonAegisStrike && crimsonVFX_Combo_Prefab != null)
            {
                attackScript.InstantiateAttackVFX(crimsonVFX_Combo_Prefab);
            }
            yield return new WaitForSeconds(attack2Duration);

            animator.Play("attack1");
            attackScript.Attack(15f);
            if (hasCrimsonAegisStrike && crimsonVFX_Combo_Prefab != null)
            {
                attackScript.InstantiateAttackVFX(crimsonVFX_Combo_Prefab);
            }
            yield return new WaitForSeconds(attack1Duration);
            yield return new WaitForSeconds(comboResetDelay);
        }

        isAttacking = false;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;

        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x *= -1f;
            attackPoint.localPosition = localPos;
        }

        if (attackVFXSpawnPoint != null)
        {
            Vector3 vfxLocalPos = attackVFXSpawnPoint.localPosition;
            if (isFacingRight)
            {
                vfxLocalPos.x = initialVFXSpawnPointLocalX;
            }
            else
            {
                vfxLocalPos.x = -initialVFXSpawnPointLocalX;
            }
            attackVFXSpawnPoint.localPosition = vfxLocalPos;
        }
    }

    public void ResetAbilities()
    {
        hasCrimsonAegisStrike = false; 
    }

    public void ResetJumpAndDash()
    {
        jumpCount = 0;
        canDash = true;
        hasAirDashed = false;
        isDashing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.parent = collision.transform;
            platformRb = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            if (collision.gameObject.activeInHierarchy)
            {
                transform.parent = null;
            }
            platformRb = null;
        }
    }
    
    private void RelinkImportantObjects()
    {
        if (groundCheck == null)
        {
            Transform found = transform.Find("GroundCheck");
            if (found != null)
            {
                groundCheck = found;
                Debug.Log("GroundCheck re-linked successfully.");
            }
            else
            {
                GameObject gc = new GameObject("GroundCheck");
                gc.transform.SetParent(transform);
                gc.transform.localPosition = new Vector3(0, -1f, 0);
                groundCheck = gc.transform;
                Debug.LogWarning("GroundCheck was missing and has been re-created.");
            }
        }
        
        if (wallCheck == null)
        {
            Transform found = transform.Find("WallCheck");
            if (found != null)
            {
                wallCheck = found;
                Debug.Log("WallCheck re-linked successfully.");
            }
            else
            {
                GameObject wc = new GameObject("WallCheck");
                wc.transform.SetParent(transform);
                wc.transform.localPosition = new Vector3(0.5f, 0, 0);
                wallCheck = wc.transform;
                Debug.LogWarning("WallCheck was missing and has been re-created.");
            }
        }
    }
}