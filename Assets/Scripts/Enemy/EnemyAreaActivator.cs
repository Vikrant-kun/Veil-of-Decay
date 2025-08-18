using UnityEngine;
using System.Collections.Generic;

public class EnemyAreaActivator : MonoBehaviour
{
    [Header("Enemies to Activate")]
    [Tooltip("Drag all DarkSoul enemies that belong to this area here.")]
    public List<DarkSoul> darkSoulsInArea = new List<DarkSoul>();
    [Tooltip("Drag all Ghost enemies that belong to this area here.")]
    public List<GhostAI> ghostsInArea = new List<GhostAI>(); // Added for GhostAI

    [Header("Activation Settings")]
    [Tooltip("If true, this area will activate enemies only once per play session (until player respawns).")]
    public bool activateOnce = true; 
    private bool hasActivatedThisSession = false;

    void Awake()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider == null)
        {
            Debug.LogError("EnemyAreaActivator: No Collider2D found on " + gameObject.name + ". A trigger collider is required!", this);
            enabled = false; // Disable script if no collider to prevent further errors
        }
        else
        {
            if (!zoneCollider.isTrigger)
            {
                Debug.LogWarning("EnemyAreaActivator: Collider on " + gameObject.name + " is not set to Is Trigger. It should be a trigger!", this);
            }
            Debug.Log($"EnemyAreaActivator: Awake on {gameObject.name}. Collider found: {zoneCollider.GetType().Name}, IsTrigger: {zoneCollider.isTrigger}", this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && (!hasActivatedThisSession || !activateOnce))
        {
            Debug.Log($"EnemyAreaActivator: Player entered {gameObject.name}. Activating enemies.");
            ActivateEnemies(other.transform); // Pass player transform directly
            if (activateOnce)
            {
                hasActivatedThisSession = true; 
            }
        }
    }

    void ActivateEnemies(Transform playerTransform) // Now takes playerTransform as argument
    {
        // Activate DarkSoul enemies
        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                DarkSoulHealth darkSoulHealth = darkSoul.GetComponent<DarkSoulHealth>();
                if (darkSoulHealth != null && !darkSoulHealth.IsDead) // Only activate if not already dead
                {
                    darkSoul.gameObject.SetActive(true); 
                    darkSoul.enabled = true; // Enable DarkSoul AI script
                    darkSoul.SetPlayerTarget(playerTransform); // Set player target
                    Debug.Log($"EnemyAreaActivator: Activated Dark Soul: {darkSoul.name}");
                } else if (darkSoulHealth != null && darkSoulHealth.IsDead) {
                    Debug.Log($"EnemyAreaActivator: Dark Soul {darkSoul.name} is already dead, skipping activation.");
                } else {
                     Debug.LogWarning($"EnemyAreaActivator: DarkSoul '{darkSoul.name}' is missing DarkSoulHealth component. Cannot check IsDead state.", darkSoul);
                }
            }
             else
            {
                Debug.LogWarning($"EnemyAreaActivator: Null reference in 'darkSoulsInArea' list for {gameObject.name}.", this);
            }
        }

        // Activate GhostAI enemies
        foreach (GhostAI ghost in ghostsInArea)
        {
            if (ghost != null)
            {
                GhostHealth ghostHealth = ghost.GetComponent<GhostHealth>(); // Assuming GhostHealth script
                if (ghostHealth != null && !ghostHealth.IsDead) // Only activate if not already dead
                {
                    ghost.gameObject.SetActive(true);
                    ghost.enabled = true; // Enable GhostAI script
                    ghost.SetPlayerTarget(playerTransform); // Set player target
                    Debug.Log($"EnemyAreaActivator: Activated Ghost: {ghost.name}");
                } else if (ghostHealth != null && ghostHealth.IsDead) {
                    Debug.Log($"EnemyAreaActivator: Ghost {ghost.name} is already dead, skipping activation.");
                } else {
                     Debug.LogWarning($"EnemyAreaActivator: Ghost '{ghost.name}' is missing GhostHealth component. Cannot check IsDead state.", ghost);
                }
            }
            else
            {
                Debug.LogWarning($"EnemyAreaActivator: Null reference in 'ghostsInArea' list for {gameObject.name}.", this);
            }
        }
    }

    public void ResetActivator()
    {
        Debug.Log($"EnemyAreaActivator: Resetting activator for {gameObject.name}.");
        hasActivatedThisSession = false;

        // Reset DarkSoul enemies
        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                DarkSoulHealth darkSoulHealth = darkSoul.GetComponent<DarkSoulHealth>();
                if (darkSoulHealth != null)
                {
                    darkSoulHealth.ResetEnemyState(); // Resets health, re-enables collider, etc.
                }
                darkSoul.enabled = false; // Keep AI script disabled until activated again
                darkSoul.gameObject.SetActive(false); // Ensure the GameObject is off for re-activation
            }
        }

        // Reset GhostAI enemies
        foreach (GhostAI ghost in ghostsInArea)
        {
            if (ghost != null)
            {
                GhostHealth ghostHealth = ghost.GetComponent<GhostHealth>(); // Assuming GhostHealth script
                if (ghostHealth != null)
                {
                    ghostHealth.ResetEnemyState(); // Resets health, re-enables collider, etc.
                }
                ghost.enabled = false; // Keep AI script disabled until activated again
                ghost.gameObject.SetActive(false); // Ensure the GameObject is off for re-activation
            }
        }
    }

    void OnDrawGizmos()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f); 
            Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
            Gizmos.color = new Color(0, 1, 0, 0.8f); 
            Gizmos.DrawWireCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        }
    }
}
