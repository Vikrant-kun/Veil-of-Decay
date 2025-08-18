using UnityEngine;
using System.Collections; // Keep this, as you're using IEnumerator

public class GhostAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Return }
    private State state;

    [Header("Player Detection")]
    public Transform player;
    public string playerTag = "Player";
    public Vector2 detectionBoxSize = new Vector2(8f, 4f);
    public Vector2 detectionBoxOffset = Vector2.zero;
    public bool requireLineOfSight = false;
    public LayerMask losBlockers;
    public LayerMask playerLayer; // used for BOTH detection + attack

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackWindup = 0.15f;
    public float attackCooldown = 0.7f;
    public int attackDamage = 6;
    public Transform attackPoint;
    public float attackRadius = 0.8f;

    [Header("Movement")]
    public float patrolSpeed = 1.8f;
    public float chaseSpeed = 3f;
    public Transform leftPoint;
    public Transform rightPoint;
    public float patrolPauseTime = 1f;

    [Header("Animation Params")]
    public string animParamIsFlying = "isFlying";
    public string animParamAttackTrigger = "Attack";
    public string animParamDeathTrigger = "Death";

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private GhostHealth ghostHealth; // Reference to GhostHealth

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
        ghostHealth = GetComponent<GhostHealth>(); // Get GhostHealth component
    }

    void Start()
    {
        // This initial player finding is good for testing, but SetPlayerTarget will be used by activator
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
            l.transform.parent = transform.parent; // Parent to same as ghost
            r.transform.parent = transform.parent; // Parent to same as ghost
        }

        patrolTarget = leftPoint;
        state = State.Patrol;
        SetAnimIdle();
    }

    void Update()
    {
        // Stop all behavior if ghost is dead
        if (ghostHealth != null && ghostHealth.IsDead)
        {
            SetVelX(0f);
            return;
        }

        // If player reference is lost, stop
        if (player == null)
        {
            SetVelX(0f);
            SetAnimIdle();
            return;
        }

        // State machine for ghost behavior
        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Chase: TickChase(); break;
            case State.Attack: /* Do nothing, handled by coroutine */ break;
            case State.Return: TickReturn(); break;
        }

        HandleFlip();
    }

    /// <summary>
    /// Sets the player target for the GhostAI. Called by activators.
    /// </summary>
    /// <param name="newTarget">The Transform of the player.</param>
    public void SetPlayerTarget(Transform newTarget)
    {
        player = newTarget;
        // When a new target is set, reset state to chase if player is detected, otherwise patrol.
        if (PlayerDetected()) {
            state = State.Chase;
            lostSightTimer = 0f;
        } else {
            state = State.Patrol;
            pauseTimer = 0f; // Reset patrol pause
        }
    }


    void TickPatrol()
    {
        if (Mathf.Abs(transform.position.x - patrolTarget.position.x) > closeEps)
        {
            float dir = Mathf.Sign(patrolTarget.position.x - transform.position.x);
            SetVelX(dir * patrolSpeed);
            SetAnimFly();
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
        float dx = player.position.x - transform.position.x;

        // Only move if NOT in attack range (prevents overshoot)
        if (Mathf.Abs(dx) > attackRange && !isAttacking)
        {
            SetVelX(Mathf.Sign(dx) * chaseSpeed);
            SetAnimFly();
        }
        else
        {
            SetVelX(0f); // Stop when close enough to attack
        }

        // Initiate attack if in range and not already attacking
        if (!isAttacking && InAttackRange() && (HasLOS() || !requireLineOfSight))
        {
            StartCoroutine(AttackRoutine());
            return; // Exit TickChase as attack routine will manage state
        }

        // Lost sight logic
        if (!PlayerDetected())
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer > 1.25f) // Time before returning to patrol
            {
                state = State.Return;
                // Determine closer patrol point to return to
                patrolTarget = (Mathf.Abs(transform.position.x - leftPoint.position.x) <
                                Mathf.Abs(transform.position.x - rightPoint.position.x))
                                ? leftPoint : rightPoint;
                return;
            }
        }
        else
        {
            lostSightTimer = 0f; // Reset timer if player is detected
        }
    }

    void TickReturn()
    {
        if (Mathf.Abs(transform.position.x - patrolTarget.position.x) > closeEps)
        {
            float dir = Mathf.Sign(patrolTarget.position.x - transform.position.x);
            SetVelX(dir * patrolSpeed);
            SetAnimFly();
        }
        else
        {
            SetVelX(0f);
            SetAnimIdle();
            state = State.Patrol; // Return to patrol state
            pauseTimer = 0f; // Reset patrol pause timer
        }

        if (PlayerDetected())
        {
            state = State.Chase;
            lostSightTimer = 0f;
        }
    }

    IEnumerator AttackRoutine()
    {
        state = State.Attack; // Set state to attack
        isAttacking = true;
        SetVelX(0f); // Stop movement during attack
        SetAnimAttack(); // Trigger attack animation

        yield return new WaitForSeconds(attackWindup); // Wait for windup

        // Perform attack damage
        if (attackPoint != null)
        {
            var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
            foreach (var hit in hits)
            {
                var ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown - attackWindup); // Wait for remaining cooldown
        isAttacking = false; // Attack finished

        // Decide next state based on whether player is still detected
        state = PlayerDetected() ? State.Chase : State.Return;
        if (state == State.Return)
        {
            // Set patrol target to the closer point if returning
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
        Collider2D hit = Physics2D.OverlapBox(boxCenter, detectionBoxSize, 0f, playerLayer);
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
        return hit.collider == null; // If no collider was hit, then we have line of sight
    }

    void SetAnimIdle()
    {
        if (animator != null)
        {
            animator.SetBool(animParamIsFlying, false);
            // Ensure attack trigger is reset if not attacking
            if (!isAttacking) animator.ResetTrigger(animParamAttackTrigger);
        }
    }

    void SetAnimFly()
    {
        if (animator != null) animator.SetBool(animParamIsFlying, true);
    }

    void SetAnimAttack()
    {
        if (animator != null) animator.SetTrigger(animParamAttackTrigger);
    }

    void HandleFlip()
    {
        if (sr == null || player == null) return;
        bool currentlyFacingRight = !sr.flipX;
        bool intendsToFaceRight = currentlyFacingRight; // Default to current direction

        // Determine intended direction based on state
        if (state == State.Chase || state == State.Attack)
            intendsToFaceRight = (player.position.x - transform.position.x) > 0f;
        else if (state == State.Patrol || state == State.Return)
        {
            // If moving, face direction of movement
            if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
                intendsToFaceRight = rb.linearVelocity.x > 0f;
            else
                // If idle or paused, face patrol target
                intendsToFaceRight = (patrolTarget.position.x - transform.position.x) > 0f;
        }

        // Flip if necessary
        if (currentlyFacingRight != intendsToFaceRight) sr.flipX = !intendsToFaceRight;

        // Adjust attack point local position based on flip
        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (sr.flipX ? -1 : 1); // Flip X based on sprite flip
            attackPoint.localPosition = localPos;
        }
    }

    // Gizmos for visual debugging in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 boxCenter = (Vector2)transform.position + detectionBoxOffset;
        Gizmos.DrawWireCube(boxCenter, detectionBoxSize); // Detection zone

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // Attack range

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius); // Attack hit area
        }

        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position); // Patrol path
            Gizmos.DrawSphere(leftPoint.position, 0.1f);
            Gizmos.DrawSphere(rightPoint.position, 0.1f);
        }
    }
}
