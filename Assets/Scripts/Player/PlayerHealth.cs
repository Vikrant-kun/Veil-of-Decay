using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Slider healthBarUI;

    [Header("Healing Flask")]
    [Tooltip("How many flasks the player starts with")]
    public int flaskCount = 2;
    [Tooltip("Amount healed per flask")]
    public int healAmount = 40;
    [Tooltip("Key to use a flask")]
    public KeyCode healKey = KeyCode.Q;
    [Tooltip("Assign the flask icons in left-to-right order")]
    public Image[] flaskIcons; 

    // components
    private Animator animator;
    private PlayerMovement movement;
    private PlayerAttack attack;
    private SpriteRenderer sr;
    private bool isDead = false;

    // --- NEW: Reference to your DeathHandler ---
    private DeathHandler deathHandler;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarUI != null)
        {
            healthBarUI.maxValue = maxHealth;
            healthBarUI.value = currentHealth;
        }

        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        attack = GetComponent<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();

        UpdateFlaskIcons();

        // --- NEW: Find the DeathHandler in the scene ---
        deathHandler = FindAnyObjectByType<DeathHandler>();
        if (deathHandler == null)
        {
            Debug.LogError("PlayerHealth: DeathHandler not found in scene! Player death will not trigger the custom scene.", this);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(healKey))
            TryUseFlask();
    }

    private IEnumerator GlowGreenSmooth()
    {
        Color originalColor = sr.color;
        Color glowColor = Color.green;

        float duration = 0.25f;

        for (int i = 0; i < 2; i++)
        {
            // Fade to green
            float t = 0;
            while (t < 1f)
            {
                sr.color = Color.Lerp(originalColor, glowColor, t);
                t += Time.deltaTime / duration;
                yield return null;
            }

            // Fade back to original
            t = 0;
            while (t < 1f)
            {
                sr.color = Color.Lerp(glowColor, originalColor, t);
                t += Time.deltaTime / duration;
                yield return null;
            }
            sr.color = originalColor;
        }
    }


    private void TryUseFlask()
    {
        if (flaskCount <= 0 || currentHealth >= maxHealth)
            return;

        flaskCount--;
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);

        if (healthBarUI != null)
            healthBarUI.value = currentHealth;

        StartCoroutine(GlowGreenSmooth());

        UpdateFlaskIcons();
    }



    private void UpdateFlaskIcons()
    {
        for (int i = 0; i < flaskIcons.Length; i++)
        {
            flaskIcons[i].enabled = i < flaskCount;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarUI != null)
            healthBarUI.value = currentHealth;

        if (currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }
        else 
        {

            if (deathHandler != null)
            {
                deathHandler.TriggerDeathScene();
                Debug.Log("PlayerHealth: Triggered global death scene via DeathHandler."); // For debugging
            }
            else
            {

                Debug.LogWarning("PlayerHealth: DeathHandler not found. Handling death locally.");
                StartCoroutine(LocalHandleDeathFallback()); // Renamed original HandleDeath for clarity
            }


            isDead = true; 
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

    private IEnumerator LocalHandleDeathFallback() // Renamed from HandleDeath
    {
        if (movement != null) movement.enabled = false;
        if (attack != null) attack.enabled = false;

        animator.SetTrigger("Death");

        yield return null;
        float deathAnimDuration = GetAnimationClipLength("Death");
        yield return new WaitForSeconds(deathAnimDuration);
        gameObject.SetActive(false);
    }

    // --- MODIFIED: ResetHealth now truly resets for a respawn ---
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false; // Player is no longer dead

        if (healthBarUI != null)
            healthBarUI.value = currentHealth;

        // Re-enable player GameObject if it was deactivated
        gameObject.SetActive(true); 

        if (movement != null) movement.enabled = true;
        if (attack != null) attack.enabled = true;

        // Set visuals back to normal if needed
        sr.color = Color.white;
        animator.Rebind(); // Resets animator to default state
        animator.Play("idle"); // Or whatever your default animation is
    }

    private float GetAnimationClipLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 1f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName)
                return clip.length;

        return 1f;
    }

    public bool IsDead() => isDead;
}