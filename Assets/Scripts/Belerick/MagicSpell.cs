using UnityEngine;

public class MagicSpell : MonoBehaviour
{
    public int damageAmount = 10; // How much damage this spell does
    public float lifetime = 2f;    // How long the spell lasts before disappearing

    void Start()
    {
        // Destroy the spell after its lifetime
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Assuming your Player has a "Player" tag
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
            // You might want the spell to disappear on hit as well
            Destroy(gameObject); 
        }
    }

    // Optional: If you use Rigidbody2D for movement, you might need FixedUpdate
    // void FixedUpdate() { ... }
}