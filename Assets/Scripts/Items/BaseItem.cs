using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BaseItem : MonoBehaviour
{
    //Properties
    [Header("General")]
    public string ItemName;
    public Sprite ItemIcon;
    public string Description;
    public int GoldValue;
    public ItemType Type;
    public bool IsConsumable;
    public bool IsEquipped;

    [Header("Actions")]
    public List<BaseAction> Actions;

    [Header("Stacking")]
    public bool IsStackable;
    public int CurrentStackSize = 1;
    public int MaxStackSize = 1;
    //private int minStackSize = 1;
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
