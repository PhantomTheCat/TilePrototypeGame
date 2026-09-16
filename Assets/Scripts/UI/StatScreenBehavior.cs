using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatScreenBehavior : MonoBehaviour
{
    //Properties
    [Header("Character Profile")]
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI baseStats;

    [Header("Experience Bar")]
    [SerializeField] private Slider expBar;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Stat Bar")]
    [SerializeField] private TextMeshProUGUI statText;

    [Header("Perk Rows")]
    [SerializeField] private TextMeshProUGUI perkPointsText;
    [SerializeField] private PerkButton perkButtonPrefab;
    [SerializeField] private PerkRowBehavior[] perkRows;

    [Header("Perk Description")]
    [SerializeField] private GameObject perkDescription;
    [SerializeField] private TextMeshProUGUI perkNameText;
    [SerializeField] private TextMeshProUGUI perkDescriptionText;
    [SerializeField] private Vector3 offset = new Vector3(200, 0, 0);
    [HideInInspector] public UnityEvent OnPerkPurchased = new UnityEvent();

    private bool isWorking = true;


    //Methods
    private void Awake()
    {
        if (portrait ==  null || baseStats == null 
            || nameText == null || statText == null 
            || expBar == null || expText == null || perkRows == null)
        {
            Debug.LogError("Not everything is set up for the stat screen.");
            isWorking = false;
        }

        BaseHero hero = UnitManager.Instance.SelectedHero;
        if (hero == null) return;
        OnPerkPurchased.AddListener(() => UpdateStats(hero));
        OnPerkPurchased.AddListener(() => UIManager.Instance.UpdateCharacterButtons());
        OnPerkPurchased.AddListener(() => UpdatePerkText(hero));
    }

    public void UpdateUserProfile()
    {
        if (!isWorking) return;
        BaseHero hero = UnitManager.Instance.SelectedHero;
        if (hero == null) return;

        UpdateStats(hero);
        UpdatePerkRows(hero);
    }

    public void MovePerkDescription(PerkButton button)
    {
        if (perkDescription == null || perkNameText == null || perkDescriptionText == null) return;
        if (button == null || button.PerkAttached == null) return;
        perkDescription.SetActive(true);
        perkNameText.text = button.PerkAttached.PerkName;
        perkDescriptionText.text = button.PerkAttached.Description;

        //Move the description using the offset
        Vector3 newPos = button.transform.position + offset;
        perkDescription.transform.position = newPos;
    }

    public void HidePerkDescription()
    {
        if (perkDescription == null) return;
        perkDescription.SetActive(false);
    }

    private void UpdatePerkRows(BaseHero hero)
    {
        UpdatePerkText(hero);
        for (int i = 0; i < hero.PerkTrees.Length; i++)
        {
            PerkRowBehavior row = perkRows[i];
            PerkTree tree = hero.PerkTrees[i];
            row.ParentScreen = this;
            row.SetPerkRow(tree, perkButtonPrefab);
        }
    }

    private void UpdatePerkText(BaseHero hero)
    {
        perkPointsText.text = $"Perk Points: {hero.PerkPoints}";
    }

    private void UpdateStats(BaseHero hero)
    {
        portrait.sprite = hero.UnitPortrait;
        nameText.text = hero.UnitName;
        baseStats.text = $"{hero.UnitLevel}\nN/A";

        expBar.value = (float)hero.CurrentExperience / hero.ExpToNextLevel;
        expText.text = $"{hero.CurrentExperience}/{hero.ExpToNextLevel}";

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
