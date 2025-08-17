using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 5f;
    private float normalMoveSpeed; // Stores the player's original movement speed
    private Vector2 moveInput;
    public bool isFacingRight = true;
    public float chargingWalkSpeed = 2f; // New variable for the walk speed while charging

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 0.5f;
    private bool isDashing = false;
    private bool canDash = true;
    private bool hasAirDashed = false;

    [Header("Jump")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    [Tooltip("Select all layers that the player can jump from, including ground and moving platforms.")]
    public LayerMask groundLayer; // Added tooltip for clarity
    private bool isGrounded = false;
    public int maxJumps = 2;
    private int jumpCount = 0;

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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        {
            initialVFXSpawnPointLocalX = attackVFXSpawnPoint.localPosition.x;
        }

        normalMoveSpeed = moveSpeed;
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
    }
    
    // New method to set the charging walk speed
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

        // Debug log to confirm grounded status
        // Debug.Log("Is Grounded: " + isGrounded + " | Jump Count: " + jumpCount);


        if (moveInput.x != 0 && !isAttacking)
        {
            bool movingRight = moveInput.x > 0;
            if (movingRight != isFacingRight)
            {
                Flip();
            }
        }

        // Updated animation logic to handle the charging walk
        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f;
        bool isCharging = attackScript.IsCharging();

        animator.SetBool("IsChargingWalk", isMoving && isCharging);
        animator.SetBool("isRunning", isMoving && !isAttacking && !isDashing && !isCharging);
        animator.SetBool("isJumping", !isGrounded && rb.linearVelocity.y > 0.1f);
        animator.SetBool("isFalling", !isGrounded && rb.linearVelocity.y < -0.1f);
        animator.SetBool("isGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        if (!isAttacking && !isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }

    void TryJump()
    {
        if (jumpCount < maxJumps && !isAttacking && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("isJumping");
            jumpCount++;
        }
    }

    void TryDash()
    {
        if (isAttacking || isDashing) return;
        int direction = isFacingRight ? 1 : -1;
        if (isGrounded && canDash)
        {
            StartCoroutine(Dash(direction));
        }
        else if (!isGrounded && !hasAirDashed && jumpCount == maxJumps)
        {
            hasAirDashed = true;
            StartCoroutine(Dash(direction));
        }
    }

    IEnumerator Dash(int direction)
    {
        isDashing = true;
        canDash = false;
        animator.SetTrigger("Dash");
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = originalGravity;
        isDashing = false;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        if (isGrounded)
        {
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
    }

    IEnumerator TryAttackCombo()
    {
        if (isAttacking) yield break;
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
}