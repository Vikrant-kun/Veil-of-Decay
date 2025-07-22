using UnityEngine;
using System.Collections;

public class BelerickAI : MonoBehaviour
{
    // Existing variables
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float moveSpeed = 4f;
    public float attackCooldown = 1f;

    // Rage Mode specific variables
    public float ascendHeight = 8f; // How high Belerick "jumps" upwards
    public float ascendDuration = 0.5f; // Time to reach peak height (set this in Inspector)
    public float slamDelay = 0.2f; // Short pause at peak before slamming down (set this in Inspector)
    public float descendDuration = 0.3f; // NEW: How fast he slams down (total time)
    // public float slamFallSpeed = 20f; // This variable is no longer strictly used for the slam mechanics, but you can keep it if you want to use it for an impact effect
    public LayerMask groundLayer;

    public Transform groundCheck;
    public LayerMask playerLayer;

    public string attackAnim1 = "Belerick_atk1";
    public string attackAnim2 = "Belerick_atk2";

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BossAttack bossAttack;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;

    private enum BelerickState { Normal, RageTransition, RageMode }
    private BelerickState currentState = BelerickState.Normal;
    private Vector3 initialGroundPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bossAttack = GetComponent<BossAttack>();

        initialGroundPosition = transform.position;

        if (player == null)
            Debug.LogError("Player GameObject not found! Make sure your player has the 'Player' tag.");

        if (bossAttack == null)
            Debug.LogWarning("⚠️ BelerickAI: Missing BossAttack component!");
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case BelerickState.Normal:
            case BelerickState.RageMode:
                HandleCombatState();
                break;
            case BelerickState.RageTransition:
                rb.linearVelocity = Vector2.zero; // Prevent any other movement
                // Do NOT call HandleFlip() here if you want him to face one way during transition
                // If you want him to still flip, keep it, but it might look odd mid-air.
                break;
        }

        if (currentState != BelerickState.RageTransition) // Only flip when not in transition
        {
            HandleFlip();
        }
    }

    void HandleCombatState()
    {
        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);
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

    public void StartRageTransition()
    {
        if (currentState == BelerickState.RageTransition) return;

        currentState = BelerickState.RageTransition;
        StopAllCoroutines();
        StartCoroutine(RageModeSequence());
    }

    IEnumerator RageModeSequence()
    {
        Debug.Log("Belerick: Initiating rage transition sequence!");

        // 1. Ascend (Jump into sky)
        PlayAnim("Belerick_fly"); // Or Belerick_idle
        rb.bodyType = RigidbodyType2D.Kinematic; // Make Rigidbody kinematic to control movement purely by transform.position
        rb.linearVelocity = Vector2.zero;

        Vector3 startPos = transform.position;
        Vector3 peakPos = new Vector3(transform.position.x, initialGroundPosition.y + ascendHeight, transform.position.z);
        float timer = 0f;

        while (timer < ascendDuration)
        {
            transform.position = Vector3.Lerp(startPos, peakPos, timer / ascendDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = peakPos;

        Debug.Log("Belerick: Reached peak height.");

        // 2. Mid-air pause and color change
        yield return new WaitForSeconds(slamDelay);

        sr.color = new Color(1f, 0.4f, 0.4f); // Turn red
        Debug.Log("💢 Belerick changed color!");

        // 3. Descend (Slam on ground)
        PlayAnim("Belerick_fly"); // Or Belerick_idle, or a specific falling anim if you get one
        startPos = transform.position; // Start from peak position
        Vector3 endPos = new Vector3(transform.position.x, initialGroundPosition.y, transform.position.z); // Target original Y

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / descendDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos; // Ensure it lands precisely

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Dynamic; // Set back to dynamic
        rb.gravityScale = 0; // Set gravity back to 0 for normal flying behavior

        Debug.Log("Belerick: Landed from slam.");

        // Optional: Trigger a screen shake or particle effect on slam impact
        // You can use a custom event or directly call a CameraShake.Instance.ShakeCamera(duration, magnitude); if you have one.
        // Also, you could use a Physics2D.OverlapCircle here to damage players near the landing spot.

        // 4. Activate Rage Mode buffs
        currentState = BelerickState.RageMode;
        moveSpeed *= 1.2f;
        attackCooldown *= 0.8f;
        Debug.Log("💢 Belerick entered Rage Mode buffs!");
    }

    // --- All other methods as previously corrected ---

    void MoveTowardsPlayer()
    {
        PlayAnim("Belerick_fly");

        float direction = Mathf.Sign(player.position.x - transform.position.x);
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
        float dodgeDir = sr.flipX ? -1 : 1;
        rb.AddForce(new Vector2(dodgeDir * 5f, 2f), ForceMode2D.Impulse);
        PlayAnim("Belerick_idle");
        StartCoroutine(FlashWhite());
    }

    IEnumerator FlashWhite()
    {
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
        if (player == null) return;

        bool facingRight = transform.position.x < player.position.x;
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
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    void PlayAnim(string animName)
    {
        if (animator.HasState(0, Animator.StringToHash(animName)))
            animator.Play(animName);
        else
            Debug.LogWarning("Missing anim: " + animName + " on " + gameObject.name);
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
