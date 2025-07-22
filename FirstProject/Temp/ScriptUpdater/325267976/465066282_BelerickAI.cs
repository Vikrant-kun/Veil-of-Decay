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
    public LayerMask playerLayer;

    [Header("Animations")]
    public string attackAnim1 = "Belerick_atk1";
    public string attackAnim2 = "Belerick_atk2";

    [Header("Slam Attack")]
    public float slamRiseHeight = 4f;
    public float slamRiseSpeed = 10f;
    public float slamFallSpeed = 25f;
    public float slamCooldown = 10f;
    public float slamShockRadius = 3f;
    public float knockbackForce = 10f;
    public GameObject slamEffectPrefab;
    public GameObject shockwavePrefab;
    public Transform slamImpactPoint;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossAttack bossAttack;
    private BelerickHealth belerickHealth;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;
    private float lastSlamTime = -Mathf.Infinity;
    private bool isInSlamPhase = false;
    private int attackCycleCount = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bossAttack = GetComponent<BossAttack>();
        belerickHealth = GetComponent<BelerickHealth>();

        if (player == null)
            Debug.LogError("❌ Player not found! Tag it as 'Player'.");
    }

    void Update()
    {
        if (isInSlamPhase || player == null || belerickHealth == null || belerickHealth.isDead) return;

        HandleCombatState();
        HandleFlip();
    }

    void HandleCombatState()
    {
        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);
        bool isPlayerDetected = horizontalDistance <= detectionRange;

        if (isAttacking || Time.time - lastAttackTime < attackCooldown) return;

        if (isPlayerDetected)
        {
            if (horizontalDistance <= attackRange)
            {
                rb.linearVelocity = Vector2.zero;

                if (attackCycleCount >= 3 && Time.time - lastSlamTime >= slamCooldown)
                {
                    StartCoroutine(PerformSlamQuake());
                }
                else
                {
                    StartCoroutine(PerformAttack());
                }
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            PlayAnim("Belerick_idle");
        }
    }

    void MoveTowardsPlayer()
    {
        PlayAnim("Belerick_fly");

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        float vertical = Mathf.Sign(player.position.y - transform.position.y);
        rb.linearVelocity = new Vector2(direction * moveSpeed, vertical * moveSpeed * 0.5f);
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        attackCycleCount++;

        PlayAnim(attackAnim1);
        yield return new WaitForSeconds(0.3f);
        bossAttack?.PerformAttack();

        yield return new WaitForSeconds(0.4f);

        PlayAnim(attackAnim2);
        yield return new WaitForSeconds(0.3f);
        bossAttack?.PerformAttack();

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    IEnumerator PerformSlamQuake()
    {
        isAttacking = true;
        isInSlamPhase = true;
        rb.linearVelocity = Vector2.zero;

        PlayAnim("Belerick_fly");
        Vector2 targetPosition = new Vector2(transform.position.x, transform.position.y + slamRiseHeight);

        while (transform.position.y < targetPosition.y)
        {
            rb.linearVelocity = new Vector2(0, slamRiseSpeed);
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        sr.color = new Color(1.2f, 0.3f, 0.3f);
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.25f);
        Time.timeScale = 1f;
        sr.color = Color.white;

        PlayAnim("Belerick_slam");
        rb.linearVelocity = new Vector2(0, -slamFallSpeed);

        while (!Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer))
            yield return null;

        rb.linearVelocity = Vector2.zero;

        if (shockwavePrefab && slamImpactPoint)
            Instantiate(shockwavePrefab, slamImpactPoint.position, Quaternion.identity);

        if (slamEffectPrefab)
            Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);

        if (player != null)
        {
            float dist = Vector2.Distance(player.position, transform.position);
            if (dist < slamShockRadius)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                    playerRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        attackCycleCount = 0;
        lastSlamTime = Time.time;
        yield return new WaitForSeconds(0.6f);

        isInSlamPhase = false;
        isAttacking = false;
    }

    public void EnterRageMode()
    {
        sr.color = new Color(1f, 0.4f, 0.4f);
        moveSpeed *= 1.4f;
        attackCooldown *= 0.75f;
        Debug.Log("🔥 Rage Mode Activated!");
    }

    void HandleFlip()
    {
        if (player == null) return;
        sr.flipX = transform.position.x > player.position.x;
    }

    void PlayAnim(string animName)
    {
        if (animator == null || belerickHealth == null || belerickHealth.isDead) return;

        animator.Play(animName);
    }

    private void OnDrawGizmosSelected()
    {
        if (slamImpactPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(slamImpactPoint.position, slamShockRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
