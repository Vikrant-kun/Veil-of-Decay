using UnityEngine;
using UnityEngine.UI; // For Image
using TMPro; // For TextMeshPro
using System.Collections; // For Coroutines
using UnityEngine.SceneManagement; // For loading scenes

public class NarrativeIntroManager : MonoBehaviour
{
    // Private member variables that were causing the errors
    private int currentPanelIndex = 0;
    private bool inputReady = false;

    [System.Serializable]
    public class StoryPanel
    {
        [Tooltip("The parent GameObject for all UI elements of this panel. Must have a CanvasGroup.")]
        public GameObject panelRootUI;
        [Tooltip("Reference to the Image component for the LEFT half of the canvas.")]
        public Image leftImageUI;
        [Tooltip("Reference to the Image component for the RIGHT half of the canvas.")]
        public Image rightImageUI;
        [Tooltip("Reference to the TextMeshPro component for the text overlay on this panel.")]
        public TMP_Text panelTextUI;
        
        [Tooltip("The Sprite for the LEFT image.")]
        public Sprite leftSprite;
        [Tooltip("The Sprite for the RIGHT image.")]
        public Sprite rightSprite;
        [TextArea(3, 10)]
        [Tooltip("The narrative text for this panel.")]
        public string panelText;

        [Tooltip("Duration for the entire panel (images and text) to fade in/out.")]
        public float panelFadeDuration = 1.5f;
        [Tooltip("Duration this panel stays fully visible before waiting for input.")]
        public float displayDuration = 5f; 
    }

    public StoryPanel[] storyPanels; // Array to hold all your story panels (excluding the final one)

    [Header("Final Panel (Single Image)")]
    [Tooltip("The parent GameObject for the final single-image panel. Must have a CanvasGroup.")]
    public GameObject finalPanelRootUI;
    [Tooltip("Reference to the Image component for the final single-image panel.")]
    public Image finalImageUI;
    [Tooltip("The Sprite for the final single image.")]
    public Sprite finalSprite;

    [Header("Continue Prompt UI")]
    public TMP_Text continuePromptTextUI; // Reference to the "Press Any Key" text

    private CanvasGroup continuePromptCanvasGroup;

    void Start()
    {
        // Get CanvasGroup for prompt
        continuePromptCanvasGroup = continuePromptTextUI.GetComponent<CanvasGroup>();
        
        // Ensure prompt starts invisible
        continuePromptCanvasGroup.alpha = 0f;
        continuePromptTextUI.gameObject.SetActive(false);

        // Ensure all panel UIs start inactive and transparent
        foreach (StoryPanel panel in storyPanels)
        {
            if (panel.panelRootUI != null)
            {
                panel.panelRootUI.SetActive(false);
                CanvasGroup rootCanvasGroup = panel.panelRootUI.GetComponent<CanvasGroup>();
                if (rootCanvasGroup != null) rootCanvasGroup.alpha = 0f;
            }
        }
        // Ensure final panel also starts inactive and transparent
        if (finalPanelRootUI != null)
        {
            finalPanelRootUI.SetActive(false);
            CanvasGroup finalRootCanvasGroup = finalPanelRootUI.GetComponent<CanvasGroup>();
            if (finalRootCanvasGroup != null) finalRootCanvasGroup.alpha = 0f;
        }

        // Start the narrative sequence
        StartCoroutine(PlayNarrativeSequence());
    }

    IEnumerator PlayNarrativeSequence()
    {
        foreach (StoryPanel panel in storyPanels)
        {
            // Basic validation for panel setup
            if (panel.panelRootUI == null || panel.leftImageUI == null || panel.rightImageUI == null || panel.panelTextUI == null || panel.leftSprite == null || panel.rightSprite == null)
            {
                Debug.LogError("NarrativeIntroManager: Panel setup error. Please check all assignments for panel " + currentPanelIndex);
                continue; // Skip to next panel if this one is misconfigured
            }

            // Activate the root UI for the current panel
            panel.panelRootUI.SetActive(true);

            // Assign sprites and text
            panel.leftImageUI.sprite = panel.leftSprite;
            panel.rightImageUI.sprite = panel.rightSprite;
            panel.panelTextUI.text = panel.panelText;
            
            // Ensure individual images/text are fully visible within their root panel's CanvasGroup
            if (panel.leftImageUI.GetComponent<CanvasGroup>() != null) panel.leftImageUI.GetComponent<CanvasGroup>().alpha = 1f;
            if (panel.rightImageUI.GetComponent<CanvasGroup>() != null) panel.rightImageUI.GetComponent<CanvasGroup>().alpha = 1f;
            if (panel.panelTextUI.GetComponent<CanvasGroup>() != null) panel.panelTextUI.GetComponent<CanvasGroup>().alpha = 1f;

            // Fade in the entire panel (all images and texts together)
            CanvasGroup rootCanvasGroup = panel.panelRootUI.GetComponent<CanvasGroup>();
            if (rootCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(rootCanvasGroup, 0f, 1f, panel.panelFadeDuration));
            }
            else
            {
                Debug.LogWarning("NarrativeIntroManager: Panel root UI missing CanvasGroup for panel " + currentPanelIndex + ". Skipping fade-in.");
            }

            // Wait for the panel's display duration
            yield return new WaitForSeconds(panel.displayDuration);

            // Show "Press Any Key to Continue..." prompt and wait for input
            inputReady = false;
            if (continuePromptTextUI != null)
            {
                continuePromptTextUI.gameObject.SetActive(true);
                continuePromptTextUI.text = "Press Any Key to Continue...";
                yield return StartCoroutine(FadeCanvasGroup(continuePromptCanvasGroup, 0f, 1f, 0.5f));
            }
            while (!inputReady)
            {
                if (Input.anyKeyDown)
                {
                    inputReady = true;
                }
                yield return null;
            }
            // Fade out the prompt
            yield return StartCoroutine(FadeCanvasGroup(continuePromptCanvasGroup, 1f, 0f, 0.5f));
            continuePromptTextUI.gameObject.SetActive(false);

            // Fade out the entire panel
            if (rootCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(rootCanvasGroup, 1f, 0f, panel.panelFadeDuration));
            }
            
            // Deactivate the root UI after fading out
            panel.panelRootUI.SetActive(false);

            currentPanelIndex++; // Increment here
        }

        // --- Handle the FINAL "Ready to Face..." Panel ---
        if (finalPanelRootUI == null || finalImageUI == null || finalSprite == null)
        {
            Debug.LogError("NarrativeIntroManager: Final panel setup error. Please check assignments.");
        }
        else
        {
            finalPanelRootUI.SetActive(true);
            finalImageUI.sprite = finalSprite;
            if (finalImageUI.GetComponent<CanvasGroup>() != null) finalImageUI.GetComponent<CanvasGroup>().alpha = 1f; // Ensure image is visible within its root

            CanvasGroup finalRootCanvasGroup = finalPanelRootUI.GetComponent<CanvasGroup>();
            if (finalRootCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(finalRootCanvasGroup, 0f, 1f, 1.5f)); // Fade in final image
            }

            // Show final "Press Any Key to Face..." prompt
            if (continuePromptTextUI != null)
            {
                continuePromptTextUI.gameObject.SetActive(true);
                continuePromptTextUI.text = "Press Any Key to Face...";
                yield return StartCoroutine(FadeCanvasGroup(continuePromptCanvasGroup, 0f, 1f, 0.5f));
            }
            
            inputReady = false; // Reset inputReady for the final prompt
            while (!inputReady)
            {
                if (Input.anyKeyDown)
                {
                    inputReady = true;
                }
                yield return null;
            }
        }

        // Load the first level of your game
        SceneManager.LoadScene("FirstLevel"); // Or "Level1" depending on your scene name
    }

    // Coroutine to fade a CanvasGroup's alpha
    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha; // Ensure final alpha is set
    }
}
