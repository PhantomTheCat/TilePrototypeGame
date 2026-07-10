using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UIChestBehavior : MonoBehaviour
{
    //Properties
    public static UIChestBehavior Instance;
    [HideInInspector] public DraggableItem ItemDragged;
    private ChestBehavior currentChest;
    private List<InventorySlot> slots;
    private List<DraggableItem> draggedItems = new List<DraggableItem>();

    //Methods
    public void Activate()
    {
        Instance = this;
        slots = GetComponentsInChildren<InventorySlot>().ToList();
    }

    public void OpenChest(ChestBehavior chest)
    {
        this.gameObject.SetActive(true);
        UpdateChestUI(chest);
    }

    public void CloseChest()
    {
        ClearChest();
        this.gameObject.SetActive(false);
    }

    private void UpdateChestUI(ChestBehavior chest)
    {
        if (chest == null) return;
        ClearChest();
        currentChest = chest;
        if (chest.ItemsHeld == null || chest.ItemsHeld.Count == 0) return;

        for (int i = 0; i < chest.ItemsHeld.Count; i++)
        {
            BaseItem item = chest.ItemsHeld[i];
            if (item == null) continue;

            //Create a draggable item
            DraggableItem dragItem = UIManager.Instance.CreateNewDraggableItem(item, slots[i].transform, null);
            draggedItems.Add(dragItem);
        }
    }

    private void ClearChest()
    {
        foreach (InventorySlot slot in slots)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void TakeOneItem()
    {
        if (draggedItems.Count == 0) return;
        BaseHero hero = UnitManager.Instance.SelectedHero;

        if (ItemDragged == null)
        {
            //Take the first item
            DraggableItem item = null;
            for (int i = 0; i < draggedItems.Count; i++)
            {
                if (item != null) { continue; }
                if (draggedItems[i] != null)
                {
                    item = draggedItems[i];
                }
            }

            MoveItemToHeroInventory(hero, item);
        }
        else
        {
            //Take the dragged item
            MoveItemToHeroInventory(hero, ItemDragged);
        }
    }

    public void TakeAllItems()
    {
        if (draggedItems.Count == 0) return;
        BaseHero hero = UnitManager.Instance.SelectedHero;

        //Move all items to the selected player's inventory
        for (int i = 0;i < draggedItems.Count; i++)
        {
            if (draggedItems[i] == null) continue;
            MoveItemToHeroInventory(hero, draggedItems[i]);
        }

        draggedItems.Clear();
    }

    private void MoveItemToHeroInventory(BaseHero hero, DraggableItem item)
    {
        if (item == null) return;
        if (!draggedItems.Contains(item)) return;
        if (!currentChest.ItemsHeld.Contains(item.ItemData)) return;

        hero.Inventory.Add(item.ItemData);
        currentChest.ItemsHeld.Remove(item.ItemData);
        Destroy(item.gameObject);
    }
}
