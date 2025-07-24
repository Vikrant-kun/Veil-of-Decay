using UnityEngine;
using System.Collections;
using System.Linq; 

public class GameRestartManager : MonoBehaviour
{
    public GameObject guardianAngelPrefab;
    public Vector3 angelSpawnOffset = new Vector3(0, 2f, -2f);
    public float angelCinematicDuration = 3.5f;

    public GameObject playerPrefab;
    public Vector3 playerSpawnPosition = new Vector3(0, 0, 0);

    private Transform currentPlayerTransform;
    private CameraFollow mainCameraFollow;

    void Awake()
    {
        GameObject mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCameraObject != null)
        {
            mainCameraFollow = mainCameraObject.GetComponent<CameraFollow>();
            if (mainCameraFollow == null)
            {
                Debug.LogError("GameRestartManager: Main Camera found but no CameraFollow script attached!");
            }
        }
        else
        {
            Debug.LogError("GameRestartManager: Main Camera GameObject not found! Ensure it's tagged 'MainCamera'.");
        }

        if (playerPrefab != null)
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer == null)
            {
                GameObject newPlayerInstance = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
                currentPlayerTransform = newPlayerInstance.transform;
                Debug.Log("GameRestartManager: Instantiated new player in Awake: " + currentPlayerTransform.name);
            }
            else
            {
                currentPlayerTransform = existingPlayer.transform;
                Debug.Log("GameRestartManager: Found existing player in Awake: " + currentPlayerTransform.name);
            }

            if (mainCameraFollow != null && currentPlayerTransform != null)
            {
                mainCameraFollow.SetTarget(currentPlayerTransform);
            }
        }
        else
        {
            Debug.LogError("GameRestartManager: Player Prefab not assigned! Cannot instantiate player.");
        }
    }

    void Start()
    {
        bool restartedFromDeath = PlayerPrefs.GetInt("GameRestartedFromDeath", 0) == 1;

        if (restartedFromDeath)
        {
            Debug.Log("GameRestartManager: Detected restart from death. Triggering Guardian Angel cinematic.");
            // Clear the flag so the next *fresh* start won't trigger this cinematic
            PlayerPrefs.SetInt("GameRestartedFromDeath", 0); 
            PlayerPrefs.Save();

            StartCoroutine(PlayGuardianAngelCinematic());
        }
        else
        {
            Debug.Log("GameRestartManager: Game started normally or not from death. Enabling player controls."); // Log message changed
            // No PlayerPrefs.SetInt("DeathCount", 0) or CurseEffectManager.UpdateCurseLevel(0) here now.
            // The curse is not active in Level 1.

            // Re-enable player controls for normal start
            if (currentPlayerTransform != null)
            {
                currentPlayerTransform.gameObject.SetActive(true);
                PlayerMovement pm = currentPlayerTransform.GetComponent<PlayerMovement>();
                if (pm != null) pm.enabled = true;
                PlayerAttack pa = currentPlayerTransform.GetComponent<PlayerAttack>();
                if (pa != null) pa.enabled = true;

                Rigidbody2D playerRb = currentPlayerTransform.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.bodyType = RigidbodyType2D.Dynamic;
                    playerRb.simulated = true;
                }
            }
        }

        // IMPORTANT: Tell enemies AND spawners about the player after player is known
        if (currentPlayerTransform != null)
        {
            AssignPlayerToEnemies(currentPlayerTransform);
        }
    }

    private void AssignPlayerToEnemies(Transform playerTransform)
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.SetPlayerTarget(playerTransform);
            }
        }

        BelerickAI[] belericks = FindObjectsByType<BelerickAI>(FindObjectsSortMode.None);
        foreach (BelerickAI belerick in belericks)
        {
            if (belerick != null)
            {
                belerick.SetPlayerTarget(playerTransform);
            }
        }

        EnemySpawner[] spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        foreach (EnemySpawner spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.SetPlayerTargetForSpawner(playerTransform);
            }
        }
        Debug.Log("GameRestartManager: Assigned player to all active enemies and spawners.");
    }

    private IEnumerator PlayGuardianAngelCinematic()
    {
        Debug.Log("GameRestartManager: Playing Guardian Angel cinematic.");

        if (currentPlayerTransform != null)
        {
            currentPlayerTransform.gameObject.SetActive(false);
            PlayerMovement pm = currentPlayerTransform.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
            PlayerAttack pa = currentPlayerTransform.GetComponent<PlayerAttack>();
            if (pa != null) pa.enabled = false;

            Rigidbody2D playerRb = currentPlayerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.bodyType = RigidbodyType2D.Kinematic;
                playerRb.simulated = false;
            }
        }

        GameObject angelInstance = null;
        if (guardianAngelPrefab != null && currentPlayerTransform != null)
        {
            Vector3 angelSpawnPos = currentPlayerTransform.position + angelSpawnOffset;
            angelInstance = Instantiate(guardianAngelPrefab, angelSpawnPos, Quaternion.identity);
        }
        else
        {
                Debug.LogWarning("GameRestartManager: Guardian Angel Prefab or Player Transform (after instantiation) not assigned. Cinematic skipped.");
        }

        yield return new WaitForSeconds(angelCinematicDuration);

        if (angelInstance != null)
        {
            Destroy(angelInstance);
        }

        if (currentPlayerTransform != null)
        {
            currentPlayerTransform.gameObject.SetActive(true);
            PlayerMovement pm = currentPlayerTransform.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = true;
            PlayerAttack pa = currentPlayerTransform.GetComponent<PlayerAttack>();
            if (pa != null) pa.enabled = true;

            Rigidbody2D playerRb = currentPlayerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.bodyType = RigidbodyType2D.Dynamic;
                playerRb.simulated = true;
            }
        }
        Debug.Log("GameRestartManager: Guardian Angel cinematic finished. Player re-enabled.");
    }
}