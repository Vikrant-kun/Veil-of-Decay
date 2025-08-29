using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class ArchimageAI : MonoBehaviour
{
    // ... (All other public variables are the same) ...

    [Header("Core References")]
    public Animator animator;
    public Transform playerTransform;
    public SpriteRenderer bossSprite;
    public BossIntroManager introManager;
    public CinemachineCamera playerFollowCam;
    public CinemachineCamera bossArenaCam;

    [Header("Magic Prefabs")]
    public GameObject attack1FirePrefab_AOE;
    public GameObject attack1ElectricBombPrefab_AOE;
    public GameObject attack2ThunderPrefab_AOE;
    public GameObject attack3ArrowPrefab_Beam;
    public GameObject attack4DefensePrefab_Self;
    public GameObject attack5CursedPrefab_AOE;
    public Transform magicSpawnPoint;
    public GameObject teleportEndVFXPrefab;

    [Header("AI & FSM Settings")]
    public float decisionCooldown = 4.0f;
    public int attacksBeforeTeleport = 5;
    public List<Transform> teleportPoints;
    public List<int> beamAttackTeleportIndices;
    public float groundLevelY = 0f;
    public float attack2SpawnY = 15f;
    public float attack5SpawnY = 20f;

    [Header("Phase Settings")]
    public float maxHealth = 1200f;
    private float currentHealth;
    public float phase2HealthThreshold = 0.5f;
    public float phase3HealthThreshold = 0.3f;

    [Header("Attack Timings & Details")]
    public float attack1CastTime = 0.5f;
    public float attack1Recovery = 1.0f;
    public float attack2CastTime = 1.0f;
    public float attack2Recovery = 1.5f;
    public float attack3CastTime = 0.7f;
    public float attack3Recovery = 1.5f;
    public float attack4CastTime = 0.3f;
    public float attack4Recovery = 2.0f;
    public float attack5CastTime = 1.2f;
    public float attack5Recovery = 2.5f;
    public float deathAnimDuration = 3.0f;
    public float hurtAnimDuration = 0.5f;
    public float teleportStartAnimDuration = 0.5f;
    public float teleportEndAnimDuration = 0.5f;

    [Header("Attack 4 (Defense) Trigger")]
    private int hitCounter = 0;
    public int hitsToTriggerDefense = 6;

    [Header("Attack 5 (Ultimate) Trigger")]
    private int phase3AttackCounter = 0;
    public int attacksBeforeUltimate = 3;
    
    // --- Private FSM State Variables ---
    private int currentPhase = 1;
    private float cooldownTimer;
    private bool isActionInProgress = false;
    private int currentAttackCounter;
    private int currentTeleportIndex = -1;
    private bool isFacingRight = true;
    private bool isCombatActive = false; // The new "switch" to keep him dormant
    
    void Start()
    {
        currentHealth = maxHealth;
        if (animator == null) animator = GetComponent<Animator>();
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (bossSprite == null) bossSprite = GetComponent<SpriteRenderer>();
        
        currentAttackCounter = attacksBeforeTeleport;
    }

    void Update()
    {
        // NEW GUARD CLAUSE: If combat has not been activated by the Intro Manager, do nothing.
        if (!isCombatActive) return;

        if (isActionInProgress) return;

        if (playerTransform.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (playerTransform.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            isActionInProgress = true;
            DecideNextAction();
        }
    }

    // NEW PUBLIC METHOD: The Intro Manager will call this to start the fight.
    public void ActivateCombat()
    {
        isCombatActive = true;
        Debug.Log("Archimage combat has been activated!");
    }
    
    private void DecideNextAction()
    {
        if (currentAttackCounter <= 0)
        {
            StartCoroutine(TeleportRoutine());
            return;
        }

        switch (currentPhase)
        {
            case 1:
                StartCoroutine(Phase1_Decision());
                break;
            case 2:
                StartCoroutine(Phase2_Decision());
                break;
            case 3:
                StartCoroutine(Phase3_Decision());
                break;
        }
    }

    private IEnumerator Phase1_Decision()
    {
        currentAttackCounter--;
        
        if (beamAttackTeleportIndices.Contains(currentTeleportIndex))
        {
            yield return StartCoroutine(Pattern_BeamAttack());
        }
        else
        {
            if (Random.value > 0.5f)
            {
                yield return StartCoroutine(Pattern_PlayerBomb());
            }
            else
            {
                yield return StartCoroutine(Pattern_GroundAOE());
            }
        }
    }

    private IEnumerator Phase2_Decision()
    {
        currentAttackCounter--;
        phase3AttackCounter++; 
        
        int choice = Random.Range(0, 3);
        if (choice == 0)
        {
            yield return StartCoroutine(Pattern_SkyThunder());
        }
        else if (choice == 1)
        {
            yield return StartCoroutine(Pattern_PlayerBomb());
        }
        else
        {
            yield return StartCoroutine(Pattern_GroundAOE());
        }
    }

    private IEnumerator Phase3_Decision()
    {
        currentAttackCounter--;
        phase3AttackCounter++;

        if(phase3AttackCounter >= attacksBeforeUltimate)
        {
            phase3AttackCounter = 0;
            yield return StartCoroutine(Pattern_UltimateAttack());
        }
        else
        {
            yield return StartCoroutine(Phase2_Decision());
        }
    }

    // ... (All pattern coroutines, spawn methods, etc. are the same as before)
    private IEnumerator Pattern_GroundAOE(){ animator.SetTrigger("DoAttack1"); yield return new WaitForSeconds(attack1CastTime); SpawnAttack1Fire(); yield return new WaitForSeconds(attack1Recovery); ActionComplete(); }
    private IEnumerator Pattern_PlayerBomb(){ animator.SetTrigger("DoAttack1"); yield return new WaitForSeconds(attack1CastTime); SpawnAttack1ElectricBomb(); yield return new WaitForSeconds(attack1Recovery); ActionComplete(); }
    private IEnumerator Pattern_SkyThunder(){ animator.SetTrigger("DoAttack2"); yield return new WaitForSeconds(attack2CastTime); SpawnAttack2Thunder(); yield return new WaitForSeconds(attack2Recovery); ActionComplete(); }
    private IEnumerator Pattern_BeamAttack(){ animator.SetTrigger("DoAttack3"); yield return new WaitForSeconds(attack3CastTime); SpawnAttack3Arrow(); yield return new WaitForSeconds(attack3Recovery); ActionComplete(); }
    private IEnumerator Pattern_Defense(){ animator.SetTrigger("DoAttack4"); yield return new WaitForSeconds(attack4CastTime); SpawnAttack4Defense(); yield return new WaitForSeconds(attack4Recovery); ActionComplete(); }
    private IEnumerator Pattern_UltimateAttack(){ animator.SetTrigger("DoAttack5"); yield return new WaitForSeconds(attack5CastTime); SpawnAttack5Cursed(); yield return new WaitForSeconds(attack5Recovery); ActionComplete(); }
    private void ActionComplete(){ cooldownTimer = decisionCooldown; isActionInProgress = false; }
    public void SpawnAttack1Fire() { if (attack1FirePrefab_AOE != null) { Vector3 playerPos = PlayerMovement.Instance.transform.position; Instantiate(attack1FirePrefab_AOE, new Vector3(playerPos.x, groundLevelY, 0), Quaternion.identity); }}
    public void SpawnAttack1ElectricBomb() { if (attack1ElectricBombPrefab_AOE != null) Instantiate(attack1ElectricBombPrefab_AOE, PlayerMovement.Instance.transform.position, Quaternion.identity); }
    public void SpawnAttack2Thunder() { if (attack2ThunderPrefab_AOE != null) { Vector3 playerPos = PlayerMovement.Instance.transform.position; Instantiate(attack2ThunderPrefab_AOE, new Vector3(playerPos.x, attack2SpawnY, 0), Quaternion.identity); }}
    public void SpawnAttack3Arrow() { if (attack3ArrowPrefab_Beam != null && magicSpawnPoint != null) { GameObject beamInstance = Instantiate(attack3ArrowPrefab_Beam, magicSpawnPoint.position, Quaternion.identity); if (!isFacingRight) { Vector3 newScale = beamInstance.transform.localScale; newScale.x *= -1; beamInstance.transform.localScale = newScale; } } }
    public void SpawnAttack4Defense() { if (attack4DefensePrefab_Self != null) { GameObject circle = Instantiate(attack4DefensePrefab_Self, transform.position, Quaternion.identity); circle.transform.parent = this.transform; } }
    public void SpawnAttack5Cursed() { if (attack5CursedPrefab_AOE != null) { Vector3 playerPos = PlayerMovement.Instance.transform.position; Instantiate(attack5CursedPrefab_AOE, new Vector3(playerPos.x, attack5SpawnY, 0), Quaternion.identity); }}
    private IEnumerator TeleportRoutine()
    {
        animator.SetTrigger("DoTeleportStart");
        yield return new WaitForSeconds(teleportStartAnimDuration);
        bossSprite.enabled = false;

        int newIndex = Random.Range(0, teleportPoints.Count);
        while (teleportPoints.Count > 1 && newIndex == currentTeleportIndex)
        {
            newIndex = Random.Range(0, teleportPoints.Count);
        }
        currentTeleportIndex = newIndex;
        Vector3 destination = teleportPoints[currentTeleportIndex].position;

        if (teleportEndVFXPrefab != null)
        {
            Instantiate(teleportEndVFXPrefab, destination, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(teleportEndAnimDuration);

        transform.position = destination;
        bossSprite.enabled = true;
        currentAttackCounter = attacksBeforeTeleport;
        ActionComplete();
    }
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        hitCounter++;

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathRoutine());
            return;
        }
        
        if (hitCounter >= hitsToTriggerDefense)
        {
            hitCounter = 0;
            isActionInProgress = true;
            StopAllCoroutines();
            StartCoroutine(Pattern_Defense());
        }
        else
        {
            animator.SetTrigger("DoHurt");
        }

        float healthPercentage = currentHealth / maxHealth;
        if (currentPhase == 1 && healthPercentage <= phase2HealthThreshold)
        {
            currentPhase = 2;
            Debug.Log("ARCHIMAGE ENTERS PHASE 2!");
        }
        else if (currentPhase == 2 && healthPercentage <= phase3HealthThreshold)
        {
            currentPhase = 3;
            Debug.Log("ARCHIMAGE ENTERS FINAL PHASE!");
        }
    }
    private IEnumerator DeathRoutine()
    {
        isActionInProgress = true;
        StopAllCoroutines();

        for (int i = 0; i < 4; i++)
        {
            animator.SetTrigger("DoHurt");
            yield return new WaitForSeconds(hurtAnimDuration);
        }
        
        animator.SetTrigger("DoDeath");
        yield return new WaitForSeconds(deathAnimDuration);
        
        if (introManager != null)
        {
            StartCoroutine(introManager.AnimateBars(false));
            introManager.leftWall.SetActive(false);
            introManager.rightWall.SetActive(false);
        }

        if (bossArenaCam != null)
        {
            bossArenaCam.Priority = 9;
        }

        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
}