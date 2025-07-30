using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRangeX = 1.5f;
    public float attackRangeY = 1f;
    public LayerMask enemyLayer;

    [Header("VFX Settings")]
    public Transform attackVFXSpawnPoint;
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
        if (attackPoint == null || playerMovement == null)
            return;

        Vector2 center = attackPoint.position;
        Vector2 size = new Vector2(attackRangeX, attackRangeY);
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        foreach (var enemy in hitEnemies)
        {
            if (enemy.gameObject == this.gameObject || enemy.CompareTag("Player"))
                continue;

            Debug.Log("Hit: " + enemy.name);

            var belerickHealth = enemy.GetComponentInParent<BelerickHealth>();
            if (belerickHealth != null && !belerickHealth.isDead)
            {
                belerickHealth.TakeDamage((int)damage);
                continue;
            }

            var enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)damage);
            }
        }
    }



    public void InstantiateAttackVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null || attackVFXSpawnPoint == null || playerSpriteRenderer == null)
            return;

        GameObject vfxInstance = Instantiate(vfxPrefab, attackVFXSpawnPoint.position, attackVFXSpawnPoint.rotation);
        SpriteRenderer vfxSpriteRenderer = vfxInstance.GetComponent<SpriteRenderer>();
        if (vfxSpriteRenderer == null)
            vfxSpriteRenderer = vfxInstance.GetComponentInChildren<SpriteRenderer>();

        if (vfxSpriteRenderer != null)
            vfxSpriteRenderer.flipX = playerSpriteRenderer.flipX;

        Destroy(vfxInstance, 1.5f);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 1f));
    }
}
        