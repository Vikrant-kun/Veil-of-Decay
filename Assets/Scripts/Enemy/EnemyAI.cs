using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection & Attack")]
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Transform playerTarget; // Renamed for clarity, will be set by GameRestartManager
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool isAttacking = false;
    private bool isPlayerDetected = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (animator == null) Debug.LogError("EnemyAI: Animator component not found on this GameObject! Make sure it's attached to " + gameObject.name, this);
        if (rb == null) Debug.LogError("EnemyAI: Rigidbody2D component not found on this GameObject! Make sure it's attached to " + gameObject.name, this);
        if (sr == null) Debug.LogError("EnemyAI: SpriteRenderer component not found on this GameObject! Make sure it's attached to " + gameObject.name, this);
    }

    // New public method to receive player target
    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
        Debug.Log("EnemyAI: Player target received from GameRestartManager: " + playerTarget.name, this);
    }

    void Update()
    {
        // Only proceed if we have a player target
        if (playerTarget == null)
        {
            return;
        }

        float horizontalDistance = Mathf.Abs(transform.position.x - playerTarget.position.x);
        float verticalDistance = Mathf.Abs(transform.position.y - playerTarget.position.y);

        isPlayerDetected = horizontalDistance <= detectionRange;

        if (isPlayerDetected && IsGrounded())
        {
            if (horizontalDistance <= attackRange)
            {
                rb.linearVelocity = Vector2.zero;

                if (!isAttacking)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            else
            {
                if (!isAttacking)
                {
                    MoveTowardsPlayer();
                }
            }
        }
        else
        {
            Idle();
        }

        HandleFlip();
    }

    void MoveTowardsPlayer()
    {
        PlayAnim("Nightborne_run");

        float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        PlayAnim("Nightborne_attack");

        yield return new WaitForSeconds(0.3f);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown - 0.3f);
        isAttacking = false;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public void DoFakeDodge()
    {
        float dodgeDir = sr.flipX ? -1 : 1;
        rb.AddForce(new Vector2(dodgeDir * 6f, 2f), ForceMode2D.Impulse);
        PlayAnim("Nightborne_idle");
        StartCoroutine(FlashWhite());
    }

    System.Collections.IEnumerator FlashWhite()
    {
        if (sr != null)
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.red;
        }
        else
        {
            Debug.LogWarning("EnemyAI: SpriteRenderer is null, cannot perform FlashWhite effect.");
        }
    }

    void Idle()
    {
        PlayAnim("Nightborne_idle");
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void HandleFlip()
    {
        if (sr == null || playerTarget == null) return; // Also check playerTarget here

        bool shouldFlip = rb.linearVelocity.x > 0.1f || rb.linearVelocity.x < -0.1f;
        if (!shouldFlip) return;

        bool facingRight = rb.linearVelocity.x > 0f;
        sr.flipX = !facingRight;

        if (attackPoint != null)
        {
            Vector3 scale = attackPoint.localPosition;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
            attackPoint.localPosition = scale;
        }
    }

    bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("EnemyAI: groundCheck Transform is null. Cannot perform ground check.", this);
            return false;
        }
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    void PlayAnim(string animName)
    {
        if (animator != null)
        {
            if (animator.HasState(0, Animator.StringToHash(animName)))
            {
                animator.Play(animName);
            }
            else
            {
                Debug.LogWarning($"EnemyAI: Missing animation state '{animName}' in Animator Controller for {gameObject.name}.");
            }
        }
        else
        {
            Debug.LogWarning($"EnemyAI: Animator is null on {gameObject.name}. Cannot play animation: {animName}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}