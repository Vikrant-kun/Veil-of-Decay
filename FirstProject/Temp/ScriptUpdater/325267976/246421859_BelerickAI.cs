using UnityEngine;
using System.Collections;

public class BelerickAI : MonoBehaviour
{
    public float detectionRange = 10f;
    public float attackRange = 3f;
    public float moveSpeed = 4f;
    public float attackCooldown = 1f;
    public float flySpeed = 8f;
    public float arcHeight = 5f;
    public float flyDuration = 3f;

    public string attackAnim1 = "Belerick_atk1";
    public string attackAnim2 = "Belerick_atk2";
    public string flyAnim = "Belerick_fly";

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossAttack bossAttack;
    private BelerickHealth belerickHealth;

    private bool isAttacking = false;
    private bool isFlyingWild = false;
    private int attackCount = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bossAttack = GetComponent<BossAttack>();
        belerickHealth = GetComponent<BelerickHealth>();

        if (player == null)
            Debug.LogError("Player not found! Tag it as 'Player'.");

        if (bossAttack == null)
            Debug.LogWarning("⚠️ BossAttack component missing!");
    }

    void Update()
    {
        if (player == null || belerickHealth?.isDead == true || isFlyingWild || isAttacking)
            return;

        float distance = Mathf.Abs(transform.position.x - player.position.x);
        if (distance <= detectionRange)
        {
            if (distance <= attackRange)
            {
                StartCoroutine(AttackPhase());
            }
            else
            {
                ChasePlayer();
            }
        }

        HandleFlip();
    }

    IEnumerator AttackPhase()
    {
        isAttacking = true;
        attackCount++;

        string chosenAnim = (Random.Range(0, 2) == 0) ? attackAnim1 : attackAnim2;
        PlayAnim(chosenAnim);
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);
        bossAttack?.PerformAttack();
        yield return new WaitForSeconds(attackCooldown);

        if (attackCount >= 3)
        {
            attackCount = 0;
            StartCoroutine(FlyWildPhase());
        }
        else
        {
            isAttacking = false;
        }
    }

    IEnumerator FlyWildPhase()
    {
        isFlyingWild = true;
        PlayAnim(flyAnim);

        float timer = 0f;
        Vector2 startPos = transform.position;
        float direction = (Random.value > 0.5f) ? 1f : -1f;

        while (timer < flyDuration)
        {
            float t = timer / flyDuration;
            float x = startPos.x + direction * t * flySpeed;
            float y = startPos.y + Mathf.Sin(t * Mathf.PI) * arcHeight;

            rb.MovePosition(new Vector2(x, y));

            timer += Time.deltaTime;
            yield return null;
        }

        isFlyingWild = false;
        isAttacking = false;
    }

    void ChasePlayer()
    {
        PlayAnim(flyAnim);
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void HandleFlip()
    {
        if (player == null) return;
        bool facingRight = transform.position.x < player.position.x;
        sr.flipX = !facingRight;
    }

    void PlayAnim(string animName)
    {
        if (animator == null) return;
        if (animator.HasState(0, Animator.StringToHash(animName)))
        {
            animator.Play(animName, 0, 0f);
        }
    }
}
