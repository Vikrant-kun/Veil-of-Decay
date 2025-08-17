using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "FirstLevel"; 

    public void StartGame()
    {
        Debug.Log("MainMenuManager: 'Start Game' button clicked.");
        
        if (PlayerMovement.Instance != null)
        {
            Debug.Log("MainMenuManager: PlayerMovement.Instance found. Calling ResetAbilities.");
            PlayerMovement.Instance.ResetAbilities(); 
            // Reverted: Checking PlayerMovement.Instance for hasCrimsonAegisStrike
            Debug.Log("MainMenuManager: ResetAbilities called. PlayerMovement.Instance.hasCrimsonAegisStrike after call: " + PlayerMovement.Instance.hasCrimsonAegisStrike); 
        }
        else
        {
            Debug.LogWarning("MainMenuManager: PlayerMovement.Instance not found when starting game. This might happen if PlayerMovement is not yet initialized or is not persistent.");
        }

        SceneManager.LoadScene(gameSceneName);
        Debug.Log("MainMenuManager: Loading scene: " + gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("MainMenuManager: Quitting Game...");
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
