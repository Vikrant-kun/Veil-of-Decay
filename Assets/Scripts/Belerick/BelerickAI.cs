using UnityEngine;
using System.Collections;

public class BelerickAI : MonoBehaviour
{
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float moveSpeed = 4f;
    public float attackCooldown = 1f;

    public LayerMask groundLayer;

    public Transform groundCheck;
    public LayerMask playerLayer;

    public string attackAnim1 = "Belerick_atk1";
    public string attackAnim2 = "Belerick_atk2";

    private Transform playerTarget; // Renamed for clarity, will be set by GameRestartManager
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossAttack bossAttack;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;

    private enum BelerickState { Normal, RageMode }
    private BelerickState currentState = BelerickState.Normal;
    private Vector3 initialGroundPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bossAttack = GetComponent<BossAttack>();

        initialGroundPosition = transform.position;

        if (bossAttack == null)
            Debug.LogWarning("⚠️ BelerickAI: Missing BossAttack component!");
    }

    // New public method to receive player target
    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
        Debug.Log("BelerickAI: Player target received from GameRestartManager: " + playerTarget.name, this);
    }

    void Update()
    {
        // Only proceed if we have a player target
        if (playerTarget == null)
        {
            return;
        }

        switch (currentState)
        {
            case BelerickState.Normal:
            case BelerickState.RageMode:
                HandleCombatState();
                break;
        }

        HandleFlip();
    }

    void HandleCombatState()
    {
        float horizontalDistance = Mathf.Abs(transform.position.x - playerTarget.position.x);
        bool isPlayerDetected = horizontalDistance <= detectionRange;

        if (isPlayerDetected)
        {
            if (horizontalDistance <= attackRange)
            {
                rb.linearVelocity = Vector2.zero;
                if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
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
    }
    void MoveTowardsPlayer()
    {
        PlayAnim("Belerick_fly");

        float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    System.Collections.IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        string selectedAttackAnim = (Random.Range(0, 2) == 0) ? attackAnim1 : attackAnim2;
        PlayAnim(selectedAttackAnim);

        yield return new WaitForSeconds(0.3f);

        bossAttack?.PerformAttack();

        yield return new WaitForSeconds(attackCooldown - 0.3f);
        isAttacking = false;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public void EnterRageMode()
    {
        sr.color = new Color(1f, 0.4f, 0.4f);
        Debug.Log("💢 Belerick entered Rage Mode!");
        moveSpeed *= 1.2f;
        attackCooldown *= 0.8f;
    }

    public void DoFakeDodge()
    {
        if (sr == null || rb == null)
        {
            Debug.LogWarning("BelerickAI: SpriteRenderer or Rigidbody2D is null, cannot perform DoFakeDodge.");
            return;
        }
        float dodgeDir = sr.flipX ? -1 : 1;
        rb.AddForce(new Vector2(dodgeDir * 5f, 2f), ForceMode2D.Impulse);
        PlayAnim("Belerick_fly");
        StartCoroutine(FlashWhite());
    }

    IEnumerator FlashWhite()
    {
        if (sr == null)
        {
            Debug.LogWarning("BelerickAI: SpriteRenderer is null, cannot perform FlashWhite effect.");
            yield break;
        }
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.red;
    }

    void Idle()
    {
        PlayAnim("Belerick_fly");
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void HandleFlip()
    {
        if (playerTarget == null || sr == null) return; // Also check playerTarget here

        bool facingRight = transform.position.x < playerTarget.position.x;
        sr.flipX = !facingRight;

        if (bossAttack != null && bossAttack.attackPoint != null)
        {
            Vector3 scale = bossAttack.attackPoint.localPosition;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
            bossAttack.attackPoint.localPosition = scale;
        }
    }

    bool IsGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("BelerickAI: groundCheck Transform is null. Cannot perform ground check.", this);
            return false;
        }
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    void PlayAnim(string animName)
    {
        if (animator != null)
        {
            if (animator.HasState(0, Animator.StringToHash(animName)))
                animator.Play(animName);
            else
                Debug.LogWarning("Missing anim: " + animName + " on " + gameObject.name);
        }
        else
        {
            Debug.LogWarning("BelerickAI: Animator is null on " + gameObject.name + ". Cannot play animation: " + animName);
        }
    }

    public IEnumerator Die()
    {
        if (animator != null) animator.Play("Belerick_death");
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        yield return new WaitForSeconds(1.5f);

        gameObject.SetActive(false);
        Debug.Log("☠️ Belerick removed.");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}