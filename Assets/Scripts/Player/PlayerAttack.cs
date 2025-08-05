using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRangeX = 1.5f;
    public float attackRangeY = 1f;
    public LayerMask enemyLayer;
    public Transform attackVFXSpawnPoint;
    public GameObject crimsonSlashVFX1Prefab;
    public GameObject crimsonSlashVFX2Prefab;
    public GameObject crimsonSlashVFXComboPrefab;
    public GameObject crimsonSurgeVFXPrefab;
    public GameObject crimsonAuraVFX;

    // UI elements are now private and will be set by the new UI script
    private GameObject chargeUIPanel;
    private Slider chargeUIFillBar;
    private GameObject abilityUnlockedUIPanel;
    private TMP_Text abilityUnlockedTitleText;
    private TMP_Text abilityUnlockedDescriptionText;
    private Image abilityUnlockedIcon;

    private PlayerMovement playerMovement;
    private SpriteRenderer playerSpriteRenderer;
    private Animator animator;
    private bool hasCrimsonSurge = false;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float requiredChargeTime = 1f;
    private bool gameIsPausedForMessage = false;
    private bool isPerformingCrimsonSurge = false;

    // Public method to check if the player is currently charging
    public bool IsCharging()
    {
        return isCharging;
    }
    
    // New method for the UI panel to register its components
    public void RegisterUI(GameObject chargePanel, Slider chargeBar, GameObject unlockedPanel, TMP_Text titleText, TMP_Text descText, Image icon)
    {
        this.chargeUIPanel = chargePanel;
        this.chargeUIFillBar = chargeBar;
        this.abilityUnlockedUIPanel = unlockedPanel;
        this.abilityUnlockedTitleText = titleText;
        this.abilityUnlockedDescriptionText = descText;
        this.abilityUnlockedIcon = icon;

        // Make sure the UI starts in the correct state
        if (this.chargeUIPanel != null) this.chargeUIPanel.SetActive(false);
        if (this.abilityUnlockedUIPanel != null) this.abilityUnlockedUIPanel.SetActive(false);
    }


    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        // This will find the aura VFX which is also in the scene, just like the UI
        crimsonAuraVFX = GameObject.Find("CrimsonAuraVFX");
        if (crimsonAuraVFX != null)
        {
            crimsonAuraVFX.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameIsPausedForMessage)
        {
            if (Input.anyKeyDown)
            {
                Time.timeScale = 1f;
                gameIsPausedForMessage = false;
                if (abilityUnlockedUIPanel != null)
                {
                    abilityUnlockedUIPanel.SetActive(false);
                }
            }
            return;
        }

        if (hasCrimsonSurge)
        {
            if (Input.GetKey(KeyCode.X))
            {
                if (!isCharging)
                {
                    StartCharge();
                }
                chargeTimer += Time.deltaTime;
                if (chargeUIFillBar != null)
                {
                    chargeUIFillBar.value = chargeTimer / requiredChargeTime;
                }
            }

            if (Input.GetKeyUp(KeyCode.X))
            {
                if (isCharging)
                {
                    StopCharge();
                    if (chargeTimer >= requiredChargeTime)
                    {
                        PerformCrimsonSurge();
                    }
                }
            }
        }
    }
    
    public void GrantCrimsonSurge()
    {
        if (hasCrimsonSurge) return;
        
        hasCrimsonSurge = true;
        ShowAbilityUnlockedMessage("New Ability: Crimson Surge!", "What it does: Hold 'X' to build a powerful charge. Release after one second to unleash a concentrated crimson surge, dealing heavy damage to enemies and breaking fragile walls in its path.");
    }

    private void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
        if (playerMovement != null)
        {
            playerMovement.SetChargingWalkSpeed(true);
        }
        
        if (crimsonAuraVFX != null)
        {
            crimsonAuraVFX.SetActive(true);
        }
        if (chargeUIPanel != null)
        {
            chargeUIPanel.SetActive(true);
        }
    }
    
    private void StopCharge()
    {
        isCharging = false;
        if (playerMovement != null)
        {
            playerMovement.SetChargingWalkSpeed(false);
        }
        
        if (crimsonAuraVFX != null)
        {
            crimsonAuraVFX.SetActive(false);
        }
        if (chargeUIPanel != null)
        {
            chargeUIPanel.SetActive(false);
        }
    }
    
    private void PerformCrimsonSurge()
    {
        if (animator != null)
        {
            isPerformingCrimsonSurge = true;
            animator.SetTrigger("Attack2");
        }
    }

    public void DealCrimsonSurgeDamage()
    {
        if (!isPerformingCrimsonSurge)
        {
            isPerformingCrimsonSurge = false;
            return;
        }

        Vector2 center = attackPoint.position;
        Vector2 size = new Vector2(attackRangeX * 1.5f, attackRangeY * 1.5f);
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        foreach (var enemy in hitEnemies)
        {
            if (enemy.gameObject == this.gameObject || enemy.CompareTag("Player"))
                continue;

            var belerickHealth = enemy.GetComponentInParent<BelerickHealth>();
            if (belerickHealth != null && !belerickHealth.isDead)
            {
                belerickHealth.TakeDamage(100);
                continue;
            }

            var enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(100);
            }
        }
        
        InstantiateAttackVFX(crimsonSurgeVFXPrefab);
        isPerformingCrimsonSurge = false;
    }

    private void ShowAbilityUnlockedMessage(string title, string description)
    {
        Time.timeScale = 0f;
        gameIsPausedForMessage = true;
        if (abilityUnlockedUIPanel != null)
        {
            abilityUnlockedUIPanel.SetActive(true);
        }
        if (abilityUnlockedTitleText != null)
        {
            abilityUnlockedTitleText.text = title;
        }
        if (abilityUnlockedDescriptionText != null)
        {
            abilityUnlockedDescriptionText.text = description;
        }
    }
    
    public void Attack(float damage)
    {
        if (attackPoint == null || playerMovement == null)
            return;

        Vector2 center = attackPoint.position;
        Vector2 size = new Vector2(attackRangeX, attackRangeY);
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        foreach (var enemy in hitEnemies)
        {
            if (enemy.gameObject == this.gameObject || enemy.CompareTag("Player"))
                continue;

            var belerickHealth = enemy.GetComponentInParent<BelerickHealth>();
            if (belerickHealth != null && !belerickHealth.isDead)
            {
                belerickHealth.TakeDamage((int)damage);
                continue;
            }

            var enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)damage);
            }
        }
    }

    public void InstantiateAttackVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null || attackVFXSpawnPoint == null || playerSpriteRenderer == null)
            return;

        GameObject vfxInstance = Instantiate(vfxPrefab, attackVFXSpawnPoint.position, attackVFXSpawnPoint.rotation);
        SpriteRenderer vfxSpriteRenderer = vfxInstance.GetComponent<SpriteRenderer>();
        if (vfxSpriteRenderer == null)
            vfxSpriteRenderer = vfxInstance.GetComponentInChildren<SpriteRenderer>();

        if (vfxSpriteRenderer != null)
        {
            vfxSpriteRenderer.flipX = playerSpriteRenderer.flipX;
        }

        Destroy(vfxInstance, 1.5f);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 1f));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX * 1.5f, attackRangeY * 1.5f, 1f));
    }
}
