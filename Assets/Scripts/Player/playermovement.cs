using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    // Add a static instance for the Singleton pattern
    public static PlayerMovement Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    public bool isFacingRight = true;

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
    public LayerMask groundLayer;
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

    void Awake() // <-- CHANGES START HERE
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // This is the key line: tells Unity not to destroy this GameObject when a new scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this GameObject (to prevent duplicates)
            Destroy(gameObject);
            return; // Exit Awake to prevent further initialization of a destroyed object
        }

        // Initialize component references here.
        // Doing it in Awake ensures they are ready before Start or OnEnable
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        attackScript = GetComponent<PlayerAttack>();
    } // <-- CHANGES END HERE

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

    public bool GetFacingRight() => !spriteRenderer.flipX;

    void OnDisable()
    {
        if (playerControls != null)
            playerControls.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded; // Important to unsubscribe to prevent memory leaks
    }

    void Start()
    {
        // Component initialization moved to Awake for robustness.
        // No need to initialize them here again unless specifically required for Start logic.
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // This logic is good, it will move the persistent player to the spawn point in Level2
        if (scene.name == "Level2")
        {
            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
                Debug.Log("Player spawned at: " + spawnPoint.transform.position);
            }
            else
            {
                Debug.LogWarning("PlayerSpawnPoint not found in scene: " + scene.name + ". Player will remain at current position.");
            }
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

        if (moveInput.x != 0 && !isAttacking)
        {
            bool movingRight = moveInput.x > 0;
            if (movingRight != isFacingRight)
            {
                Flip();
            }
        }

        animator.SetBool("isRunning", Mathf.Abs(moveInput.x) > 0.01f && !isAttacking && !isDashing);
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
            animator.SetTrigger("Jump");
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
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(8f);
            PlayCrimsonAegisVFX(crimsonVFX_Attack1_Prefab);
            yield return new WaitForSeconds(attack1Duration);
        }
        else if (comboStep == 2)
        {
            animator.Play("attack2");
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(8f);
            PlayCrimsonAegisVFX(crimsonVFX_Attack2_Prefab);
            yield return new WaitForSeconds(attack2Duration);
        }
        else if (comboStep == 3)
        {
            animator.Play("attack2");
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(15f);
            PlayCrimsonAegisVFX(crimsonVFX_Combo_Prefab);
            yield return new WaitForSeconds(attack2Duration);

            animator.Play("attack1");
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(15f);
            PlayCrimsonAegisVFX(crimsonVFX_Combo_Prefab);
            yield return new WaitForSeconds(attack1Duration);

            yield return new WaitForSeconds(comboResetDelay);
        }

        isAttacking = false;
    }

    private void PlayCrimsonAegisVFX(GameObject vfxPrefab)
    {
        if (hasCrimsonAegisStrike && vfxPrefab != null && attackVFXSpawnPoint != null)
        {
            GameObject vfxInstance = Instantiate(vfxPrefab, attackVFXSpawnPoint.position, attackVFXSpawnPoint.rotation);
            Destroy(vfxInstance, 1.5f);
        }
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
    }
}