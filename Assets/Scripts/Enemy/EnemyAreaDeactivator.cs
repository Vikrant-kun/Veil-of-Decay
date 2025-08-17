using UnityEngine;
using System.Collections.Generic;

public class EnemyAreaDeactivator : MonoBehaviour
{
    [Header("Enemies to Deactivate")]
    [Tooltip("Drag all DarkSoul enemies that belong to this area here.")]
    public List<DarkSoul> darkSoulsInArea = new List<DarkSoul>();
    // Removed: nightbornesInArea List

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
        }
        if (!zoneCollider.isTrigger)
        {
            Debug.LogWarning("EnemyAreaDeactivator: Collider on " + gameObject.name + " is not set to Is Trigger. It should be a trigger!", this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && (!hasDeactivatedThisSession || !deactivateOnce))
        {
            Debug.Log($"EnemyAreaDeactivator: Player entered {gameObject.name}. Deactivating enemies.");
            DeactivateEnemies();
            if (deactivateOnce)
            {
                hasDeactivatedThisSession = true;
            }
        }
    }

    void DeactivateEnemies()
    {
        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                // Accessing DarkSoulHealth to check IsDead property
                DarkSoulHealth darkSoulHealth = darkSoul.GetComponent<DarkSoulHealth>();
                if (darkSoulHealth != null && !darkSoulHealth.IsDead)
                {
                    darkSoul.gameObject.SetActive(false); 
                }
            }
        }

        // Removed: foreach loop for nightbornesInArea
    }

    public void ResetDeactivator()
    {
        Debug.Log($"EnemyAreaDeactivator: Resetting deactivator for {gameObject.name}.");
        hasDeactivatedThisSession = false;

        foreach (DarkSoul darkSoul in darkSoulsInArea)
        {
            if (darkSoul != null)
            {
                // Ensure the game object is active when resetting the deactivator
                darkSoul.gameObject.SetActive(true); 
            }
        }
        // Removed: foreach loop for nightbornesInArea activation
    }

    void OnDrawGizmos()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        if (zoneCollider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f); 
            Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
            Gizmos.color = new Color(1, 0, 0, 0.8f); 
            Gizmos.DrawWireCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        }
    }
}
