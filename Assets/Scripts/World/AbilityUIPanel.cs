using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUIPanel : MonoBehaviour
{
    // These should be assigned in the Inspector
    public GameObject chargeUIPanel;
    public Slider chargeUIFillBar;
    public GameObject abilityUnlockedUIPanel;
    public TMP_Text abilityUnlockedTitleText;
    public TMP_Text abilityUnlockedDescriptionText;
    public Image abilityUnlockedIcon;

    void Start()
    {
        // Find the player's attack script
        PlayerAttack playerAttack = Object.FindFirstObjectByType <PlayerAttack>();
        if (playerAttack != null)
        {
            // Register the UI elements with the PlayerAttack script
            playerAttack.RegisterUI(
                chargeUIPanel,
                chargeUIFillBar,
                abilityUnlockedUIPanel,
                abilityUnlockedTitleText,
                abilityUnlockedDescriptionText,
                abilityUnlockedIcon
            );
        }
    }
}
