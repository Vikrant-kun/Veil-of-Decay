using UnityEngine;
using System.Collections;

public class DarkSoul : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Return }
    private State state;

    public Transform player;
    public string playerTag = "Player";

    public Vector2 detectionBoxSize = new Vector2(10f, 5f);
    public Vector2 detectionBoxOffset = Vector2.zero;
    public bool requireLineOfSight = true;
    public LayerMask losBlockers;
    public LayerMask playerDetectionLayer;

    public float attackRange = 1.4f;
    public float attackWindup = 0.12f;
    public float attackCooldown = 0.8f;
    public int attackDamage = 10;
    public Transform attackPoint;
    public float attackRadius = 0.9f;
    public LayerMask playerLayer;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.8f;
    public Transform leftPoint;
    public Transform rightPoint;
    public float patrolPauseTime = 0.8f;
    public bool requireGroundedToMove = false;
    public Transform groundCheck;
    public LayerMask groundLayer;

    public string animParamIsWalking = "isWalking";
    public string animParamAttackTrigger = "Attack";
    public string animParamDeathTrigger = "Death";

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private DarkSoulHealth darkSoulHealth;

    private Transform patrolTarget;
    private bool isAttacking;
    private float pauseTimer;
    private float lostSightTimer;
    private const float closeEps = 0.06f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        darkSoulHealth = GetComponent<DarkSoulHealth>();
    }

    public void SetPlayerTarget(Transform target)
    {
        player = target;
    }

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        if (leftPoint == null || rightPoint == null)
        {
            GameObject l = new GameObject(name + "_PatrolL");
            GameObject r = new GameObject(name + "_PatrolR");
            l.transform.position = transform.position + Vector3.left * 2f;
            r.transform.position = transform.position + Vector3.right * 2f;
            leftPoint = l.transform;
            rightPoint = r.transform;
            l.transform.parent = transform.parent;
            r.transform.parent = transform.parent;
        }

        patrolTarget = leftPoint;
        state = State.Patrol;
        SetAnimIdle();
    }

    void Update()
    {
        if (darkSoulHealth != null && darkSoulHealth.IsDead)
        {
            SetVelX(0f);
            return;
        }

        if (player == null)
        {
            SetVelX(0f);
            SetAnimIdle();
            return;
        }

        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Chase: TickChase(); break;
            case State.Attack: break;
            case State.Return: TickReturn(); break;
        }

        HandleFlip();
    }

    void TickPatrol()
    {
        if (requireGroundedToMove && !IsGrounded())
        {
            SetVelX(0f);
            SetAnimIdle();
            return;
        }

        if (Mathf.Abs(transform.position.x - patrolTarget.position.x) > closeEps)
        {
            float dir = Mathf.Sign(patrolTarget.position.x - transform.position.x);
            SetVelX(dir * patrolSpeed);
            SetAnimRun();
        }
        else
        {
            SetVelX(0f);
            SetAnimIdle();
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= patrolPauseTime)
            {
                patrolTarget = (patrolTarget == leftPoint) ? rightPoint : leftPoint;
                pauseTimer = 0f;
            }
        }

        if (PlayerDetected())
        {
            state = State.Chase;
            lostSightTimer = 0f;
        }
    }

    void TickChase()
    {
        if (requireGroundedToMove && !IsGrounded())
        {
            SetVelX(0f);
            SetAnimIdle();
            return;
        }

        float dx = player.position.x - transform.position.x;
        SetVelX(Mathf.Sign(dx) * chaseSpeed);
        SetAnimRun();

        if (!isAttacking && InAttackRange() && (HasLOS() || !requireLineOfSight))
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        if (!PlayerDetected())
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer > 1.25f)
            {
                state = State.Return;
                patrolTarget = (Mathf.Abs(transform.position.x - leftPoint.position.x) <
                                Mathf.Abs(transform.position.x - rightPoint.position.x))
                                ? leftPoint : rightPoint;
                return;
            }
        }
        else lostSightTimer = 0f;
    }

    void TickReturn()
    {
        if (requireGroundedToMove && !IsGrounded())
        {
            SetVelX(0f);
            SetAnimIdle();
            return;
        }

        if (Mathf.Abs(transform.position.x - patrolTarget.position.x) > closeEps)
        {
            float dir = Mathf.Sign(patrolTarget.position.x - transform.position.x);
            SetVelX(dir * patrolSpeed);
            SetAnimRun();
        }
        else
        {
            SetVelX(0f);
            SetAnimIdle();
            state = State.Patrol;
            pauseTimer = 0f;
        }

        if (PlayerDetected())
        {
            state = State.Chase;
            lostSightTimer = 0f;
        }
    }

    IEnumerator AttackRoutine()
    {
        state = State.Attack;
        isAttacking = true;
        SetVelX(0f);
        SetAnimAttack();

        yield return new WaitForSeconds(attackWindup);

        if (attackPoint != null)
        {
            var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
            foreach (var hit in hits)
            {
                var ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown - attackWindup);
        isAttacking = false;
        state = PlayerDetected() ? State.Chase : State.Return;
        if (state == State.Return)
        {
            patrolTarget = (Mathf.Abs(transform.position.x - leftPoint.position.x) <
                            Mathf.Abs(transform.position.x - rightPoint.position.x))
                            ? leftPoint : rightPoint;
        }
    }

    void SetVelX(float x)
    {
        if (rb != null) rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    bool PlayerDetected()
    {
        if (player == null) return false;
        Vector2 boxCenter = (Vector2)transform.position + detectionBoxOffset;
        Collider2D hit = Physics2D.OverlapBox(boxCenter, detectionBoxSize, 0f, playerDetectionLayer);
        if (hit != null && hit.CompareTag("Player"))
        {
            if (requireLineOfSight) return HasLOS();
            return true;
        }
        return false;
    }

    bool InAttackRange()
    {
        return player != null && Vector2.Distance(player.position, transform.position) <= attackRange;
    }

    bool HasLOS()
    {
        if (player == null) return false;
        Vector2 from = transform.position;
        Vector2 to = player.position;
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);
        var hit = Physics2D.Raycast(from, dir, dist, losBlockers);
        return hit.collider == null;
    }

    bool IsGrounded()
    {
        if (groundCheck == null) return true;
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    void SetAnimIdle()
    {
        if (animator != null)
        {
            animator.SetBool(animParamIsWalking, false);
            animator.ResetTrigger(animParamAttackTrigger);
        }
    }

    void SetAnimRun()
    {
        if (animator != null) animator.SetBool(animParamIsWalking, true);
    }

    void SetAnimAttack()
    {
        if (animator != null) animator.SetTrigger(animParamAttackTrigger);
    }

    void HandleFlip()
    {
        if (sr == null || player == null) return;
        bool currentlyFacingRight = !sr.flipX;
        bool intendsToFaceRight = currentlyFacingRight;

        if (state == State.Chase || state == State.Attack)
            intendsToFaceRight = (player.position.x - transform.position.x) > 0f;
        else if (state == State.Patrol || state == State.Return)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
                intendsToFaceRight = rb.linearVelocity.x > 0f;
            else
                intendsToFaceRight = (patrolTarget.position.x - transform.position.x) > 0f;
        }

        if (currentlyFacingRight != intendsToFaceRight) sr.flipX = !intendsToFaceRight;

        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (sr.flipX ? -1 : 1);
            attackPoint.localPosition = localPos;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 boxCenter = (Vector2)transform.position + detectionBoxOffset;
        Gizmos.DrawWireCube(boxCenter, detectionBoxSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);
            Gizmos.DrawSphere(leftPoint.position, 0.1f);
            Gizmos.DrawSphere(rightPoint.position, 0.1f);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}
