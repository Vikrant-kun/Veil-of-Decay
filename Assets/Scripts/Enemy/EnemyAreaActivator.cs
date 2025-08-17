using UnityEngine;
using System.Collections.Generic;

public class EnemyAreaActivator : MonoBehaviour
{
    [Header("Enemies to Activate")]
    [Tooltip("Drag all DarkSoul enemies that belong to this area here.")]
    public List<DarkSoul> darkSoulsInArea = new List<DarkSoul>();
    // Removed: nightbornesInArea List

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
        }
        if (!zoneCollider.isTrigger)
        {
            Debug.LogWarning("EnemyAreaActivator: Collider on " + gameObject.name + " is not set to Is Trigger. It should be a trigger!", this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && (!hasActivatedThisSession || !activateOnce))
        {
            Debug.Log($"EnemyAreaActivator: Player entered {gameObject.name}. Activating enemies.");
            ActivateEnemies();
            if (activateOnce)
            {
                hasActivatedThisSession = true; 
            }
        }
    }

    void ActivateEnemies()
    {
        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                // Accessing DarkSoulHealth to check IsDead property
                DarkSoulHealth darkSoulHealth = darkSoul.GetComponent<DarkSoulHealth>();
                if (darkSoulHealth != null && !darkSoulHealth.IsDead) // Only activate if not already dead
                {
                    darkSoul.gameObject.SetActive(true); 
                    darkSoul.enabled = true; // Enable DarkSoul AI script
                    if (GameRestartManager.Instance != null && GameRestartManager.Instance.currentPlayerTransform != null)
                    {
                        darkSoul.SetPlayerTarget(GameRestartManager.Instance.currentPlayerTransform);
                    }
                }
            }
        }

        // Removed: foreach loop for nightbornesInArea
    }

    public void ResetActivator()
    {
        Debug.Log($"EnemyAreaActivator: Resetting activator for {gameObject.name}.");
        hasActivatedThisSession = false;

        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                // Accessing DarkSoulHealth to reset enemy state
                DarkSoulHealth darkSoulHealth = darkSoul.GetComponent<DarkSoulHealth>();
                if (darkSoulHealth != null)
                {
                    darkSoulHealth.ResetEnemyState(); // Resets health, re-enables collider, etc.
                }
                darkSoul.enabled = false; // Keep AI script disabled until activated again
            }
        }

        // Removed: foreach loop for nightbornesInArea reset
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
