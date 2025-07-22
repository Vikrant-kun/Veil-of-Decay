using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
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

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Input System
    private PlayerInputActions playerControls;

    private void OnEnable()
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
    }

    public bool GetFacingRight() => !spriteRenderer.flipX;

    private void OnDisable()
    {
        if (playerControls != null)
            playerControls.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
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

    private void FixedUpdate()
    {
        if (!isAttacking && !isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void TryJump()
    {
        if (jumpCount < maxJumps && !isAttacking && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
            jumpCount++;
        }
    }

    private void TryDash()
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

    private IEnumerator Dash(int direction)
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

    private IEnumerator TryAttackCombo()
    {
        if (isAttacking) yield break;

        isAttacking = true;
        comboStep = (comboStep % 3) + 1;

        var attackScript = GetComponent<PlayerAttack>();

        if (comboStep == 1)
        {
            animator.Play("attack1");
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(5f); // attack1 → 5 dmg
            yield return new WaitForSeconds(attack1Duration);
        }
        else if (comboStep == 2)
        {
            animator.Play("attack2");
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(5f); // attack2 → 5 dmg
            yield return new WaitForSeconds(attack2Duration);
        }
        else if (comboStep == 3)
        {
            animator.Play("attack1");
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(4f); // combo part1
            yield return new WaitForSeconds(attack1Duration);

            animator.Play("attack2");
            yield return new WaitForSeconds(0.1f);
            attackScript.Attack(4f); // combo part2
            yield return new WaitForSeconds(attack2Duration);

            yield return new WaitForSeconds(comboResetDelay); // short pause before reset
        }

        isAttacking = false;
    }

    private void Flip()
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
