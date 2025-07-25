
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
    private Animator anim;
    private Rigidbody2D rb;
    private BossAttack bossAttack;
    private BelerickAI belerickAI;
    public bool isDead = false;

    [Header("Post Boss")]
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

        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        // Disable AI and attack
        if (belerickAI != null)
            belerickAI.enabled = false;

        if (bossAttack != null)
            bossAttack.enabled = false;

        // Wait for the death anim to finish
        float animLength = anim != null ? anim.GetCurrentAnimatorStateInfo(0).length : 2f;
        yield return new WaitForSeconds(animLength);

        // Freeze body
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        if (triangleGate != null)
            triangleGate.OpenGate();

        if (demonGatePrefab != null && spawnPoint_DemonGate != null)
            Instantiate(demonGatePrefab, spawnPoint_DemonGate.position, Quaternion.identity);

        if (angelPrefab != null && spawnPoint_Angel != null)
            Instantiate(angelPrefab, spawnPoint_Angel.position, Quaternion.identity);
    }

    // Deprecated — not used anymore
    public void Die()
    {
        Debug.Log("💀 Belerick is dead and frozen.");
    }

    float GetAnimationClipLength(string clipName)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
            return 1f;

        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 1f;
    }
}
