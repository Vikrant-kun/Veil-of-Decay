// PlayerAttack.cs
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRangeX = 1.5f;
    public float attackRangeY = 1f;
    public LayerMask enemyLayer;

    public float attackCooldown = 1f;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("VFX Settings")]
    public Transform attackVFXSpawnPoint;

    // These public fields are for assignment in the Inspector.
    // They are accessed by PlayerMovement to pass the correct prefab.
    public GameObject crimsonSlashVFX1Prefab;
    public GameObject crimsonSlashVFX2Prefab;
    public GameObject crimsonSlashVFXComboPrefab;

    private PlayerMovement playerMovement;
    private SpriteRenderer playerSpriteRenderer;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Attack(float damage)
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (attackPoint == null || playerMovement == null)
        {
            return;
        }

        Vector2 center = (Vector2)attackPoint.position;
        Vector2 size = new Vector2(attackRangeX, attackRangeY);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            BelerickHealth belerickHealth = enemy.transform.root.GetComponent<BelerickHealth>();
            if (belerickHealth != null)
            {
                if (!belerickHealth.isDead)
                {
                    belerickHealth.TakeDamage((int)damage);
                }
                continue;
            }

            if (enemy.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage((int)damage);
                continue;
            }
        }
    }

    // This method is called by PlayerMovement to instantiate and flip the VFX
    public void InstantiateAttackVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null || attackVFXSpawnPoint == null || playerSpriteRenderer == null)
        {
            return;
        }

        GameObject vfxInstance = Instantiate(vfxPrefab, attackVFXSpawnPoint.position, attackVFXSpawnPoint.rotation);

        SpriteRenderer vfxSpriteRenderer = vfxInstance.GetComponent<SpriteRenderer>();
        if (vfxSpriteRenderer == null)
        {
            vfxSpriteRenderer = vfxInstance.GetComponentInChildren<SpriteRenderer>();
        }

        if (vfxSpriteRenderer != null)
        {
            vfxSpriteRenderer.flipX = playerSpriteRenderer.flipX;
        }

        Destroy(vfxInstance, 1.5f); // Assuming 1.5s is a good general lifetime for your attack VFX
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 1f));
    }
}