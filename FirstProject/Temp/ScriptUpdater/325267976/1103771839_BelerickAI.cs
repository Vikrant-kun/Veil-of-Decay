using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BelerickAI : MonoBehaviour
{
    public float detectionRange = 10f;
    public float attackRange = 3f;
    public float moveSpeed = 4f;
    public float attackCooldown = 1.5f;

    public string attackAnim1 = "Belerick_atk1";
    public string attackAnim2 = "Belerick_atk2";

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossAttack bossAttack;
    private BelerickHealth belerickHealth;

    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bossAttack = GetComponent<BossAttack>();
        belerickHealth = GetComponent<BelerickHealth>();

        if (player == null)
            Debug.LogError("❌ Player not found! Tag the player as 'Player'.");
    }

    void Update()
    {
        if (player == null || belerickHealth == null || belerickHealth.isDead) return;

        HandleFlip();
        HandleCombat();
    }

    void HandleCombat()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (isAttacking) return;

        if (distance <= attackRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
                StartCoroutine(PerformAttack());
        }
        else if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            PlayAnim("Belerick_fly");
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

        string selected = Random.value < 0.5f ? attackAnim1 : attackAnim2;
        PlayAnim(selected);

        yield return new WaitForSeconds(0.3f);
        bossAttack?.PerformAttack();

        yield return new WaitForSeconds(attackCooldown - 0.3f);
        isAttacking = false;
    }

    void HandleFlip()
    {
        if (player == null) return;
        sr.flipX = (transform.position.x > player.position.x);
    }

    void PlayAnim(string anim)
    {
        if (animator != null && animator.HasState(0, Animator.StringToHash(anim)))
        {
            animator.Play(anim, 0, 0f);
        }
    }

    public void EnterRageMode()
    {
        sr.color = Color.red;
        moveSpeed *= 1.3f;
        attackCooldown *= 0.7f;
        Debug.Log("🔥 Rage mode activated!");
    }
}
