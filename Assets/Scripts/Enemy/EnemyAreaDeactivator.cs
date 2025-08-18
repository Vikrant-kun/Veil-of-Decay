using UnityEngine;
using System.Collections.Generic;

public class EnemyAreaDeactivator : MonoBehaviour
{
    [Header("Enemies to Deactivate")]
    [Tooltip("Drag all DarkSoul enemies that belong to this area here.")]
    public List<DarkSoul> darkSoulsInArea = new List<DarkSoul>();
    [Tooltip("Drag all Ghost enemies that belong to this area here.")]
    public List<GhostAI> ghostsInArea = new List<GhostAI>(); // Added for GhostAI

    [Header("Deactivation Settings")]
    [Tooltip("If true, this area will deactivate enemies only once per play session (until player respawns).")]
    public bool deactivateOnce = true;
    private bool hasDeactivatedThisSession = false;

    void Awake()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider == null)
        {
            Debug.LogError("EnemyAreaDeactivator: No Collider2D found on " + gameObject.name + ". A trigger collider is required!", this);
            enabled = false; // Disable script if no collider to prevent further errors
        }
        else
        {
            if (!zoneCollider.isTrigger)
            {
                Debug.LogWarning("EnemyAreaDeactivator: Collider on " + gameObject.name + " is NOT set to Is Trigger. It should be a trigger!", this);
            }
            Debug.Log($"EnemyAreaDeactivator: Awake on {gameObject.name}. Collider found: {zoneCollider.GetType().Name}, IsTrigger: {zoneCollider.isTrigger}", this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"EnemyAreaDeactivator: OnTriggerEnter2D fired for {gameObject.name} by {other.name} (Tag: {other.tag})", this);

        if (other.CompareTag("Player"))
        {
            Debug.Log($"EnemyAreaDeactivator: Player detected by {gameObject.name}. HasDeactivatedThisSession: {hasDeactivatedThisSession}, DeactivateOnce: {deactivateOnce}", this);
            
            if (!hasDeactivatedThisSession || !deactivateOnce)
            {
                Debug.Log($"EnemyAreaDeactivator: Conditions met to deactivate enemies for {gameObject.name}.");
                DeactivateEnemies();
                if (deactivateOnce)
                {
                    hasDeactivatedThisSession = true;
                    Debug.Log($"EnemyAreaDeactivator: 'Deactivate Once' enabled, marking as deactivated for this session.");
                }
            } else {
                Debug.Log($"EnemyAreaDeactivator: Skipping deactivation for {gameObject.name}. Already deactivated this session or 'deactivateOnce' is true.", this);
            }
        } else {
            Debug.Log($"EnemyAreaDeactivator: Collider entered by non-player object: {other.name} (Tag: {other.tag})", this);
        }
    }

    void DeactivateEnemies()
    {
        Debug.Log($"EnemyAreaDeactivator: Attempting to deactivate enemies for {gameObject.name}. DarkSouls: {darkSoulsInArea.Count}, Ghosts: {ghostsInArea.Count}");

        // Deactivate DarkSoul enemies
        if (darkSoulsInArea.Count == 0)
        {
            Debug.LogWarning($"EnemyAreaDeactivator: 'darkSoulsInArea' list is EMPTY on {gameObject.name}. No DarkSouls to deactivate!", this);
        }
        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                DarkSoulHealth darkSoulHealth = darkSoul.GetComponent<DarkSoulHealth>();
                if (darkSoulHealth != null)
                {
                    if (!darkSoulHealth.IsDead) // Only deactivate if not already dead
                    {
                        darkSoul.gameObject.SetActive(false);
                        Debug.Log($"EnemyAreaDeactivator: Deactivated active Dark Soul: {darkSoul.name}");
                    }
                    else
                    {
                        Debug.Log($"EnemyAreaDeactivator: Dark Soul {darkSoul.name} is already dead, skipping deactivation.");
                    }
                }
                else
                {
                    Debug.LogWarning($"EnemyAreaDeactivator: DarkSoul '{darkSoul.name}' is missing DarkSoulHealth component.", darkSoul);
                }
            }
            else
            {
                Debug.LogWarning($"EnemyAreaDeactivator: Null reference in 'darkSoulsInArea' list for {gameObject.name}.", this);
            }
        }

        // Deactivate GhostAI enemies
        if (ghostsInArea.Count == 0)
        {
            Debug.LogWarning($"EnemyAreaDeactivator: 'ghostsInArea' list is EMPTY on {gameObject.name}. No Ghosts to deactivate!", this);
        }
        foreach (GhostAI ghost in ghostsInArea)
        {
            if (ghost != null)
            {
                GhostHealth ghostHealth = ghost.GetComponent<GhostHealth>(); // Assuming GhostHealth script
                if (ghostHealth != null)
                {
                    if (!ghostHealth.IsDead) // Only deactivate if not already dead
                    {
                        ghost.gameObject.SetActive(false);
                        Debug.Log($"EnemyAreaDeactivator: Deactivated active Ghost: {ghost.name}");
                    }
                    else
                    {
                        Debug.Log($"EnemyAreaDeactivator: Ghost {ghost.name} is already dead, skipping deactivation.");
                    }
                }
                else
                {
                    Debug.LogWarning($"EnemyAreaDeactivator: Ghost '{ghost.name}' is missing GhostHealth component. Please ensure it has one.", ghost);
                }
            }
            else
            {
                Debug.LogWarning($"EnemyAreaDeactivator: Null reference in 'ghostsInArea' list for {gameObject.name}.", this);
            }
        }
    }

    public void ResetDeactivator()
    {
        Debug.Log($"EnemyAreaDeactivator: Resetting deactivator for {gameObject.name}. Re-enabling all listed enemies.");
        hasDeactivatedThisSession = false; // Allow deactivation again

        // Reset DarkSoul enemies
        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                darkSoul.gameObject.SetActive(true); // Re-activate the GameObject
                Debug.Log($"EnemyAreaDeactivator: Re-enabled Dark Soul: {darkSoul.name}");
                // Note: Full reset (health, AI state) is typically handled by the Activator upon re-entry or a dedicated GameRestartManager
            }
        }

        // Reset GhostAI enemies
        foreach (GhostAI ghost in ghostsInArea)
        {
            if (ghost != null)
            {
                ghost.gameObject.SetActive(true); // Re-activate the GameObject
                Debug.Log($"EnemyAreaDeactivator: Re-enabled Ghost: {ghost.name}");
                // Note: Full reset (health, AI state) is typically handled by the Activator upon re-entry or a dedicated GameRestartManager
            }
        }
    }

    void OnDrawGizmos()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f); // Red transparent for deactivator
            Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
            Gizmos.color = new Color(1, 0, 0, 0.8f); // More solid red for wireframe
            Gizmos.DrawWireCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        }
    }
}
