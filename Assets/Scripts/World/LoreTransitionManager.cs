// LoreTransitionManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoreTransitionManager : MonoBehaviour
{
    public static LoreTransitionManager Instance { get; private set; }

    [Header("UI References")]
    public CanvasGroup loreCanvasGroup; // This will now act as the main screen fader
    public TMP_Text loreText;
    public Image abilityUnlockedImage;
    public TMP_Text abilityDescriptionText;
    public Image additionalImage1;
    public Image additionalImage2;
    public Image hpIncreaseImage;
    public TMP_Text hpIncreaseText;

    [Header("Location Transition UI")]
    public CanvasGroup locationNameCanvasGroup;
    public TMP_Text locationNameText;
    public float locationNameDisplayTime = 2f;

    [Header("Transition Settings")]
    public float fadeDuration = 0.5f;
    public string nextSceneName = "Scenes/Level2";

    [Header("Lore Content")]
    [TextArea(5, 10)]
    public string loreContent = "**As the Firstborn fell**, the void itself trembled. From his shadowed throne, the **Abyssal Lord** unleashed the **Great Corruption**, a tendril of ancient malice spreading across the lands.\n" +
                                "Even as **Demon Wood** found respite, a chilling power stirred. The **Secondborn**, the **Lich Lord of the Whispering Woods**, twisted the encroaching darkness to a new, necromantic will, binding the very essence of the **Dark Forest**.\n" +
                                "But from the sacred glades, the radiant **Guardian, Seraphina**, emerged. Sensing your triumph, and the looming shadow, she bestowed upon you a fragment of her divine power – a piercing grace forged in defiance.";

    [Header("Ability Content")]
    [TextArea(3, 5)]
    public string abilityDescriptionContent = "**New Ability Unlocked!**\n" +
                                                "**Crimson Aegis Strike**\n" +
                                                "**Effect on enemies:** Spirit Burn\n" +
                                                "**What it does:** Each of your basic attacks now deals a small amount of bonus holy/spirit damage in addition to your normal physical damage. This bonus damage is particularly potent against corrupted foes.";

    [Header("HP Increase Settings")]
    public int oldMaxHP = 500;
    public int newMaxHP = 700; // This is the correct variable name!

    [Header("Heal Flask Increase UI")]
    public Image healFlaskIncreaseImage;
    public TMP_Text healFlaskIncreaseText;


    [Header("UI Prompt")]
    public TMP_Text pressAnyKeyPromptText;

    private bool _transitionActive = false;
    private int _pressCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initial setup to ensure everything starts hidden
        if (loreCanvasGroup != null)
        {
            loreCanvasGroup.alpha = 0f;
            loreCanvasGroup.gameObject.SetActive(false);
        }
        if (loreText != null) { loreText.gameObject.SetActive(false); }
        if (abilityUnlockedImage != null) { abilityUnlockedImage.gameObject.SetActive(false); }
        if (abilityDescriptionText != null) { abilityDescriptionText.gameObject.SetActive(false); }
        if (additionalImage1 != null) { additionalImage1.gameObject.SetActive(false); }
        if (additionalImage2 != null) { additionalImage2.gameObject.SetActive(false); }
        if (hpIncreaseImage != null) { hpIncreaseImage.gameObject.SetActive(false); }
        if (hpIncreaseText != null) { hpIncreaseText.gameObject.SetActive(false); }

        // Initialize new UI elements
        if (healFlaskIncreaseImage != null) { healFlaskIncreaseImage.gameObject.SetActive(false); }
        if (healFlaskIncreaseText != null) { healFlaskIncreaseText.gameObject.SetActive(false); }

        if (locationNameCanvasGroup != null)
        {
            locationNameCanvasGroup.alpha = 0f;
            locationNameCanvasGroup.gameObject.SetActive(false);
        }
        if (locationNameText != null) { locationNameText.gameObject.SetActive(false); }

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }
    }

    public void StartLoreTransition()
    {
        if (!_transitionActive)
        {
            _transitionActive = true;
            _pressCount = 0;

            if (PlayerHUDManager.Instance != null)
            {
                PlayerHUDManager.Instance.HideHUD();
            }

            StartCoroutine(TransitionSequence());
        }
    }

    private IEnumerator TransitionSequence()
    {
        Time.timeScale = 0f; // Pause game time during initial transition

        // Ensure lore canvas is active and fades in
        if (loreCanvasGroup != null)
        {
            loreCanvasGroup.gameObject.SetActive(true);
            loreCanvasGroup.alpha = 0f;
        }
        if (loreText != null)
        {
            loreText.text = loreContent;
            loreText.gameObject.SetActive(true);
        }

        // Fade in main lore screen
        float timer = 0f;
        while (timer < fadeDuration)
        {
            if (loreCanvasGroup != null)
                loreCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        if (loreCanvasGroup != null)
            loreCanvasGroup.alpha = 1f;

        // --- Lore Display ---
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        while (!Input.anyKeyDown) { yield return null; }
        _pressCount++; yield return new WaitForEndOfFrame();
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }

        // --- Ability Unlocked Display ---
        if (loreText != null) { loreText.gameObject.SetActive(false); }
        if (abilityUnlockedImage != null) { abilityUnlockedImage.gameObject.SetActive(true); }
        if (abilityDescriptionText != null)
        {
            abilityDescriptionText.text = abilityDescriptionContent;
            abilityDescriptionText.gameObject.SetActive(true);
            PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
            if (playerMovement != null) { playerMovement.hasCrimsonAegisStrike = true; Debug.Log("Crimson Aegis Strike unlocked on PlayerMovement script!"); }
            else { Debug.LogWarning("PlayerMovement script not found to unlock ability!"); }
        }
        if (additionalImage1 != null) { additionalImage1.gameObject.SetActive(true); }
        if (additionalImage2 != null) { additionalImage2.gameObject.SetActive(true); }

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        while (!Input.anyKeyDown) { yield return null; }
        _pressCount++; yield return new WaitForEndOfFrame();
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }

        // --- HP Increase Display ---
        if (abilityUnlockedImage != null) { abilityUnlockedImage.gameObject.SetActive(false); }
        if (abilityDescriptionText != null) { abilityDescriptionText.gameObject.SetActive(false); }
        if (additionalImage1 != null) { additionalImage1.gameObject.SetActive(false); }
        if (additionalImage2 != null) { additionalImage2.gameObject.SetActive(false); }

        if (hpIncreaseImage != null) { hpIncreaseImage.gameObject.SetActive(true); }
        if (hpIncreaseText != null)
        {
            // FIX: Changed 'newMax' to 'newMaxHP' here
            hpIncreaseText.text = "YOUR MAX HP HAS BEEN INCREASED!\n" + oldMaxHP + " --> " + newMaxHP;
            hpIncreaseText.gameObject.SetActive(true);
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null) { playerHealth.IncreaseMaxHealth(newMaxHP); }
            else { Debug.LogWarning("PlayerHealth script not found to update HP!"); }
        }

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        while (!Input.anyKeyDown) { yield return null; }
        _pressCount++; yield return new WaitForEndOfFrame(); // Keep for user input pause
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }

        // Hide HP increase UI elements to prepare for next info
        if (hpIncreaseImage != null) { hpIncreaseImage.gameObject.SetActive(false); }
        if (hpIncreaseText != null) { hpIncreaseText.gameObject.SetActive(false); }


        // --- Heal Flask Increase Display ---
        if (healFlaskIncreaseImage != null) { healFlaskIncreaseImage.gameObject.SetActive(true); }
        if (healFlaskIncreaseText != null)
        {
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            int currentFlasks = (playerHealth != null) ? playerHealth.currentHealthFlasks : 0;
            int flasksAdded = 1;
            int newTotalFlasks = currentFlasks + flasksAdded;

            healFlaskIncreaseText.text = $"HEAL FLASK GAINED!\nTotal Flasks: {currentFlasks} --> {newTotalFlasks}";

            if (playerHealth != null)
            {
                playerHealth.AddHealthFlask(flasksAdded);
            }
            healFlaskIncreaseText.gameObject.SetActive(true);
        }

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        while (!Input.anyKeyDown) { yield return null; }
        _pressCount++;

        // --- CRITICAL CHANGE FOR SCREEN BLACKOUT BEFORE SCENE LOAD ---
        if (loreCanvasGroup != null)
        {
            loreCanvasGroup.alpha = 1f;
            loreCanvasGroup.gameObject.SetActive(true);
        }
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }

        // Hide heal flask increase UI elements (now covered by black screen)
        if (healFlaskIncreaseImage != null) { healFlaskIncreaseImage.gameObject.SetActive(false); }
        if (healFlaskIncreaseText != null) { healFlaskIncreaseText.gameObject.SetActive(false); }

        // Time.timeScale remains 0f here. It will be set to 1f after scene activation.

        // Start loading the next scene asynchronously in the background.
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;

        // Wait until the new scene is almost loaded (e.g., 90%)
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // The new scene is now loaded but not yet active/visible.
        // The LoreTransitionManager and Player have persisted into this loaded scene.

        // Prepare the location name UI elements.
        if (locationNameCanvasGroup != null)
        {
            locationNameCanvasGroup.gameObject.SetActive(true);
            locationNameCanvasGroup.alpha = 0f;
        }
        if (locationNameText != null)
        {
            locationNameText.text = "The Path Of Eternal Dusk";
            locationNameText.gameObject.SetActive(true);
        }

        // Simultaneously fade out the covering black screen (loreCanvasGroup)
        // AND fade in the location name text (locationNameCanvasGroup).
        float currentTimer = 0f;
        while (currentTimer < fadeDuration)
        {
            if (loreCanvasGroup != null)
                loreCanvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTimer / fadeDuration);

            if (locationNameCanvasGroup != null)
                locationNameCanvasGroup.alpha = Mathf.Lerp(0f, 1f, currentTimer / fadeDuration);

            currentTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure final alpha states after the combined fade
        if (loreCanvasGroup != null)
        {
            loreCanvasGroup.alpha = 0f;
            loreCanvasGroup.gameObject.SetActive(false);
        }
        if (locationNameCanvasGroup != null)
        {
            locationNameCanvasGroup.alpha = 1f;
        }

        // Now, allow the asynchronously loaded scene to activate and become visible.
        operation.allowSceneActivation = true;

        // Wait until the scene is fully activated and rendered.
        while (!operation.isDone)
        {
            yield return null;
        }

        Time.timeScale = 1f; // Restore game time for the now active scene.

        // Hold the location name on screen for its display duration
        yield return new WaitForSecondsRealtime(locationNameDisplayTime);

        // Fade out the location name text
        currentTimer = 0f;
        while (currentTimer < fadeDuration)
        {
            if (locationNameCanvasGroup != null)
                locationNameCanvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTimer / fadeDuration);

            currentTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure final states for location name UI
        if (locationNameCanvasGroup != null)
        {
            locationNameCanvasGroup.alpha = 0f;
            locationNameCanvasGroup.gameObject.SetActive(false);
        }
        if (locationNameText != null)
        {
            locationNameText.gameObject.SetActive(false);
        }

        // Ensure HUD is shown after all transitions are complete.
        if (PlayerHUDManager.Instance != null)
        {
            PlayerHUDManager.Instance.ShowHUD();
        }

        _transitionActive = false; // Reset transition state, ready for next transition
    }
}