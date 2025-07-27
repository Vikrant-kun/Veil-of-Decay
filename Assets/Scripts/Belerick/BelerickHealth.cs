using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BelerickHealth : MonoBehaviour
{
    public int maxHealth = 500;
    private int currentHealth;

    public float rageThreshold = 0.3f;
    private bool hasEnteredRageMode = false;

    public Slider healthBarUI;

    private Animator anim;
    private Rigidbody2D rb;
    private BossAttack bossAttack;
    private BelerickAI belerickAI;
    public bool isDead = false;

    public TriangleGate triangleGate;
    public GameObject demonGatePrefab;
    public GameObject angelPrefab;
    public Transform spawnPoint_DemonGate;
    public Transform spawnPoint_Angel;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bossAttack = GetComponent<BossAttack>();
        belerickAI = GetComponent<BelerickAI>();

        currentHealth = maxHealth;

        if (healthBarUI != null)
        {
            healthBarUI.maxValue = maxHealth;
            healthBarUI.value = currentHealth;
            healthBarUI.gameObject.SetActive(true); 
        }

        if (belerickAI == null)
            Debug.LogWarning("BelerickAI not found!");
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarUI != null)
            healthBarUI.value = currentHealth;

        if (!hasEnteredRageMode && currentHealth <= maxHealth * rageThreshold)
        {
            hasEnteredRageMode = true;
            belerickAI?.EnterRageMode();
            Debug.Log("💢 Rage Mode Activated!");
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDeath());
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarUI != null)
            healthBarUI.value = currentHealth;
    }

    private IEnumerator HandleDeath()
    {
        isDead = true;
        Debug.Log("💀 Belerick is dead and frozen.");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static; // Make him static so he doesn't fall through ground
        }
        
        if (belerickAI != null)
            belerickAI.enabled = false; // Disable AI

        if (bossAttack != null)
            bossAttack.enabled = false; // Disable attacks

        if (anim != null)
        {
            anim.SetTrigger("Death"); // Play death animation
            // Ensure the "Death" animation state in the Animator has "Exit Time" checked
            // and maybe a small Exit Time value (e.g., 0.9) if you want it to finish before moving on.
        }

        if (healthBarUI != null)
        {
            healthBarUI.gameObject.SetActive(false); // Hide health bar
        }

        // Wait for the death animation to potentially play
        // Note: anim.GetCurrentAnimatorStateInfo(0).length might give the length of the *current* state
        // which might not be "Death" if the trigger just fired.
        // A safer way is to have a known length for the death anim, or rely on an animation event.
        // For now, keeping the WaitForSeconds to at least let it start.
        float deathAnimLength = GetAnimationClipLength("Death"); // Assuming your death animation clip is named "Death"
        Debug.Log($"BelerickHealth: Waiting for {deathAnimLength} seconds for death animation.");
        yield return new WaitForSeconds(deathAnimLength);


        // --- IMPORTANT: Removed the Destroy(gameObject); line here ---
        // This will make Belerick remain in the scene after death.
        // Destroy(gameObject); // REMOVED OR COMMENTED OUT THIS LINE!

        // Trigger scene elements *after* the death animation has largely played
        if (triangleGate != null)
            triangleGate.OpenGate();

        if (demonGatePrefab != null && spawnPoint_DemonGate != null)
            Instantiate(demonGatePrefab, spawnPoint_DemonGate.position, Quaternion.identity);

        if (angelPrefab != null && spawnPoint_Angel != null)
            Instantiate(angelPrefab, spawnPoint_Angel.position, Quaternion.identity);

        // If LoreTransitionManager should only activate *after* Belerick is "dead and laid out"
        // you might want to call it here.
        // Example: FindObjectOfType<LoreTransitionManager>()?.StartLoreTransition();
    }

    public void Die()
    {
        Debug.Log("💀 Belerick is dead and frozen. (Called Deprecated Die method)");
        // This method is not used by TakeDamage, so it won't be called normally.
        // The HandleDeath coroutine is what's used.
    }

    // Helper method to get animation clip length
    float GetAnimationClipLength(string clipName)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
            return 0f; // Return 0 if animator not set up

        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        Debug.LogWarning($"BelerickHealth: Animation clip '{clipName}' not found in Animator Controller. Returning default length of 1 second.");
        return 1f; // Default length if clip not found
    }
}