using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DarkSoulHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    public bool IsDead { get; private set; } = false;

    [Header("UI References")]
    public Slider healthBar;

    private DarkSoul darkSoulAI;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private SpriteRenderer sr;

    [Header("Animator Params")]
    public string animParamDeathTrigger = "Death";

    void Awake()
    {
        animator = GetComponent<Animator>();
        darkSoulAI = GetComponent<DarkSoul>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            healthBar.gameObject.SetActive(false); // Hide health bar initially
        }
        ResetEnemyState(); // Ensure initial state is healthy and visible/hidden as per design
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            healthBar.gameObject.SetActive(true); // Show health bar when taking damage
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashColor(Color.white, 0.1f));
        }
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (animator != null)
        {
            animator.SetTrigger(animParamDeathTrigger);
        }

        if (darkSoulAI != null)
        {
            darkSoulAI.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
            rb.gravityScale = 0f;
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        StartCoroutine(DisableGameObjectAfterDelay(GetAnimationLength(animParamDeathTrigger) + 0.1f));
    }

    private IEnumerator DisableGameObjectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private IEnumerator FlashColor(Color flashColor, float duration)
    {
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(duration);
            sr.color = originalColor;
        }
    }

    private float GetAnimationLength(string triggerName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 1f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.ToLower().Contains("death"))
            {
                return clip.length;
            }
        }
        return 1f;
    }

    public void ResetEnemyState()
    {
        currentHealth = maxHealth;
        IsDead = false;

        gameObject.SetActive(true);

        if (darkSoulAI != null) darkSoulAI.enabled = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        if (enemyCollider != null) enemyCollider.enabled = true;
        if (sr != null) sr.enabled = true;

        if (animator != null)
        {
            animator.Rebind();
            animator.Play("idle");
            animator.ResetTrigger(animParamDeathTrigger);
        }

        if (healthBar != null)
        {
            healthBar.value = maxHealth;
            healthBar.gameObject.SetActive(false);
        }
    }
}
