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

[Tooltip("How many flasks the player currently has")]

public int currentHealthFlasks = 2;

[Tooltip("Maximum flasks the player can carry")]

public int maxHealthFlasks = 3;

[Tooltip("Amount healed per flask")]

public int healAmount = 40;

[Tooltip("Key to use a flask")]

public KeyCode healKey = KeyCode.Q;

[Tooltip("Assign the flask icons in left-to-right order")]

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



Debug.Log($"[PlayerHealth] Start: Initial currentHealthFlasks = {currentHealthFlasks}"); // Debug log added

UpdateFlaskIcons();



deathHandler = FindAnyObjectByType<DeathHandler>();

if (deathHandler == null)

{

Debug.LogError("PlayerHealth: DeathHandler not found in scene! Player death will not trigger the custom scene.");

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

Debug.Log("PlayerHealth: Triggered global death scene via DeathHandler.");

}

else

{

Debug.LogWarning("PlayerHealth: DeathHandler not found. Handling death locally.");

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

Debug.Log($"Player Max HP increased to: {maxHealth}");



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

Debug.Log($"Player gained {amount} health flask(s). Total: {currentHealthFlasks}/{maxHealthFlasks}");

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

animator.Play("idle");

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