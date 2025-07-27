using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "FirstLevel"; // Make sure this is "Level1" if that's your starting game level

    public void StartGame()
    {
        Debug.Log("MainMenuManager: 'Start Game' button clicked."); // ADDED DEBUG
        
        // --- CRUCIAL ADDITION HERE ---
        // Find the PlayerMovement instance and reset its abilities BEFORE loading the scene.
        // Since PlayerMovement uses DontDestroyOnLoad, it persists, so we need to reset its state.
        if (PlayerMovement.Instance != null)
        {
            Debug.Log("MainMenuManager: PlayerMovement.Instance found. Calling ResetAbilities."); // ADDED DEBUG
            PlayerMovement.Instance.ResetAbilities(); 
            Debug.Log("MainMenuManager: ResetAbilities called. PlayerMovement.Instance.hasCrimsonAegisStrike after call: " + PlayerMovement.Instance.hasCrimsonAegisStrike); // ADDED DEBUG
        }
        else
        {
            Debug.LogWarning("MainMenuManager: PlayerMovement.Instance not found when starting game. This might happen if PlayerMovement is not yet initialized or is not persistent."); // ADDED DEBUG
        }
        // --- END CRUCIAL ADDITION ---

        SceneManager.LoadScene(gameSceneName);
        Debug.Log("MainMenuManager: Loading scene: " + gameSceneName); // ADDED DEBUG
    }

    public void QuitGame()
    {
        Debug.Log("MainMenuManager: Quitting Game..."); // ADDED DEBUG
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OpenSettingsPanel(GameObject settingsPanel)
    {
        settingsPanel.SetActive(true);
    }

    public void ClosePanel(GameObject panelToClose)
    {
        panelToClose.SetActive(false);
    }
}