using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float moveSpeed = 4f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;
    public Transform groundCheck;
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isAttacking = false;
    private bool isPlayerDetected = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);
        float verticalDistance = Mathf.Abs(transform.position.y - player.position.y);

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

        float direction = Mathf.Sign(player.position.x - transform.position.x);
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
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.red;
    }

    void Idle()
    {
        PlayAnim("Nightborne_idle");
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void HandleFlip()
    {
        bool shouldFlip = rb.linearVelocity.x > 0.1f || rb.linearVelocity.x < -0.1f;
        if (!shouldFlip) return;

        bool facingRight = rb.linearVelocity.x > 0f;
        sr.flipX = !facingRight;

        Vector3 scale = attackPoint.localPosition;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        attackPoint.localPosition = scale;
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    void PlayAnim(string animName)
    {
        if (animator.HasState(0, Animator.StringToHash(animName)))
            animator.Play(animName);
        else
            Debug.LogWarning("Missing anim: " + animName);
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
