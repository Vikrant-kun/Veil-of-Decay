using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurseEffectManager : MonoBehaviour
{
    public static CurseEffectManager Instance { get; private set; }

    public Image crackLevel1Image;
    public Image crackLevel2Image;
    public Image crackLevel3Image;
    public Image fullScreenGlitcHImage;

    [Range(0f, 1f)] public float level1Alpha = 0.2f;
    [Range(0f, 1f)] public float level2Alpha = 0.5f;
    [Range(0f, 1f)] public float level3Alpha = 0.8f;
    [Range(0f, 1f)] public float fullScreenGlitcHAlpha = 0.3f;

    // fadeDuration is no longer used for crack appearance, but kept for potential future use
    public float fadeDuration = 1f; 

    private int currentDeathCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        currentDeathCount = PlayerPrefs.GetInt("DeathCount", 0); 
        ApplyCurseVisuals(currentDeathCount); // No duration needed
        Debug.Log("CurseEffectManager: Initialized. Current death count loaded: " + currentDeathCount);
    }

    public void UpdateCurseLevel(int newDeathCount)
    {
        currentDeathCount = newDeathCount;
        PlayerPrefs.SetInt("DeathCount", currentDeathCount);
        PlayerPrefs.Save();

        ApplyCurseVisuals(currentDeathCount); // No duration needed
        Debug.Log("CurseEffectManager: Curse level updated to: " + currentDeathCount);
    }

    private void ApplyCurseVisuals(int level) // Removed duration parameter
    {
        StopAllCoroutines(); // Stop any leftover fades from previous logic (good practice)

        // Instantly set the alpha for each crack level
        SetImageAlpha(crackLevel1Image, level >= 1 ? level1Alpha : 0f);
        SetImageAlpha(crackLevel2Image, level >= 2 ? level2Alpha : 0f);
        SetImageAlpha(crackLevel3Image, level >= 3 ? level3Alpha : 0f);
        SetImageAlpha(fullScreenGlitcHImage, level >= 4 ? fullScreenGlitcHAlpha : 0f);
    }

    // New helper method to instantly set an image's alpha
    private void SetImageAlpha(Image img, float targetAlpha)
    {
        if (img == null) return;
        Color currentColor = img.color;
        img.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }

    public void ResetCurse()
    {
        UpdateCurseLevel(0);
        Debug.Log("CurseEffectManager: Curse reset to level 0.");
    }
}