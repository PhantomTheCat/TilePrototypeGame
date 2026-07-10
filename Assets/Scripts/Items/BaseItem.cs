using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NUnit.Framework;

public class BaseItem : MonoBehaviour
{
    //Properties
    [Header("General")]
    public string ItemName;
    public Sprite ItemIcon;
    public string Description;
    public int GoldValue;
    public ItemType Type;
    public Rarity Rarity;
    public bool IsConsumable;

    [Header("Actions")]
    public List<BaseAction> Actions;

    [Header("Modifiers")]
    [Tooltip("Affectables and modifiers must be the same length, with no repeating Affectables")]
    public AffectableStat[] Affectables;
    public int[] Modifiers;
    private Dictionary<AffectableStat, int> StatChanges = new Dictionary<AffectableStat, int>();

    [Header("Stacking")]
    public bool IsStackable;
    public int CurrentStackSize = 1;
    public int MaxStackSize = 1;

    [HideInInspector] public bool IsEquipped;
    private bool isActive = false;


    //Methods
    public void Activate()
    {
        if (isActive) return;
        if (CheckInspectorValues())
        {
            for (int i = 0; i < Affectables.Length; i++)
            {
                AffectableStat stat = Affectables[i];
                int modifier = Modifiers[i];
                StatChanges.Add(stat, modifier);
            }

            ChangeDescription();

            isActive = true;
        }
    }

    private void ChangeDescription()
    {
        //Changing description of item to add the stat changes
        foreach (AffectableStat stat in StatChanges.Keys)
        {
            if (StatChanges[stat] == 0) continue;

            if (StatChanges[stat] > 0)
            {
                Description = Description + $"\n +{StatChanges[stat]} to {stat}";
            }
            else
            {
                Description = Description + $"\n -{StatChanges[stat]} to {stat}";
            }
        }

        if (Actions == null) return;
        foreach (BaseAction action in Actions)
        {
            Description = Description + $"\n Grants the {action.ActionName} action";
        }
    }

    public void OnEquip(BaseHero hero)
    {
        if (IsConsumable) return;
        if (!IsEquipped) return;


        if (Actions != null)
        {
            //Apply the stat changes and actions to the character
            foreach (BaseAction action in Actions)
            {
                hero.AddAction(action);
            }

            UIManager.Instance.UpdateHotbar(hero);
        }

        if (Modifiers != null)
        {
            foreach (AffectableStat stat in StatChanges.Keys)
            {
                int i = StatChanges[stat];

                if (i != 0)
                {
                    hero.StatUpdate(stat, i);
                }
            }
        }
    }

    public void OnUnequip(BaseHero hero)
    {
        if (IsConsumable) return;
        if (IsEquipped) return;

        if (Actions != null)
        {
            //Remove actions tied to this item
            foreach (BaseAction action in Actions)
            {
                hero.RemoveAction(action);
            }

            UIManager.Instance.UpdateHotbar(hero);
        }

        if (Affectables != null)
        {
            //Reverse the stat upgrade or downgrade by multiplying by -1
            foreach (AffectableStat stat in StatChanges.Keys)
            {
                int i = StatChanges[stat];

                if (i != 0)
                {
                    hero.StatUpdate(stat, -i);
                }
            }
        }
    }

    private bool CheckInspectorValues()
    {
        if (Affectables == null || Modifiers == null) return false;
        if (Affectables.Length != Modifiers.Length) return false;

        //Make sure there are no duplicate affectables
        for (int i = 0; i < Affectables.Length; i++)
        {
            for (int n = 0; n < Affectables.Length; n++)
            {
                if (i == n) continue;

                if (Affectables[i] == Affectables[n])
                {
                    Debug.LogError($"{ItemName} has duplicate affecting attributes. Item won't have attributes with this error.");
                    return false;
                }
            }
        }
        return true;
    }
}

public enum ItemType
{
    WEAPON,
    SHIELD,
    HELMET,
    ARMOR,
    GLOVES,
    BOOTS,
    TRINKET,
    CONSUMABLE,
    OTHER
}

public enum Rarity
{
    COMMON = 0,
    UNCOMMON = 1,
    RARE = 2,
    LEGENDARY = 3
}
