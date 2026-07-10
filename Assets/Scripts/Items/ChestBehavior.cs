using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChestBehavior : MonoBehaviour
{
    //Properties
    [Header("Chest Info")]
    [SerializeField] private Rarity rarity;
    [SerializeField] private int amountOfItems = 4;

    [Header("Item Chances")]
    [Range(0f, 1f)][SerializeField] private float commonChance = 0.1f;
    [Range(0f, 1f)][SerializeField] private float uncommonChance = 0.1f;
    [Range(0f, 1f)][SerializeField] private float rareChance = 0.1f;
    [Range(0f, 1f)][SerializeField] private float legendaryChance = 0.05f;


    [HideInInspector] public List<BaseItem> ItemsHeld = new List<BaseItem>();
    [HideInInspector] public BaseTile OccupiedTile;


    //Methods
    private void Start()
    {
        FillRandomLoot();
    }

    public void CallUIChestUpdate()
    {
        UIChestBehavior.Instance.OpenChest(this);
    }

    private Rarity GetRandomRarity()
    {
        //Add up all the chances then get a random number, from that number, we determine which rarity it is
        float wheel = commonChance + uncommonChance + rareChance + legendaryChance;
        float number = Random.Range(0f, wheel);

        number -= commonChance;
        if (number <= 0) return Rarity.COMMON;
        number -= uncommonChance;
        if (number <= 0) return Rarity.UNCOMMON;
        number -= rareChance;
        if (number <= 0) return Rarity.RARE;
        number -= legendaryChance;
        if (number <= 0) return Rarity.LEGENDARY;

        return Rarity.COMMON;
    }

    public void FillRandomLoot()
    {
        //Making all the items based off the amount of items and rarity chances
        for (int i = 0; i < amountOfItems; i++)
        {
            Rarity itemRarity = GetRandomRarity();
            BaseItem item = ItemManager.Instance.GetRandomItem(itemRarity);
            ItemsHeld.Add(item);
        }
    }
}
