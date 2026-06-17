using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Properties
    public static UIManager Instance;

    [Header("Hero Description")]
    [SerializeField] private TextMeshProUGUI selectedHeroText;

    [Header("Turn Indicator")]
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
    [SerializeField] private Image turnIndicatorBackground;
    [SerializeField] private Color heroTurnColor = Color.darkGreen;
    [SerializeField] private Color enemyTurnColor = Color.red;
    [SerializeField] private Color mistTurnColor = Color.darkMagenta;

    [Header("Action Buttons")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Sprite blankHotbarButtonSprite;
    [HideInInspector] private List<HotbarButtonBehavior> HotbarActionButtons;

    [Header("Description Panel")]
    [SerializeField] private GameObject descriptionBox;
    [SerializeField] private TextMeshProUGUI descriptionNameText;
    [SerializeField] private TextMeshProUGUI descriptionBodyText;

    [Header("Character Panel")]
    [SerializeField] private GameObject characterButtonParent;
    [SerializeField] private GameObject characterButtonPrefab;
    private List<CharacterButtonBehavior> characterButtons;

    [Header("Inventory")]
    [SerializeField] private GameObject inventorySlotParent;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject draggableItemPrefab;
    private List<InventorySlot> inventorySlots;

    [Header("Equipment Slots")]
    [SerializeField] private InventorySlot helmetSlot;
    [SerializeField] private InventorySlot armorSlot;
    [SerializeField] private InventorySlot bootsSlot;
    [SerializeField] private InventorySlot glovesSlot;
    [SerializeField] private InventorySlot weaponSlot;
    [SerializeField] private InventorySlot shieldSlot;
    [SerializeField] private InventorySlot trinket1Slot;
    [SerializeField] private InventorySlot trinket2Slot;
    [SerializeField] private InventorySlot[] consumableSlots;
    private InventorySlot[] equipmentSlots;

    [Header("Stat Screen")]
    [SerializeField] private StatScreenBehavior statScreen;


    //Methods
    private void Awake()
    {
        Instance = this;
        characterButtons = new List<CharacterButtonBehavior>();
        GetHotbar();
        GetInventorySlots();
        equipmentSlots = GetEquipmentSlots();
    }

    private InventorySlot[] GetEquipmentSlots()
    {
        List<InventorySlot> slots = new List<InventorySlot> { helmetSlot, armorSlot, bootsSlot, glovesSlot, weaponSlot, shieldSlot, trinket1Slot, trinket2Slot };
        slots.AddRange(consumableSlots);
        return slots.ToArray();
    }

    public void UpdateSelectedHeroUI(BaseHero hero)
    {
        selectedHeroText.text = hero != null ? $"{hero.UnitName}" : "N/A";
        GridManager.Instance.HighlightHeroTiles();
        UpdateHotbar(hero);
        UpdateInventorySlots();
        statScreen.UpdateUserProfile();
    }

    public void UpdateTurnIndicator(GameState state)
    {
        if (state == GameState.HERO_TURN)
        {
            turnIndicatorText.text = "Hero Turn";
            turnIndicatorBackground.color = heroTurnColor;
        }
        else if (state == GameState.ENEMY_TURN)
        {
            turnIndicatorText.text = "Enemy Turn";
            turnIndicatorBackground.color = enemyTurnColor;
        }
        else if (state == GameState.MIST_TURN)
        {
            turnIndicatorText.text = "Mist Turn";
            turnIndicatorBackground.color = mistTurnColor;
        }
    }

    private void GetHotbar()
    {
        //Finding the hotbar action buttons in the scene and adding them to the list
        HotbarActionButtons = new List<HotbarButtonBehavior>();
        for (int i = 1; i <= 10; i++)
        {
            HotbarButtonBehavior button = GameObject.Find($"ActionHotbar/Slot{i}").GetComponent<HotbarButtonBehavior>();
            if (button != null)
            {
                HotbarActionButtons.Add(button);
            }
            else
            {
                Debug.LogError($"Hotbar action button {i} not found in the scene.");
            }
        }
    }

    private void GetInventorySlots()
    {
        inventorySlots = inventorySlotParent.GetComponentsInChildren<InventorySlot>().ToList();
        if (inventorySlots == null || inventorySlots.Count == 0)
        {
            Debug.LogError("No inventory slots found under the specified parent.");
        }
    }

    public void MakeCharacterButtons()
    {
        int index = 1;
        //Instantiating the character buttons for the left panel (1 for each hero)
        foreach (BaseHero hero in UnitManager.Instance.Heroes)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, characterButtonParent.transform);
            CharacterButtonBehavior button = buttonObj.GetComponent<CharacterButtonBehavior>();
            if (button != null)
            {
                button.Activate();
                button.SetTiedCharacter(hero, index);
                characterButtons.Add(button);
                index++;
            }
            else
            {
                Debug.LogError("Character button prefab is missing the CharacterButtonBehavior component.");
            }
        }
    }

    public void UpdateCharacterButtons()
    {
        foreach (CharacterButtonBehavior button in characterButtons)
        {
            if (button != null)
            {
                button.UpdateHealth();
            }
        }
    }

    private void CreateNewDraggableItem(BaseItem itemData, Transform parent, BaseHero hero)
    {
        GameObject newItem = Instantiate(draggableItemPrefab, parent);
        DraggableItem draggable = newItem.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.Initialize(itemData, hero);
        }
    }

    public void UpdateInventorySlots()
    {
        ClearInventorySlots();

        //Filling the inventory slots with the selected hero's items
        BaseHero selectedHero = UnitManager.Instance.SelectedHero;

        if (selectedHero.Inventory == null || selectedHero.Inventory.Count == 0)
        {
            HideItemDescription();
            return;
        }

        if (selectedHero.Inventory.Count > inventorySlots.Count)
        {
            Debug.LogWarning("Selected hero has more items in inventory than available slots. Some items will not be displayed.");
        }

        //Instantiating draggable item prefabs in the inventory slots and initializing them with the corresponding item data
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < selectedHero.Inventory.Count)
            {
                BaseItem itemData = selectedHero.Inventory[i];
                if (itemData == null)
                {
                    continue;
                }

                if (itemData.IsEquipped)
                {
                    continue; // Skip equipped items in the inventory display
                }

                CreateNewDraggableItem(itemData, inventorySlots[i].transform, selectedHero);

                if (i == 0)
                {
                    ShowInventoryItemDescription(itemData);
                }
            }
        }

        //Filling the equipment slots with the selected hero's equipped items
        BaseHero hero = UnitManager.Instance.SelectedHero;
        ShowEquippedItems(hero);
    }

    public void ClearInventorySlots()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (InventorySlot slot in equipmentSlots)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ShowEquippedItems(BaseHero hero)
    {
        int trinketIndex = 0;
        int consumableIndex = 0;

        foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
        {
            switch (itemType)
            {
                case ItemType.HELMET:
                    if (hero.Helmet != null)
                        CreateNewDraggableItem(hero.Helmet, helmetSlot.transform, hero);
                    break;
                case ItemType.ARMOR:
                    if (hero.Armor != null)
                        CreateNewDraggableItem(hero.Armor, armorSlot.transform, hero);
                    break;
                case ItemType.BOOTS:
                    if (hero.Boots != null)
                        CreateNewDraggableItem(hero.Boots, bootsSlot.transform, hero);
                    break;
                case ItemType.GLOVES:
                    if (hero.Gloves != null)
                        CreateNewDraggableItem(hero.Gloves, glovesSlot.transform, hero);
                    break;
                case ItemType.WEAPON:
                    if (hero.Weapon != null)
                        CreateNewDraggableItem(hero.Weapon, weaponSlot.transform, hero);
                    break;
                case ItemType.SHIELD:
                    if (hero.Shield != null)
                        CreateNewDraggableItem(hero.Shield, shieldSlot.transform, hero);
                    break;
                case ItemType.TRINKET:
                    while (trinketIndex < 2)
                    {
                        if (trinketIndex == 0 && hero.Trinket1 != null)
                        {
                            CreateNewDraggableItem(hero.Trinket1, trinket1Slot.transform, hero);
                        }
                        else if (trinketIndex == 1 && hero.Trinket2 != null)
                        {
                            CreateNewDraggableItem(hero.Trinket2, trinket2Slot.transform, hero);
                        }
                        trinketIndex++;
                    }
                    break;
                case ItemType.CONSUMABLE:
                    while (consumableIndex < 4)
                    {
                        if (hero.Consumables[consumableIndex] != null)
                        {
                            CreateNewDraggableItem(hero.Consumables[consumableIndex], consumableSlots[consumableIndex].transform, hero);
                        }
                        consumableIndex++;
                    }
                    break;
            }
        }
    }

    public void ShowInventoryItemDescription(BaseItem item)
    {
        if (item == null)
        {
            Debug.LogError("Item is null.");
            return;
        }

        itemNameText.text = item.ItemName;
        itemDescriptionText.text = item.Description;
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = item.ItemIcon;
    }

    public void HideItemDescription()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "";
        itemImage.gameObject.SetActive(false);
    }

    public void ShowDescriptionBox(BaseAction action, Vector3 placement)
    {
        if (descriptionBox == null || descriptionNameText == null || descriptionBodyText == null || action == null)
        {
            Debug.LogError("Description box or its components are not properly set up.");
            return;
        }

        descriptionBox.SetActive(true);

        if (action.ActionDescription == null || action.ActionName == null)
        {
            Debug.LogError("Action is missing a name or description.");
            return;
        }

        descriptionNameText.text = action.ActionName;
        descriptionBodyText.text = action.ActionDescription;
        descriptionBox.transform.position = placement;
    }

    public void HideHotbarDescription()
    {
        if (descriptionBox != null)
        {
            descriptionBox.SetActive(false);
        }
    }

    public void UpdateHotbar(BaseHero hero)
    {
        if (hero == null) return;
        if (HotbarActionButtons == null || HotbarActionButtons.Count == 0 || hero.Actions.Count < 1)
        {
            Debug.LogError("Hero has no actions or no Hotbar buttons set up");
            return;
        }

        //Updating the hotbar action buttons based on the selected hero's abilities and available actions
        for (int i = 0; i < HotbarActionButtons.Count; i++)
        {
            Button buttonComponent = HotbarActionButtons[i];
            HotbarButtonBehavior button = buttonComponent.GetComponent<HotbarButtonBehavior>();

            if (i < hero.Actions.Count)
            {
                BaseAction action = hero.Actions[i];

                if (action == null) return;

                if (button == null) 
                { 
                    Debug.LogError($"Hotbar button at index {i} is missing the HotbarButtonBehavior component.");
                    return; 
                }

                button.AssociatedAction = action;
                button.Icon.sprite = action.ToolbarImage;
                button.Icon.gameObject.SetActive(true);
                button.interactable = true;
            }
            else
            {
                button.Icon.sprite = blankHotbarButtonSprite;
                button.Icon.gameObject.SetActive(false);
                buttonComponent.interactable = false;
            }
        }
    }

    /// <summary>
    /// Called when the end turn button is clicked.
    /// </summary>
    public static void EndTurnButtonClicked()
    {
        if (GameManager.Instance.GameState == GameState.HERO_TURN)
        {
            GameManager.Instance.ChangeState(GameState.ENEMY_TURN);
        }
    }

    /// <summary>
    /// Called when an action button is clicked. 
    /// The button index corresponds to the action in the hero's hotbar, saying which button was clicked.
    /// </summary>
    /// <param name="buttonIndex"></param>
    public void ActionButtonClicked(BaseAction action)
    {
        if (GameManager.Instance.GameState != GameState.HERO_TURN) { return; }
        BaseHero selectedHero = UnitManager.Instance.SelectedHero;
        if (selectedHero == null) { return; }

        if (selectedHero.MoveState == BaseUnit.UnitState.USING_ACTION)
        {
            selectedHero.MoveState = BaseUnit.UnitState.IDLE;
            GridManager.Instance.HighlightHeroTiles();
            return;
        }

        selectedHero.CurrentAction = action;
        selectedHero.MoveState = BaseUnit.UnitState.USING_ACTION;

        GridManager.Instance.HighlightActionTiles(action);
    }
}
