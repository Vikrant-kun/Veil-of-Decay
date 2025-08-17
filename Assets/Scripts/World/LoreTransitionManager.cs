using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoreTransitionManager : MonoBehaviour
{
    public static LoreTransitionManager Instance { get; private set; }

    public CanvasGroup loreCanvasGroup;
    public TMP_Text loreText;
    public Image abilityUnlockedImage;
    public TMP_Text abilityDescriptionText;
    public Image additionalImage1;
    public Image additionalImage2;
    public Image hpIncreaseImage;
    public TMP_Text hpIncreaseText;

    public CanvasGroup locationNameCanvasGroup;
    public TMP_Text locationNameText;
    public float locationNameDisplayTime = 2.5f;

    public float fadeDuration = 0.5f;
    public string nextSceneName = "Level2";

    [TextArea(5, 10)]
    public string loreContent = "**As the Firstborn fell**, the void itself trembled. From his shadowed throne, the **Abyssal Lord** unleashed the **Great Corruption**, a tendril of ancient malice spreading across the lands.\n" +
                                "**Even as Demon Wood** found respite, a chilling power stirred. The **Secondborn**, the **Lich Lord of the Whispering Woods**, twisted the encroaching darkness to a new, necromantic will, binding the very essence of the **Dark Forest**.\n" +
                                "But from the sacred glades, the radiant **Guardian, Seraphina**, emerged. Sensing your triumph, and the looming shadow, she bestowed upon you a fragment of her divine power – a piercing grace forged in defiance.";

    [TextArea(3, 5)]
    public string abilityDescriptionContent = "**New Ability Unlocked!**\n" +
                                                "**Crimson Aegis Strike**\n" +
                                                "**Effect on enemies:** Spirit Burn\n" +
                                                "**What it does:** Each of your basic attacks now deals a small amount of bonus holy/spirit damage in addition to your normal physical damage. This bonus damage is particularly potent against corrupted foes.";

    public int oldMaxHP = 500;
    public int newMaxHP = 700;

    public Image healFlaskIncreaseImage;
    public TMP_Text healFlaskIncreaseText;

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
        Time.timeScale = 0f;
        
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

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        yield return WaitForAnyKeyInputThenClear(0.1f);
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }

        if (loreText != null) { loreText.gameObject.SetActive(false); }
        if (abilityUnlockedImage != null) { abilityUnlockedImage.gameObject.SetActive(true); }
        if (abilityDescriptionText != null)
        {
            abilityDescriptionText.text = abilityDescriptionContent;
            abilityDescriptionText.gameObject.SetActive(true);
            // Reverted: Unlocking Crimson Aegis Strike directly on PlayerMovement.Instance
            PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
            if (playerMovement != null) { playerMovement.hasCrimsonAegisStrike = true; Debug.Log("LoreTransitionManager: Crimson Aegis Strike unlocked on PlayerMovement script!"); }
            else { Debug.LogWarning("LoreTransitionManager: PlayerMovement script not found to unlock ability!"); }
            Debug.Log("LoreTransitionManager: After unlocking, PlayerMovement.hasCrimsonAegisStrike is: " + (playerMovement != null ? playerMovement.hasCrimsonAegisStrike.ToString() : "N/A")); 
        }
        if (additionalImage1 != null) { additionalImage1.gameObject.SetActive(true); }
        if (additionalImage2 != null) { additionalImage2.gameObject.SetActive(true); }

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        yield return WaitForAnyKeyInputThenClear(0.1f);
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }

        if (abilityUnlockedImage != null) { abilityUnlockedImage.gameObject.SetActive(false); }
        if (abilityDescriptionText != null) { abilityDescriptionText.gameObject.SetActive(false); }
        if (additionalImage1 != null) { additionalImage1.gameObject.SetActive(false); }
        if (additionalImage2 != null) { additionalImage2.gameObject.SetActive(false); }

        if (hpIncreaseImage != null) { hpIncreaseImage.gameObject.SetActive(true); }
        if (hpIncreaseText != null)
        {
            hpIncreaseText.text = "YOUR MAX HP HAS BEEN INCREASED!\n" + oldMaxHP + " --> " + newMaxHP;
            hpIncreaseText.gameObject.SetActive(true);
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null) { playerHealth.IncreaseMaxHealth(newMaxHP); }
            else { Debug.LogWarning("PlayerHealth script not found to update HP!"); }
        }

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        yield return WaitForAnyKeyInputThenClear(0.1f);
        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(false); }

        if (hpIncreaseImage != null) { hpIncreaseImage.gameObject.SetActive(false); }
        if (hpIncreaseText != null) { hpIncreaseText.gameObject.SetActive(false); }

        if (healFlaskIncreaseImage != null) { healFlaskIncreaseImage.gameObject.SetActive(true); }
        if (healFlaskIncreaseText != null)
        {
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                int initialFlasksBeforeAdding = playerHealth.currentHealthFlasks;
                int targetFlasks = 3;
                int flasksToAdd = targetFlasks - initialFlasksBeforeAdding;

                if (flasksToAdd < 0) { flasksToAdd = 0; }

                int newTotalFlasks = initialFlasksBeforeAdding + flasksToAdd;


                playerHealth.AddHealthFlask(flasksToAdd);
            }
            else { Debug.LogWarning("PlayerHealth script not found to add flasks!"); }
            healFlaskIncreaseText.gameObject.SetActive(true);
        }

        if (pressAnyKeyPromptText != null) { pressAnyKeyPromptText.gameObject.SetActive(true); }
        yield return WaitForAnyKeyInputThenClear(0.1f);

        if (loreCanvasGroup != null)
        {
            loreCanvasGroup.alpha = 1f;
            loreCanvasGroup.gameObject.SetActive(true);
        }
        if (healFlaskIncreaseImage != null) { healFlaskIncreaseImage.gameObject.SetActive(false); }
        if (healFlaskIncreaseText != null) { healFlaskIncreaseText.gameObject.SetActive(false); }

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        Time.timeScale = 1f;

        Debug.Log("LoreTransitionManager: STARTING LOCATION NAME DISPLAY SEQUENCE.");
        if (locationNameCanvasGroup != null)
        {
            locationNameCanvasGroup.gameObject.SetActive(true);
            locationNameCanvasGroup.alpha = 0f;
            Debug.Log("LoreTransitionManager: locationNameCanvasGroup activated. Alpha initially: " + locationNameCanvasGroup.alpha);
        }
        if (locationNameText != null)
        {
            locationNameText.text = "The Path Of Eternal Dusk";
            locationNameText.gameObject.SetActive(true);
            Debug.Log("LoreTransitionManager: locationNameText activated and text set.");
        }

        timer = 0f;
        while (timer < fadeDuration)
        {
            if (loreCanvasGroup != null)
                loreCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            if (locationNameCanvasGroup != null) {
                locationNameCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("LoreTransitionManager: Location name FADE-IN COMPLETE.");
        if (loreCanvasGroup != null)
        {
            loreCanvasGroup.alpha = 0f;
            loreCanvasGroup.gameObject.SetActive(false);
        }
        if (locationNameCanvasGroup != null)
        {
            locationNameCanvasGroup.alpha = 1f;
            Debug.Log("LoreTransitionManager: Location Name Canvas Group final alpha after fade-in: " + locationNameCanvasGroup.alpha);
        }

        Debug.Log("LoreTransitionManager: Holding location name for " + locationNameDisplayTime + " seconds.");
        yield return new WaitForSecondsRealtime(locationNameDisplayTime);

        Debug.Log("LoreTransitionManager: Starting location name FADE-OUT.");
        timer = 0f;
        while (timer < fadeDuration)
        {
            if (locationNameCanvasGroup != null)
                locationNameCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("LoreTransitionManager: Location name FADE-OUT COMPLETE. Deactivating UI.");
        if (locationNameCanvasGroup != null)
        {
            locationNameCanvasGroup.alpha = 0f;
            locationNameCanvasGroup.gameObject.SetActive(false);
        }
        if (locationNameText != null)
        {
            locationNameText.gameObject.SetActive(false);
        }

        if (PlayerHUDManager.Instance != null)
        {
            PlayerHUDManager.Instance.ShowHUD();
        }

        _transitionActive = false;
    }

    private IEnumerator WaitForAnyKeyInputThenClear(float debounceTime)
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        float timer = 0f;
        while (timer < debounceTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        while (Input.anyKey)
        {
            yield return null;
        }
    }
}
