using UnityEngine;

public class BossEnemy1AI : MonoBehaviour
{
    public Transform player;
    public Transform visionPoint;
    public float visionRange = 8f;
    public float attackRange = 3f;
    public LayerMask playerLayer;

    void Update()
    {
        float dist = Vector2.Distance(visionPoint.position, player.position);

        if (dist <= visionRange)
        {
            // Chase or face player
            if (dist <= attackRange)
            {
                Attack();
            }
            else
            {
                MoveTowardPlayer();
            }
        }
    }

    void Attack()
    {
        // normal or AOE attack
    }

    void MoveTowardPlayer()
    {
        // walk to player
    }
}
