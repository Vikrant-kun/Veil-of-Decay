using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BelerickHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 500;
    private int currentHealth;

    [Header("Rage Mode Settings")]
    public float rageThreshold = 0.3f;
    private bool hasEnteredRageMode = false;

    [Header("UI")]
    public Slider healthBarUI;

    [Header("Components")]
    private Animator animator;
    public bool isDead = false;
    private BelerickAI belerickAI;

    [Header("Post Boss")]
    public TriangleGate triangleGate;
    public GameObject demonGatePrefab;
    public GameObject angelPrefab;
    public Transform spawnPoint_DemonGate;
    public Transform spawnPoint_Angel;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (healthBarUI != null)
        {
            healthBarUI.maxValue = maxHealth;
            healthBarUI.value = currentHealth;
        }

        animator = GetComponent<Animator>();
        belerickAI = GetComponent<BelerickAI>();

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

        animator.SetTrigger("Death");
        Debug.Log("☠️ Death triggered.");

        float deathDuration = GetAnimationClipLength("Belerick_death");
        yield return new WaitForSeconds(deathDuration);

        animator.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        this.enabled = false;
        if (belerickAI != null)
            belerickAI.enabled = false;

        Debug.Log("💀 Belerick is dead and frozen.");

        triangleGate?.OpenGate();

        if (demonGatePrefab && spawnPoint_DemonGate)
            Instantiate(demonGatePrefab, spawnPoint_DemonGate.position, Quaternion.identity);

        if (angelPrefab && spawnPoint_Angel)
            Instantiate(angelPrefab, spawnPoint_Angel.position, Quaternion.identity);
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
        return 1f;
    }
}
