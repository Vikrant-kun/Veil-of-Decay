using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; 
using TMPro;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance { get; private set; } 

    public Transform attackPoint;
    public float attackRangeX = 1.5f;
    public float attackRangeY = 1f;
    public LayerMask enemyLayer;
    public LayerMask fragileWallLayer; 

    public Transform attackVFXSpawnPoint;
    public GameObject crimsonSlashVFX1Prefab;
    public GameObject crimsonSlashVFX2Prefab;
    public GameObject crimsonSlashVFXComboPrefab;
    public GameObject crimsonSurgeVFXPrefab;
    public GameObject crimsonAuraVFX;

    // UI elements are now PUBLIC so they can be assigned in the Inspector
    public GameObject chargeUIPanel;
    public Slider chargeUIFillBar;
    public GameObject abilityUnlockedUIPanel;
    public TMP_Text abilityUnlockedTitleText;
    public TMP_Text abilityUnlockedDescriptionText;
    public Image abilityUnlockedIcon;

    private PlayerMovement playerMovement; 
    private SpriteRenderer playerSpriteRenderer;
    private Animator animator;
    private bool hasCrimsonSurge = false;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float requiredChargeTime = 1f;
    private bool gameIsPausedForMessage = false;
    private bool isPerformingCrimsonSurge = false;
    
    // comboStep is managed by PlayerMovement, PlayerAttack doesn't need it internally here.
    private int comboStep = 0; // Re-added comboStep for internal consistency if other scripts relied on it

    public bool IsCharging()
    {
        return isCharging;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
            return;
        }

        playerMovement = GetComponent<PlayerMovement>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    
    void Start()
    {
        if (crimsonAuraVFX != null)
        {
            crimsonAuraVFX.SetActive(false);
        }
        if (chargeUIPanel != null)
        {
            chargeUIPanel.SetActive(false);
        }
        if (abilityUnlockedUIPanel != null)
        {
            abilityUnlockedUIPanel.SetActive(false);
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
        
        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer | fragileWallLayer);

        foreach (var obj in hitObjects)
        {
            if ((enemyLayer.value & (1 << obj.gameObject.layer)) > 0)
            {
                if (obj.gameObject == this.gameObject || obj.CompareTag("Player"))
                    continue;

                var belerickHealth = obj.GetComponentInParent<BelerickHealth>();
                if (belerickHealth != null && !belerickHealth.isDead)
                {
                    belerickHealth.TakeDamage(100); 
                    Debug.Log($"<color=blue>PlayerAttack (CrimsonSurge): Hit Belerick ({obj.name}) for 100 damage.</color>");
                    continue;
                }
                var darkSoulHealth = obj.GetComponentInParent<DarkSoulHealth>();
                if (darkSoulHealth != null)
                {
                    darkSoulHealth.TakeDamage(100); 
                    Debug.Log($"<color=blue>PlayerAttack (CrimsonSurge): Hit DarkSoul ({obj.name}) for 100 damage.</color>");
                    continue;
                }
                // --- ADDED GHOSTHEALTH HERE ---
                var ghostHealth = obj.GetComponentInParent<GhostHealth>();
                if (ghostHealth != null)
                {
                    ghostHealth.TakeDamage(100);
                    Debug.Log($"<color=blue>PlayerAttack (CrimsonSurge): Hit Ghost ({obj.name}) for 100 damage.</color>");
                    continue;
                }
                // --- END ADDED GHOSTHEALTH ---
                var enemyHealth = obj.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(100);
                    Debug.Log($"<color=blue>PlayerAttack (CrimsonSurge): Hit Enemy ({obj.name}) for 100 damage.</color>");
                    continue;
                }
            }
            else if ((fragileWallLayer.value & (1 << obj.gameObject.layer)) > 0)
            {
                var fragileWall = obj.GetComponent<FragileWall>();
                if (fragileWall != null)
                {
                    fragileWall.BreakWall();
                    Debug.Log($"<color=blue>PlayerAttack (CrimsonSurge): Broke Fragile Wall ({obj.name}).</color>");
                }
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
    
    public void Attack(float basePhysicalDamage)
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
                belerickHealth.TakeDamage((int)basePhysicalDamage); 
                Debug.Log($"<color=blue>PlayerAttack (Normal): Hit Belerick ({enemy.name}) for {(int)basePhysicalDamage} physical damage.</color>");
                continue;
            }

            var darkSoulHealth = enemy.GetComponentInParent<DarkSoulHealth>();
            if (darkSoulHealth != null)
            {
                darkSoulHealth.TakeDamage(basePhysicalDamage); 
                Debug.Log($"<color=blue>PlayerAttack (Normal): Hit DarkSoul ({enemy.name}) for {basePhysicalDamage} physical damage.</color>");
                continue;
            }
            // --- ADDED GHOSTHEALTH HERE ---
            var ghostHealth = enemy.GetComponentInParent<GhostHealth>();
            if (ghostHealth != null)
            {
                ghostHealth.TakeDamage(basePhysicalDamage);
                Debug.Log($"<color=blue>PlayerAttack (Normal): Hit Ghost ({enemy.name}) for {basePhysicalDamage} physical damage.</color>");
                continue;
            }
            // --- END ADDED GHOSTHEALTH ---
            var enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)basePhysicalDamage);
                Debug.Log($"<color=blue>PlayerAttack (Normal): Hit Enemy ({enemy.name}) for {(int)basePhysicalDamage} physical damage.</color>");
                continue;
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

    public void ResetAttackStates()
    {
        isCharging = false;
        chargeTimer = 0f;
        isPerformingCrimsonSurge = false;
        comboStep = 0; // Reset combo to start from attack1
        gameIsPausedForMessage = false; 
        
        if (playerMovement != null) playerMovement.SetChargingWalkSpeed(false);
        if (crimsonAuraVFX != null) crimsonAuraVFX.SetActive(false);
        if (chargeUIPanel != null) chargeUIPanel.SetActive(false);
        if (abilityUnlockedUIPanel != null) abilityUnlockedUIPanel.SetActive(false);
    }
}
