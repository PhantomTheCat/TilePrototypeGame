using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatScreenBehavior : MonoBehaviour
{
    //Properties
    [Header("Character Profile")]
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI baseStats;
    [SerializeField] private Button levelUpButton;

    [Header("Stat Bar")]
    [SerializeField] private TextMeshProUGUI statText;


    //Methods
    private void Awake()
    {
        if (portrait ==  null || baseStats == null 
            || nameText == null || statText == null)
        {
            Debug.LogError("Not everything is set up for the stat screen.");
        }

        if (levelUpButton != null)
        {
            //Is disabled until player can level up
            levelUpButton.interactable = false;
        }
    }

    public void UpdateUserProfile()
    {
        BaseHero hero = UnitManager.Instance.SelectedHero;
        if (hero == null) return;

        portrait.sprite = hero.UnitPortrait;
        nameText.text = hero.UnitName;
        baseStats.text = $"{hero.Level}\nN/A";

        string stats = "";
        stats += $"{hero.Strength}\n";
        stats += $"{hero.Dexterity}\n";
        stats += $"{hero.Constitution}\n";
        stats += $"{hero.Faith}\n";
        stats += $"{hero.Intelligence}\n";
        stats += $"{hero.CurrentHealth}/{hero.MaxHealth}\n";
        stats += $"{hero.CurrentMana}/{hero.MaxMana}\n";
        stats += $"{hero.CurrentActionPoint}/{hero.MaxActionPoint}\n";
        stats += $"{hero.MoveRangeLeft}/{hero.MoveRange}\n";
        stats += $"{hero.CritChance}%\n";
        stats += $"x{hero.CritMultiplier}\n";
        statText.text = stats;
    }
}
