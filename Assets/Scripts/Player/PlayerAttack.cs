using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRangeX = 1.5f;
    public float attackRangeY = 1f;
    public LayerMask enemyLayer;

    public float attackCooldown = 1f; 
    private float lastAttackTime = -Mathf.Infinity;

    private PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void Attack(float damage)
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (attackPoint == null || playerMovement == null)
        {
            Debug.LogWarning("Missing references: attackPoint or playerMovement");
            return;
        }

        Vector2 center = (Vector2)attackPoint.position;
        Vector2 size = new Vector2(attackRangeX, attackRangeY);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Always check the root in case colliders are on child objects
            BelerickHealth belerickHealth = enemy.transform.root.GetComponent<BelerickHealth>();
            if (belerickHealth != null)
            {
                if (!belerickHealth.isDead)
                {
                    belerickHealth.TakeDamage((int)damage);
                    Debug.Log($"Hit Belerick! Damage: {damage}");
                }
                else
                {
                    Debug.Log("✅ Skipped damage — Belerick is already dead.");
                }
                continue;
            }

            if (enemy.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage((int)damage);
                Debug.Log($"Hit general enemy! Damage: {damage}");
                continue;
            }

            Debug.LogWarning($"Hit object '{enemy.name}' on layer '{LayerMask.LayerToName(enemy.gameObject.layer)}' but no health script found.");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 1f));
    }
}
