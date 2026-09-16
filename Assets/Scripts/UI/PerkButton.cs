using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class PerkButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Properties
    [SerializeField] private TextMeshProUGUI costText;
    [HideInInspector] public int Index;
    [HideInInspector] public BasePerk PerkAttached;
    private PerkRowBehavior parentRow;
    private Button button;
    private Image image;


    //Methods
    public void Activate(BasePerk perk, PerkRowBehavior row)
    {
        if (perk == null || row == null) return;
        PerkAttached = perk;
        parentRow = row;

        button = GetComponent<Button>();
        image = GetComponent<Image>();

        button.onClick.AddListener(OnClick);

        if (PerkAttached.Icon != null)
        {
            image.sprite = PerkAttached.Icon;
        }
        if (costText != null)
        {
            costText.text = PerkAttached.PointCost.ToString();
        }
    }

    public void OnClick()
    {
        if (CheckIfEligibleForPurchase())
        {
            PurchasePerk();
        }
    }

    private void PurchasePerk()
    {
        BaseHero hero = UnitManager.Instance.SelectedHero;
        if (hero == null || PerkAttached == null) return;
        hero.PerkPoints -= PerkAttached.PointCost;
        PerkAttached.IsBought = true;
        PerkAttached.ApplyPerk(hero);
        parentRow.ParentScreen.OnPerkPurchased.Invoke();
    }

    private bool CheckIfEligibleForPurchase()
    {
        BaseHero hero = UnitManager.Instance.SelectedHero;
        if (hero == null || PerkAttached == null) return false;
        if (hero.PerkPoints < PerkAttached.PointCost) return false;
        if (PerkAttached.IsBought) return false;
        if (Index > 0)
        {
            if (parentRow.PerkButtons[Index - 1].PerkAttached == null
                || !parentRow.PerkButtons[Index - 1].PerkAttached.IsBought) return false;
        }
        return true;
    }

    public void UpdateButtonState()
    {
        if (PerkAttached.IsBought)
        {
            button.interactable = false;
            image.color = Color.green;
        }
        else
        if (CheckIfEligibleForPurchase())
        {
            button.interactable = true;
            image.color = Color.white;
        }
        else
        {
            button.interactable = false;
            image.color = Color.gray;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parentRow.ParentScreen.MovePerkDescription(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentRow.ParentScreen.HidePerkDescription();
    }
}