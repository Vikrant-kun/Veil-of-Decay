using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public Slider healthBarUI;

    [Header("Components")]
    private Animator animator;
    private bool isDead = false;
    private PlayerMovement movement;
    private PlayerAttack attack;
    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        healthBarUI.maxValue = maxHealth;
        healthBarUI.value = currentHealth;

        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        attack = GetComponent<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBarUI.value = currentHealth;

        if (currentHealth > 0)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            StartCoroutine(HandleDeath());
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBarUI.value = currentHealth;
    }

    System.Collections.IEnumerator HandleDeath()
    {
        isDead = true;

        // Stop movement & attack but keep animator on
        if (movement != null) movement.enabled = false;
        if (attack != null) attack.enabled = false;

        animator.SetTrigger("Death");

        // Force one frame delay to allow trigger to register
        yield return null;

        // Wait for the length of the death animation
        float deathAnimDuration = GetAnimationClipLength("Death");
        yield return new WaitForSeconds(deathAnimDuration);

        // Vanish after animation finishes
        gameObject.SetActive(false);

        Debug.Log("💀 Player finished death animation and is now gone.");
    }

    float GetAnimationClipLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 1f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 1f; // fallback if not found
    }

    public bool IsDead()
    {
        return isDead;
    }
}
