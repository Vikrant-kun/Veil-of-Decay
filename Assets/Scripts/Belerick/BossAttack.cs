using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public int damage = 20;
    public LayerMask playerLayer;
    public GameObject hitEffect;

    private BelerickHealth belerickHealth;

    void Start()
    {
        belerickHealth = GetComponent<BelerickHealth>();
    }

    public void PerformAttack()
    {
        if (belerickHealth != null && belerickHealth.isDead)
        {
            Debug.Log("❌ Belerick tried to attack while dead. Skipping.");
            return;
        }

        Collider2D playerHit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (playerHit != null)
        {
            PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("💥 Belerick hit the player!");

                if (hitEffect != null)
                    Instantiate(hitEffect, playerHit.transform.position, Quaternion.identity);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
