using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 4f;
    public int attackDamage = 15;
    public LayerMask playerLayer;

    public Transform attackPoint;

    public void PerformAttack()
    {
        // Detect all colliders within attack range on the player layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);

        if (hits.Length == 0)
        {
            Debug.Log("🏃 No targets in attack range!");
        }

        foreach (var hit in hits)
        {
            Transform root = hit.transform.root;
            Debug.Log($"🎯 Overlap hit: {root.name}");

            if (root.TryGetComponent(out PlayerHealth ph))
            {
                ph.TakeDamage(attackDamage);
                Debug.Log($"💥 HIT! {root.name} took {attackDamage} damage!");
            }
            else
            {
                Debug.LogWarning("⚠️ Hit object has no PlayerHealth!");
            }
        }
    }

    // Just for debugging: show the hitbox in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
