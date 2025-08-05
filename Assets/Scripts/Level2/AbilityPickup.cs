using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the PlayerAttack component from the player
            PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();

            if (playerAttack != null)
            {
                // Call the function on the player to grant the ability
                playerAttack.GrantCrimsonSurge();
                
                // You can add a particle effect or sound here
                
                // Destroy the pickup object so it can't be picked up again
                Destroy(gameObject);
            }
        }
    }
}