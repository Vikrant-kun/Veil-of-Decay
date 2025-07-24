using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class DeathHandler : MonoBehaviour
{
    public CanvasGroup deathCanvas;
    public TMP_Text DeathMessage;
    public TMP_Text RespawnPrompt;

    public float fadeDuration = 1.5f;

    public string sceneToRestart = "FirstLevel";

    private Transform player;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private Rigidbody2D playerRigidbody;

    private bool isWaitingForRespawn = false;

    void Start()
    {
        deathCanvas.alpha = 0;
        deathCanvas.gameObject.SetActive(false);
        RespawnPrompt.gameObject.SetActive(false);
        DeathMessage.gameObject.SetActive(false);
    }

    public void TriggerDeathScene()
    {
        if (isWaitingForRespawn) return;
        Debug.Log("DeathHandler: TriggerDeathScene called. Starting HandleDeath coroutine.");

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = player.GetComponent<PlayerHealth>(); // This is where your error was
            playerMovement = player.GetComponent<PlayerMovement>();
            playerAttack = player.GetComponent<PlayerAttack>();
            playerRigidbody = player.GetComponent<Rigidbody2D>();

            if (playerHealth == null) Debug.LogError("PlayerHealth not found on player object during death trigger!");
        }
        else
        {
            Debug.LogError("DeathHandler: Player GameObject with tag 'Player' NOT found when death was triggered! Cannot handle death.");
            return;
        }

        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        isWaitingForRespawn = true;

        if (player != null)
        {
            player.gameObject.SetActive(false);
            if (playerMovement != null) playerMovement.enabled = false;
            if (playerAttack != null) playerAttack.enabled = false;
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.bodyType = RigidbodyType2D.Kinematic; 
                playerRigidbody.simulated = false;
            }
        }

        deathCanvas.gameObject.SetActive(true);

        // REMOVED: CurseEffectManager.Instance.UpdateCurseLevel(currentDeathCount);
        // No curse in Level 1 now.

        // Now, fade in the death canvas
        float t = 0;
        while (t < fadeDuration)
        {
            deathCanvas.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        deathCanvas.alpha = 1;

        // After death screen fades in, pause the game.
        Time.timeScale = 0f; 

        DeathMessage.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1f); 
        RespawnPrompt.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isWaitingForRespawn && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("DeathHandler: R key pressed. Destroying player and loading scene immediately.");
            StartCoroutine(RestartGame());
        }
    }

    private IEnumerator RestartGame()
    {
        isWaitingForRespawn = false;
        Time.timeScale = 1f;

        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }

        DeathMessage.gameObject.SetActive(false);
        RespawnPrompt.gameObject.SetActive(false);
        deathCanvas.alpha = 0;
        deathCanvas.gameObject.SetActive(false);

        // Keep this flag for the GameRestartManager to know it was a death-restart
        PlayerPrefs.SetInt("GameRestartedFromDeath", 1); 
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneToRestart);
        Debug.Log("DeathHandler: Loading scene: " + sceneToRestart);

        yield break;
    }
}