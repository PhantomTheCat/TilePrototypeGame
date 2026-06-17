using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Represents the base class for all hero units in the game.
/// </summary>
public class BaseHero : BaseUnit
{
    //Properties
    [HideInInspector] public BaseItem Helmet;
    [HideInInspector] public BaseItem Armor;
    [HideInInspector] public BaseItem Boots;
    [HideInInspector] public BaseItem Gloves;
    [HideInInspector] public BaseItem Weapon;
    [HideInInspector] public BaseItem Shield;
    [HideInInspector] public BaseItem Trinket1;
    [HideInInspector] public BaseItem Trinket2;
    [HideInInspector] public BaseItem[] Consumables = new BaseItem[4];
    private List<BaseItem> equippedItems = new List<BaseItem>();



    //Methods
    public override void CheckPath(BaseTile targetTile)
    {
        base.CheckPath(targetTile);

        if (MoveState == UnitState.MOVING)
        {
            GridManager.Instance.ClearAllHighlights();
        }
    }

    public override void Die()
    {
        base.Die();
        UnitManager.Instance.DefeatedHeroes.Add(this);
        UnitManager.Instance.Heroes.Remove(this);
    }

    public override void Revive(BaseTile spawnTile)
    {
        base.Revive(spawnTile);
        UnitManager.Instance.DefeatedHeroes.Remove(this);
        UnitManager.Instance.Heroes.Add(this);
    }

    /// <summary>
    /// Logic for equipping an item to the hero. Slot index is only relevant for trinkets and consumables, which have multiple slots.
    /// </summary>
    /// <param name="item">The item to equip.</param>
    /// <param name="itemType">The type of the item.</param>
    /// <param name="slotIndex">The slot index for items with multiple slots (trinkets and consumables), starting with 0.</param>
    public void EquipItem(BaseItem item, int slotIndex)
    {
        bool successfullyEquipped = false;
        BaseItem prevItem = null;

        switch (item.Type)
        {
            case ItemType.HELMET:
                prevItem = Helmet;
                Helmet = item;
                successfullyEquipped = true;
                break;
            case ItemType.ARMOR:
                prevItem = Armor;
                Armor = item;
                successfullyEquipped = true;
                break;
            case ItemType.BOOTS:
                prevItem = Boots;
                Boots = item;
                successfullyEquipped = true;
                break;
            case ItemType.GLOVES:
                prevItem = Gloves;
                Gloves = item;
                successfullyEquipped = true;
                break;
            case ItemType.WEAPON:
                prevItem = Weapon;
                Weapon = item;
                successfullyEquipped = true;
                break;
            case ItemType.SHIELD:
                prevItem = Shield;
                Shield = item;
                successfullyEquipped = true;
                break;
            case ItemType.TRINKET:
                if (slotIndex == 0)
                {
                    prevItem = Trinket1;
                    Trinket1 = item;
                    successfullyEquipped = true;
                }
                else if (slotIndex == 1)
                {
                    prevItem = Trinket2;
                    Trinket2 = item;
                    successfullyEquipped = true;
                }
                else
                    Debug.LogWarning("Invalid trinket slot index: " + slotIndex);
                break;
            case ItemType.CONSUMABLE:
                if (slotIndex >= 0 && slotIndex < Consumables.Length)
                {
                    prevItem = Consumables[slotIndex];
                    Consumables[slotIndex] = item;
                    successfullyEquipped = true;
                }
                else
                    Debug.LogWarning("Invalid consumable slot index: " + slotIndex);
                break;
            default:
                Debug.LogWarning("Invalid item type for equipping: " + item.Type);
                break;
        }

        if (successfullyEquipped)
        {
            item.IsEquipped = true;
            equippedItems.Add(item);

            if (prevItem != null)
            {
                prevItem.IsEquipped = false;
                if (equippedItems.Contains(prevItem))
                    equippedItems.Remove(prevItem);
            }
        }
    }

    public void UnequipItem(BaseItem item, int slotIndex)
    {
        //Check to see if item is equipped
        if (!equippedItems.Contains(item))
            return;

        equippedItems.Remove(item);
        item.IsEquipped = false;

        switch (item.Type)
        {
            case ItemType.HELMET:
                Helmet = null;
                break;
            case ItemType.ARMOR:
                Armor = null;
                break;
            case ItemType.BOOTS:
                Boots = null;
                break;
            case ItemType.GLOVES:
                Gloves = null;
                break;
            case ItemType.WEAPON:
                Weapon = null;
                break;
            case ItemType.SHIELD:
                Shield = null;
                break;
            case ItemType.TRINKET:
                if (slotIndex == 0)
                {
                    Trinket1 = null;
                }
                else if (slotIndex == 1)
                {
                    Trinket2 = null;
                }
                else
                    Debug.LogWarning("Invalid trinket slot index: " + slotIndex);
                break;
            case ItemType.CONSUMABLE:
                if (slotIndex >= 0 && slotIndex < Consumables.Length)
                {
                    Consumables[slotIndex] = null;
                }
                else
                    Debug.LogWarning("Invalid consumable slot index: " + slotIndex);
                break;
            default:
                Debug.LogWarning("Invalid item type for equipping: " + item.Type);
                break;
        }
    }


    //TODO: Implement movement, attack, and other hero-specific behaviors
    //TODO: Add inventory selection and management
}
