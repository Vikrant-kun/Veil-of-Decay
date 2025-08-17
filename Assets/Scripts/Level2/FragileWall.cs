using UnityEngine;
using System.Collections;

public class FragileWall : MonoBehaviour
{
    private Animator animator;
    private Collider2D wallCollider;
    
    // The duration of your break animation clip
    public float breakAnimationDuration = 1.0f; // Set this to the actual length of your animation

    void Awake()
    {
        animator = GetComponent<Animator>();
        wallCollider = GetComponent<Collider2D>();

        if (animator == null)
        {
            Debug.LogError("FragileWall: Missing Animator component on " + gameObject.name, this);
        }
        if (wallCollider == null)
        {
            Debug.LogError("FragileWall: Missing Collider2D component on " + gameObject.name, this);
        }
    }

    public void BreakWall()
    {
        // Immediately disable the collider so the player can pass through
        if (wallCollider != null)
        {
            wallCollider.enabled = false;
        }

        // Trigger the break animation
        if (animator != null)
        {
            animator.SetTrigger("Break");
        }

        // Destroy the GameObject after the animation has finished playing
        Destroy(gameObject, breakAnimationDuration);
    }
}