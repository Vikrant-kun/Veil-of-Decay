// This is the correct PlayerMagic.cs you should have
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PlayerMagic : MonoBehaviour
{
    public int maxMana = 100;
    private int currentMana;

    public List<string> unlockedSpells = new List<string>();

    [Header("Mana UI")]
    public Slider manaBarUI;

    [Header("Spell Unlock UI")]
    public GameObject spellUnlockPanel;
    public TMP_Text spellUnlockMessageText;

    public float messageDisplayDuration = 3f;

    void Start()
    {
        currentMana = 0;
        
        if (manaBarUI != null)
        {
            manaBarUI.maxValue = maxMana;
            manaBarUI.value = currentMana;
        }

        UpdateManaUI();

        if (spellUnlockPanel != null)
        {
            spellUnlockPanel.SetActive(false);
        }
    }

    void UpdateManaUI()
    {
        if (manaBarUI != null)
        {
            manaBarUI.value = currentMana;
        }
    }

    public void UnlockMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        UpdateManaUI();
    }

    public void UnlockSpell(string spellName)
    {
        if (!unlockedSpells.Contains(spellName))
        {
            unlockedSpells.Add(spellName);
            DisplayMessage($"New Spell Unlocked: {spellName}!");
        }
    }

    public void DisplayMessage(string message) // THIS IS THE IMPORTANT METHOD
    {
        if (spellUnlockPanel != null && spellUnlockMessageText != null)
        {
            StopAllCoroutines();
            spellUnlockMessageText.text = message;
            spellUnlockPanel.SetActive(true);
            StartCoroutine(HideMessageAfterDelay(messageDisplayDuration));
        }
    }

    System.Collections.IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (spellUnlockPanel != null)
        {
            spellUnlockPanel.SetActive(false);
        }
    }

    public bool CanCastSpell(int manaCost)
    {
        return currentMana >= manaCost;
    }

    public void CastSpell(int manaCost)
    {
        if (CanCastSpell(manaCost))
        {
            currentMana -= manaCost;
            UpdateManaUI();
        }
    }
}