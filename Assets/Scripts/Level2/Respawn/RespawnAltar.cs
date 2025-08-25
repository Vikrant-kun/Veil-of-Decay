using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RespawnAltar : MonoBehaviour
{
    [Header("Animator & Components")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Animation Parameters")]
    [Tooltip("The name of the Trigger parameter in the Animator for activation (e.g., 'ActivateAltar').")]
    public string activateAltarTriggerParam = "ActivateAltar";

    [Header("Interaction Settings")]
    [Tooltip("The tag of the player GameObject (e.g., 'Player').")]
    public string playerTag = "Player";
    [Tooltip("The key the player presses to activate the altar (e.g., KeyCode.R).")]
    public KeyCode activateKey = KeyCode.R;
    [Tooltip("Has this altar already been activated? (Managed by Lvl2_GMS)")]
    public bool isActivated = false; // This is now primarily managed by Lvl2_GMS

    [Header("UI Prompt")]
    [Tooltip("The CanvasGroup for the UI prompt (e.g., the panel containing 'R' in a box).")]
    public CanvasGroup promptCanvasGroup;
    [Tooltip("The TextMeshProUGUI component for the 'R' prompt. (Optional, can be used for the 🅁 symbol).")]
    public TMP_Text promptText;
    [Tooltip("The fade in/out speed for the UI prompt.")]
    public float uiFadeSpeed = 5f;

    private bool playerInRange = false;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (playerInRange && !isActivated)
        {
            if (promptCanvasGroup != null && promptCanvasGroup.alpha < 1f)
            {
                promptCanvasGroup.alpha += Time.deltaTime * uiFadeSpeed;
                if (promptCanvasGroup.alpha >= 1f)
                {
                    promptCanvasGroup.alpha = 1f;
                    promptCanvasGroup.interactable = true;
                    promptCanvasGroup.blocksRaycasts = true;
                }
            }

            if (Input.GetKeyDown(activateKey))
            {
                ActivateAltar();
            }
        }
        else
        {
            if (promptCanvasGroup != null && promptCanvasGroup.alpha > 0f)
            {
                promptCanvasGroup.alpha -= Time.deltaTime * uiFadeSpeed;
                if (promptCanvasGroup.alpha <= 0f)
                {
                    promptCanvasGroup.alpha = 0f;
                    promptCanvasGroup.interactable = false;
                    promptCanvasGroup.blocksRaycasts = false;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            Debug.Log($"RespawnAltar: Player entered range. Prompt will show for {gameObject.name}.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            Debug.Log($"RespawnAltar: Player exited range. Prompt will hide for {gameObject.name}.");
        }
    }

    void ActivateAltar()
    {
        if (isActivated) return; // Do nothing if already activated locally (prevents re-triggering animation)

        Debug.Log($"RespawnAltar: Activating altar at {transform.position}.");

        // Trigger the activation animation in the Animator
        if (animator != null)
        {
            animator.SetTrigger(activateAltarTriggerParam);
        }
        else
        {
            Debug.LogWarning($"RespawnAltar: Animator not assigned or found on {gameObject.name}. Cannot play activation animation.", this);
        }

        // Hide the prompt UI immediately upon activation
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }

        // Set this altar's position as the new respawn point via the Lvl2_GMS
        // --- IMPORTANT CHANGE: Pass 'this' (the current RespawnAltar instance) ---
        if (Lvl2_GMS.Instance != null)
        {
            Lvl2_GMS.Instance.SetRespawnPoint(transform.position, this); // Now passes the altar itself
            // isActivated = true; // No longer set directly here, Lvl2_GMS will manage this for consistency
        }
        else
        {
            Debug.LogError("RespawnAltar: Lvl2_GMS.Instance not found! Cannot set respawn point. Is the Lvl2_GMS in the scene and active?", this);
        }

        // Optional: Play a sound effect, particle effect etc.
    }

    /// <summary>
    /// Deactivates the altar's visuals and resets its state to "off".
    /// Called by Lvl2_GMS when another altar is activated.
    /// </summary>
    public void DeactivateVisuals() // --- NEW METHOD ---
    {
        isActivated = false; // Mark as inactive (important for UI prompt)
        if (animator != null)
        {
            animator.Play("Altar_Off"); // Force playing the static "off" state
            animator.Rebind(); // Ensure triggers are reset
            Debug.Log($"RespawnAltar: {gameObject.name} visuals deactivated and reverted to 'Off' state.");
        } else {
            Debug.LogWarning($"RespawnAltar: Animator is null for {gameObject.name}. Cannot deactivate visuals.", this);
        }
        // Ensure prompt is off
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Resets the altar to its inactive state. Useful for full level resets.
    /// This method is more for external game management/resetting the scene.
    /// </summary>
    public void ResetAltarState() // This is now more of a full system reset, DeactivateVisuals is for runtime changes.
    {
        isActivated = false;
        if (animator != null)
        {
            animator.Play("Altar_Off");
            animator.Rebind();
        }
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }
        playerInRange = false;
        Debug.Log($"RespawnAltar: {gameObject.name} state reset to inactive.");
    }

    void OnDrawGizmosSelected()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.4f);
            if (triggerCollider is BoxCollider2D box)
            {
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (triggerCollider is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
            else
            {
                Gizmos.DrawCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
            }
        }
    }
}