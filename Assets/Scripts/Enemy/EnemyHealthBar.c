using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public EnemyAI enemy;

    void Update()
    {
        if (enemy != null)
        {
            healthSlider.value = enemy.currentHealth;
        }
    }

    public void SetMaxHealth(float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }
}
