using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CharacterButtonBehavior : Button, IDropHandler
{
    //Properties
    /// <summary>
    /// Holds which button this in on the panel
    /// </summary>
    [HideInInspector] public int CharacterIndex;
    /// <summary>
    /// Holds the character that is currently tied to this button, if any. This is used to determine which character to select when the button is clicked.
    /// </summary>
    [HideInInspector] public BaseHero tiedCharacter;
    private Image portraitImage;
    private TextMeshProUGUI healthText;
    private bool isActive = false;


    //Methods
    public void Activate()
    {
        onClick.AddListener(OnButtonClicked);
        portraitImage = GetComponent<Image>();
        healthText = GetComponentInChildren<TextMeshProUGUI>();
        isActive = true;
    }

    private void OnButtonClicked()
    {
        if (tiedCharacter == null) return;
        UnitManager.Instance.ChangeSelectedHero(tiedCharacter);
    }

    public void SetTiedCharacter(BaseHero hero, int index)
    {
        if (hero == null || !isActive) return;
        tiedCharacter = hero;
        CharacterIndex = index;
        portraitImage.sprite = hero.UnitPortrait;
    }

    public void UpdateHealth()
    {
        healthText.text = $"{tiedCharacter.CurrentHealth}/{tiedCharacter.MaxHealth}";
    }

    public void OnDrop(PointerEventData eventData)
    {
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
