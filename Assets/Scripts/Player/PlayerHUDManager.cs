using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    // Singleton Instance
    public static PlayerHUDManager Instance { get; private set; }

    // Reference to the CanvasGroup component on this Canvas (if you added one)
    // Or you can directly reference this GameObject if no CanvasGroup is needed for fading
    public CanvasGroup canvasGroup; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make this HUD Canvas persist across scenes
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>(); // Try to get CanvasGroup if not assigned
            }
        }
        else
        {
            // If another instance of the HUD exists, destroy this one
            Destroy(gameObject);
        }
    }

    // Method to hide the HUD
    public void HideHUD()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Method to show the HUD
    public void ShowHUD()
    {
        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}