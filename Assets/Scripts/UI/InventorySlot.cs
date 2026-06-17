using System.Linq;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    //Properties
    [Header("Slot Settings")]
    [Tooltip("If true, this slot will only accept specific item types.")]
    [SerializeField] private bool isEquipmentSlot;
    [SerializeField] private ItemType[] acceptedItemTypes;
    [Tooltip("This corresponds to the index of the slot for items with multiple slots (trinkets and consumables).")]
    [SerializeField] private int slotIndex;

    //Methods
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        DraggableItem dragItem = droppedObject.GetComponent<DraggableItem>();
        if (dragItem == null) return;

        //See if we can swap items if the slot is already occupied
        if (transform.childCount > 0)
        {
            SwapItems(dragItem);
            return;
        }

        if (isEquipmentSlot)
        {
            if (acceptedItemTypes.Contains(dragItem.ItemData.Type))
            {
                dragItem.ParentAfterDrag = this.transform;
                dragItem.HeroTied.EquipItem(dragItem.ItemData, slotIndex);
            }
            return;
        }
        else
        {
            //Unequip the item, if not even equipped, the script will not do anything
            dragItem.HeroTied.UnequipItem(dragItem.ItemData, slotIndex);
            dragItem.ParentAfterDrag = this.transform;
        }
    }

    private void SwapItems(DraggableItem draggedItem)
    {
        BaseHero hero = UnitManager.Instance.SelectedHero;

        //Getting the existing item in this slot
        GameObject existingItem = transform.GetChild(0).gameObject;
        DraggableItem existingDragItem = existingItem.GetComponent<DraggableItem>();
        if (existingDragItem == null) { return; }

        //Getting the other inventory slot that the draggable item comes from
        GameObject otherInventorySlot = draggedItem.ParentAfterDrag.gameObject;
        InventorySlot otherSlot = otherInventorySlot.GetComponent<InventorySlot>();
        if (otherSlot == null) { return; }

        //Making sure both slots are compatible with the other's items
        if (isEquipmentSlot && !acceptedItemTypes.Contains(draggedItem.ItemData.Type)) {  return; }
        if (otherSlot.isEquipmentSlot && !otherSlot.acceptedItemTypes.Contains(existingDragItem.ItemData.Type)) { return; }

        //Properly equipping them or unequipping them
        //Already confirmed that both items are compatible with other slots
        if (isEquipmentSlot && otherSlot.isEquipmentSlot)
        {
            hero.EquipItem(draggedItem.ItemData, slotIndex);
            hero.EquipItem(existingDragItem.ItemData, otherSlot.slotIndex);
        }
        else if (isEquipmentSlot && !otherSlot.isEquipmentSlot)
        {
            hero.EquipItem(draggedItem.ItemData, slotIndex);
        }
        else if (!isEquipmentSlot && otherSlot.isEquipmentSlot)
        {
            hero.EquipItem(existingDragItem.ItemData, otherSlot.slotIndex);
        }

        //Now swapping them
        existingDragItem.ParentAfterDrag = draggedItem.ParentAfterDrag;
        existingDragItem.transform.SetParent(existingDragItem.ParentAfterDrag);
        draggedItem.ParentAfterDrag = transform;
    }
}
