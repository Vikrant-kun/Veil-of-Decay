using UnityEngine;

public class Lvl2_GMS : MonoBehaviour
{
    public static Lvl2_GMS Instance { get; private set; }

    public PlayerHealth playerHealth;
    public Transform defaultRespawnPoint;

    private Vector3 currentRespawnPoint;

    // --- NEW: Reference to the currently active RespawnAltar ---
    private RespawnAltar currentActiveAltar; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (defaultRespawnPoint != null)
        {
            currentRespawnPoint = defaultRespawnPoint.position;
            Debug.Log($"GameRestartManager ({gameObject.scene.name}): Default respawn point set to {currentRespawnPoint}");
        }
        else
        {
            Debug.LogWarning($"GameRestartManager ({gameObject.scene.name}): Default Respawn Point is not assigned! Player might not respawn correctly. Using manager's position as fallback.", this);
            currentRespawnPoint = transform.position;
        }

        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError($"GameRestartManager ({gameObject.scene.name}): PlayerHealth component not found in scene! Cannot manage player respawn.", this);
            }
        }
    }

    /// <summary>
    /// Sets a new respawn point and updates the active altar.
    /// Called by a RespawnAltar upon activation.
    /// </summary>
    /// <param name="newPoint">The position of the activated altar.</param>
    /// <param name="activatedAltar">The RespawnAltar instance that was just activated.</param>
    public void SetRespawnPoint(Vector3 newPoint, RespawnAltar activatedAltar) // --- CHANGED: Now takes the activating altar ---
    {
        // If there was a previously active altar and it's not the one we're currently activating
        if (currentActiveAltar != null && currentActiveAltar != activatedAltar)
        {
            currentActiveAltar.DeactivateVisuals(); // --- NEW: Tell the old altar to turn off its visuals ---
            currentActiveAltar.isActivated = false; // Reset its internal state as well
            Debug.Log($"GameRestartManager: Deactivated previous altar at {currentActiveAltar.transform.position}");
        }

        currentRespawnPoint = newPoint;
        currentActiveAltar = activatedAltar; // --- NEW: Store reference to the newly activated altar ---
        currentActiveAltar.isActivated = true; // Ensure the new altar knows it's active internally

        Debug.Log($"GameRestartManager ({gameObject.scene.name}): Respawn point updated to: {currentRespawnPoint} by altar {activatedAltar.name}");
    }

    /// <summary>
    /// Handles respawning the player. Typically called by a DeathManager or PlayerHealth.
    /// </summary>
    public void RespawnPlayer()
    {
        if (playerHealth != null)
        {
            Debug.Log($"GameRestartManager ({gameObject.scene.name}): Attempting to respawn player at {currentRespawnPoint}");
            playerHealth.ResetStateAndMove(currentRespawnPoint);
        }
        else
        {
            Debug.LogError($"GameRestartManager ({gameObject.scene.name}): PlayerHealth reference is missing! Cannot respawn player.", this);
        }
    }
}