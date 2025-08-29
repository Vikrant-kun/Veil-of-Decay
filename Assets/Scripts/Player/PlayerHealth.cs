using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; 
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Slider healthBarUI;

    public int currentHealthFlasks = 2;
    public int maxHealthFlasks = 3;
    public int healAmount = 40;
    public KeyCode healKey = KeyCode.Q;
    public Image[] flaskIcons;

    private Animator animator;
    private PlayerMovement movement;
    private PlayerAttack attack;
    private SpriteRenderer sr;
    private bool isDead = false;

    private DeathHandler deathHandler;
    private Coroutine glowRoutine;

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

        deathHandler = FindAnyObjectByType<DeathHandler>();
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(healKey))
            TryUseFlask();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Hazard"))
        {
            TakeDamage(maxHealth);

            if (movement != null)
            {
                movement.enabled = false;
            }
        }
    }

    private IEnumerator GlowGreenSmooth()
    {
        Color originalColor = sr.color;
        Color glowColor = Color.green;

        float duration = 0.25f;

        for (int i = 0; i < 2; i++)
        {
            float t = 0;
            while (t < 1f)
            {
                sr.color = Color.Lerp(originalColor, glowColor, t);
                t += Time.deltaTime / duration;
                yield return null;
            }

            t = 0;
            while (t < 1f)
            {
                sr.color = Color.Lerp(glowColor, originalColor, t);
                t += Time.deltaTime / duration;
                yield return null;
            }
        }
        sr.color = originalColor;
        glowRoutine = null;
    }

    private void TryUseFlask()
    {
        if (currentHealthFlasks <= 0 || currentHealth >= maxHealth)
            return;

        currentHealthFlasks--;
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);

        if (healthBarUI != null)
            healthBarUI.value = currentHealth;

        if (glowRoutine != null)
        {
            StopCoroutine(glowRoutine);
            sr.color = Color.white;
        }
        glowRoutine = StartCoroutine(GlowGreenSmooth());

        UpdateFlaskIcons();
    }



    private void UpdateFlaskIcons()
    {
        for (int i = 0; i < flaskIcons.Length; i++)
        {
            flaskIcons[i].enabled = i < currentHealthFlasks;
        }
    }

    // --- THIS METHOD HAS BEEN UPDATED ---
    public void TakeDamage(float amount) // Changed from int to float
    {
        if (isDead) return;

        currentHealth -= (int)amount; // Convert the float to an int for health calculation
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
            }
            else
            {
                StartCoroutine(LocalHandleDeathFallback());
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

    public void IncreaseMaxHealth(int newMaxHp)
    {
        if (newMaxHp > maxHealth)
        {
            maxHealth = newMaxHp;
            currentHealth = maxHealth;

            if (healthBarUI != null)
            {
                healthBarUI.maxValue = maxHealth;
                healthBarUI.value = currentHealth;
            }
        }
    }

    public void AddHealthFlask(int amount)
    {
        currentHealthFlasks = Mathf.Min(currentHealthFlasks + amount, maxHealthFlasks);
        UpdateFlaskIcons();
    }

    private IEnumerator LocalHandleDeathFallback()
    {
        if (movement != null) movement.enabled = false;
        if (attack != null) attack.enabled = false;

        animator.SetTrigger("Death");

        yield return null;
        float deathAnimDuration = GetAnimationClipLength("Death");
        yield return new WaitForSeconds(deathAnimDuration);
        gameObject.SetActive(false);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (healthBarUI != null)
            healthBarUI.value = currentHealth;

        gameObject.SetActive(true);

        if (movement != null) movement.enabled = true;
        if (attack != null) attack.enabled = true;

        sr.color = Color.white;
        animator.Rebind();
        animator.Play("Idle");
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

    public void ResetStateAndMove(Vector3 spawnPoint)
    {
        gameObject.SetActive(true);

        currentHealth = maxHealth;
        isDead = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.position = spawnPoint;

        Animator playerAnimator = GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            playerAnimator.Play("Idle");
        }
    }
}