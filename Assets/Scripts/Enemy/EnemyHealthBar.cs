using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Slider healthBar;

    private EnemyAI enemyAI;
    private Animator animator;

    private float lastFlinchTime = -1f;
    public float flinchCooldown = 0.4f;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;

        animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.value = currentHealth;

        if (currentHealth > 0f)
        {
            if (!enemyAI.IsAttacking() && Time.time - lastFlinchTime >= flinchCooldown)
            {
                StartCoroutine(PlayHitAnimation());
                lastFlinchTime = Time.time;

                if (Random.value < 0.3f) // 30% chance to dodge
                {
                    enemyAI.DoFakeDodge();
                }
            }
        }
        else
        {
            Die();
        }
    }

    IEnumerator PlayHitAnimation()
    {
        animator.Play("Nightborne_hit");
        yield return new WaitForSeconds(0.3f);
    }

    void Die()
    {
        animator.Play("Nightborne_death");
        enemyAI.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f);
    }
}