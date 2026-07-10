using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class CharacterButtonBehavior : MonoBehaviour, IDropHandler
{
    //Properties
    [Header("UI Elements")]
    [SerializeField] private GameObject deathBoxObject;
    [SerializeField] private TextMeshProUGUI healthText;

    /// <summary>
    /// Holds which button this in on the panel
    /// </summary>
    [HideInInspector] public int CharacterIndex;
    /// <summary>
    /// Holds the character that is currently tied to this button, if any. This is used to determine which character to select when the button is clicked.
    /// </summary>
    [HideInInspector] public BaseHero tiedCharacter;
    private Button button;
    private Image portraitImage;
    private bool isActive = false;


    //Methods
    public void Activate()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        portraitImage = GetComponent<Image>();
        isActive = true;
    }

    private void OnButtonClicked()
    {
        if (tiedCharacter == null || !isActive) return;
        UnitManager.Instance.ChangeSelectedHero(tiedCharacter);
    }

    public void SetTiedCharacter(BaseHero hero, int index)
    {
        if (hero == null || !isActive) return;
        tiedCharacter = hero;
        CharacterIndex = index;
        portraitImage.sprite = hero.UnitPortrait;
        UpdateHealth();
    }

    public void UpdateHealth()
    {
        healthText.text = $"{tiedCharacter.CurrentHealth}/{tiedCharacter.MaxHealth}";

        if (tiedCharacter.CurrentHealth <= 0)
        {
            deathBoxObject.SetActive(true);
            isActive = false;
        }
        else
        {
            deathBoxObject.SetActive(false);
            isActive = true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!isActive) return;
        //Get dropped object
        GameObject droppedObject = eventData.pointerDrag;
        DraggableItem dragItem = droppedObject.GetComponent<DraggableItem>();
        if (dragItem == null) return;

        //Make sure the item being inventory swapped is not already tied to this character
        if (dragItem.HeroTied != tiedCharacter)
        {
            if (tiedCharacter.Inventory.Count >= tiedCharacter.InventorySize)
            {
                Debug.Log("Character inventory is full, cannot swap item.");
                return;
            }

            //Swap item to character tied to this button
            tiedCharacter.Inventory.Add(dragItem.ItemData);
            dragItem.HeroTied.Inventory.Remove(dragItem.ItemData);

            //Destroy the dragged item and remove the item description from the UI
            Destroy(droppedObject);
            UIManager.Instance.HideItemDescription();
        }
    }
}
