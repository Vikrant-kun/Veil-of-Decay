using UnityEngine;
using System.Collections;

public class BelerickAI : MonoBehaviour
{
    public float detectionRange = 8f;
    public float attackRange = 3f;
    public float moveSpeed = 4f;
    public float attackCooldown = 1f;

    public string attackAnim1 = "Belerick_atk1";
    public string attackAnim2 = "Belerick_atk2";

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossAttack bossAttack;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;

    private BelerickHealth belerickHealth;

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
    }

    void Update()
    {
        if (player == null || belerickHealth == null || belerickHealth.isDead) return;

        HandleCombatState();
        HandleFlip();
    }

    void HandleCombatState()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (isAttacking) return;

        if (distance <= detectionRange)
        {
            if (distance <= attackRange && Mathf.Abs(transform.position.y - player.position.y) <= 1f)
            {
                rb.linearVelocity = Vector2.zero;

                if (Time.time - lastAttackTime >= attackCooldown)
                    StartCoroutine(PerformAttack());
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        PlayAnim("Belerick_fly");
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        string selected = (Random.Range(0, 2) == 0) ? attackAnim1 : attackAnim2;
        PlayAnim(selected);

        yield return new WaitForSeconds(0.3f);
        bossAttack?.PerformAttack();

        yield return new WaitForSeconds(attackCooldown - 0.3f);
        isAttacking = false;
    }

    public void EnterRageMode()
    {
        sr.color = new Color(1f, 0.4f, 0.4f); // red tint
        moveSpeed *= 1.2f;
        attackCooldown *= 0.8f;
        Debug.Log("🔥 Rage Mode Activated!");
    }

    void HandleFlip()
    {
        if (player == null) return;

        bool facingRight = transform.position.x < player.position.x;
        sr.flipX = !facingRight;
    }

    void PlayAnim(string animName)
    {
        if (animator == null || belerickHealth == null || belerickHealth.isDead) return;

        if (animator.HasState(0, Animator.StringToHash(animName)))
        {
            animator.Play(animName, 0, 0f);
        }
    }
}
