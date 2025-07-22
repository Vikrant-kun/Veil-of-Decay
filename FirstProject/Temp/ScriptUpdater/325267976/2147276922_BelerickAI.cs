using UnityEngine;
using System.Collections;

public class BelerickAI : MonoBehaviour
{
    [Header("General Settings")]
    public float detectionRange = 8f;
    public float attackRange = 3f;
    public float moveSpeed = 4f;
    public float attackCooldown = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Rage Mode")]
    public bool isInRage = false;
    public float rageSpeedMultiplier = 1.5f;
    public Color rageColor = Color.red;
    private SpriteRenderer sr;

    [Header("Attack Cycle")]
    private int attackCount = 0;
    public int maxAttackBeforeSlam = 3;

    [Header("Slam Quake Attack")]
    public GameObject slamShockwave;
    public float slamRiseSpeed = 12f;
    public float slamFallSpeed = 15f;
    public float slamRadius = 2.5f;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private bool isAttacking;
    private bool isGrounded;
    private float lastAttackTime;
    private bool isSlamming = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isSlamming || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (distance <= attackRange && Time.time > lastAttackTime + attackCooldown && isGrounded)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine());
        }
        else if (distance <= detectionRange)
        {
            MoveTowardPlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void MoveTowardPlayer()
    {
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        float vertical = player.position.y > transform.position.y ? 1f : -0.5f;
        float speed = isInRage ? moveSpeed * rageSpeedMultiplier : moveSpeed;
        rb.linearVelocity = new Vector2(direction * speed, vertical * speed * 0.5f);
        sr.flipX = direction < 0;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        if (attackCount >= maxAttackBeforeSlam)
        {
            attackCount = 0;
            StartCoroutine(SlamQuakeAttack());
        }
        else
        {
            anim.SetTrigger(attackCount % 2 == 0 ? "Attack1" : "Attack2");
            attackCount++;
            yield return new WaitForSeconds(0.6f);
            isAttacking = false;
        }
    }

    IEnumerator SlamQuakeAttack()
    {
        isSlamming = true;
        anim.SetTrigger("Roar");
        yield return new WaitForSeconds(0.7f);

        rb.linearVelocity = new Vector2(0, slamRiseSpeed);
        yield return new WaitForSeconds(0.6f);

        rb.linearVelocity = new Vector2(0, -slamFallSpeed);
        yield return new WaitUntil(() => isGrounded);

        anim.SetTrigger("Slam");
        yield return new WaitForSeconds(0.3f);

        Instantiate(slamShockwave, transform.position, Quaternion.identity);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, slamRadius, playerLayer);
        if (hit)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph) ph.TakeDamage(30);
        }

        yield return new WaitForSeconds(0.5f);
        isSlamming = false;
        isAttacking = false;
    }

    public void EnterRageMode()
    {
        if (isInRage) return;
        isInRage = true;
        sr.color = rageColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}
