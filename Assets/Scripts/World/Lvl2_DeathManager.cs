using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Required for Coroutines

public class Lvl2_DeathManager : MonoBehaviour
{
    // Make this a Singleton so PlayerHealth can easily access it
    public static Lvl2_DeathManager Instance { get; private set; }

    [Header("UI Elements")]
    public CanvasGroup deathCanvas;
    public TMP_Text DeathMessage;
    public TMP_Text RespawnPrompt; // Or perhaps a "Restart Level 2" / "Back to Main Menu" prompt

    [Header("Settings")]
    public float fadeDuration = 1.5f;
    public string mainMenuSceneName = "MainMenu"; // Or "GameOver" scene

    private bool isDeathScreenActive = false;

    void Awake()
    {
        // Singleton pattern: Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Optionally, if this manager should persist across scenes (unlikely for a specific level's death manager), uncomment below:
            // DontDestroyOnLoad(gameObject); 
        }
    }

    void Start()
    {
        // Initialize UI state
        if (deathCanvas != null) deathCanvas.alpha = 0;
        if (deathCanvas != null) deathCanvas.gameObject.SetActive(false);
        if (RespawnPrompt != null) RespawnPrompt.gameObject.SetActive(false);
        if (DeathMessage != null) DeathMessage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Displays the death screen for Level 2.
    /// </summary>
    public void ShowDeathScreen()
    {
        if (isDeathScreenActive) return;
        isDeathScreenActive = true;
        Debug.Log("Lvl2_DeathManager: Showing death screen.");

        // Optionally, disable player input here if not handled by PlayerHealth
        // Find the player and disable components if needed, similar to DeathHandler

        StartCoroutine(FadeInDeathScreen());
    }

    private IEnumerator FadeInDeathScreen()
    {
        if (deathCanvas != null) deathCanvas.gameObject.SetActive(true);

        float t = 0;
        while (t < fadeDuration)
        {
            if (deathCanvas != null) deathCanvas.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            t += Time.unscaledDeltaTime; // Use unscaledDeltaTime for UI fades during game pause
            yield return null;
        }
        if (deathCanvas != null) deathCanvas.alpha = 1;

        Time.timeScale = 0f; // Pause the game

        if (DeathMessage != null) DeathMessage.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f); // Wait for a second in real time
        if (RespawnPrompt != null) RespawnPrompt.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isDeathScreenActive && Input.GetKeyDown(KeyCode.R)) // Example: Press R to restart or go to main menu
        {
            Debug.Log("Lvl2_DeathManager: R key pressed. Restarting or going to main menu.");
            RestartLevel2(); // Or LoadMainMenu()
        }
    }

    /// <summary>
    /// Handles restarting Level 2 or transitioning to a game over state.
    /// You might want to reload the scene or load a specific game over scene.
    /// </summary>
    public void RestartLevel2()
    {
        isDeathScreenActive = false;
        Time.timeScale = 1f; // Unpause the game

        // Hide UI immediately
        if (DeathMessage != null) DeathMessage.gameObject.SetActive(false);
        if (RespawnPrompt != null) RespawnPrompt.gameObject.SetActive(false);
        if (deathCanvas != null) deathCanvas.alpha = 0;
        if (deathCanvas != null) deathCanvas.gameObject.SetActive(false);

        // Load the Level 2 scene again, or the Main Menu, or a specific Game Over scene.
        // For Level 2, you might NOT want to reload the whole scene if the player is persistent.
        // Instead, you'd reset the player's position and health within the *current* scene,
        // similar to how GameRestartManager handles respawn within a scene.
        // For simplicity, here's a scene reload example:
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene (Level 2)
        // Or to go to main menu: SceneManager.LoadScene(mainMenuSceneName);
    }
}
