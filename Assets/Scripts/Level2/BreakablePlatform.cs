using UnityEngine;
using System.Collections;

public class BreakablePlatform : MonoBehaviour
{
    public float breakDelay = 0.5f;
    public float reappearTime = 2f;
    
    private bool hasBeenActivated = false;
    private bool isBroken = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Find both colliders on this single object
        colliders = GetComponents<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenActivated)
        {
            hasBeenActivated = true;
            StartCoroutine(BreakPlatformAfterDelay());
        }
    }
    
    IEnumerator BreakPlatformAfterDelay()
    {
        yield return new WaitForSeconds(breakDelay);
        
        if (!isBroken)
        {
            BreakThePlatform();
        }
    }

    private void BreakThePlatform()
    {
        isBroken = true;
        
        spriteRenderer.enabled = false;
        // Disable both colliders
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        StartCoroutine(ReappearPlatformAfterDelay());
    }
    
    IEnumerator ReappearPlatformAfterDelay()
    {
        yield return new WaitForSeconds(reappearTime);

        isBroken = false;
        hasBeenActivated = false;
        
        spriteRenderer.enabled = true;
        // Enable both colliders
        foreach (Collider2D col in colliders)
        {
            col.enabled = true;
        }
    }
}