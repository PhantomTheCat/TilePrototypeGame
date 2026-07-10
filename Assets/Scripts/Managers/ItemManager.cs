using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    //Properties
    public static ItemManager Instance;
    private List<ScriptableItem> items;
    private List<ScriptableItem> commonItems = new List<ScriptableItem>();
    private List<ScriptableItem> uncommonItems = new List<ScriptableItem>();
    private List<ScriptableItem> rareItems = new List<ScriptableItem>();
    private List<ScriptableItem> legendaryItems = new List<ScriptableItem>();


    //Methods
    private void Awake()
    {
        Instance = this;
        items = Resources.LoadAll<ScriptableItem>("Items").ToList();
        SetUpLists();
    }

    private void SetUpLists()
    {
        commonItems = items.Where(a => a.Rarity == Rarity.COMMON).ToList();
        uncommonItems = items.Where(a => a.Rarity == Rarity.UNCOMMON).ToList();
        rareItems = items.Where(a => a.Rarity == Rarity.RARE).ToList();
        legendaryItems = items.Where(a => a.Rarity == Rarity.LEGENDARY).ToList();
    }

    private List<ScriptableItem> GetRarityList(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.COMMON:
                return commonItems;
            case Rarity.UNCOMMON:
                return uncommonItems;
            case Rarity.RARE:
                return rareItems;
            case Rarity.LEGENDARY:
                return legendaryItems;
        }
        return null;
    }

    /// <summary>
    /// Getting a random item from all the scriptable objects of a certain rarity
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public BaseItem GetRandomItem(Rarity rarity)
    {
        List<ScriptableItem> itemList = GetRarityList(rarity);
        if (itemList ==  null || itemList.Count == 0)
        {
            Debug.LogError($"No items of {rarity} rarity were found.");
            return null;
        }

        int randomInt = Random.Range(0, itemList.Count);
        ScriptableItem scriptItem = itemList[randomInt];
        BaseItem item = InstantiateItem(scriptItem);
        return item;
    }

    /// <summary>
    /// Getting a random item that fits into the type and rarity listed
    /// </summary>
    /// <param name="type"></param>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public BaseItem GetRandomItemOfType(ItemType type, Rarity rarity)
    {
        //Making the list only contain items of a certain type and be randomized
        List<ScriptableItem> itemList = GetRarityList(rarity);
        if (itemList == null || itemList.Count == 0)
        {
            Debug.LogError($"No items of {rarity} rarity were found.");
            return null;
        }

        itemList = itemList.Where(a => a.Type == type).ToList();
        int randomInt = Random.Range(0, itemList.Count);
        ScriptableItem scriptableItem = itemList[randomInt];

        BaseItem item = InstantiateItem(scriptableItem);
        return item;
    }


    public List<BaseItem> GetRandomItemList(int listCount, Rarity rarity)
    {
        List<ScriptableItem> itemList = GetRarityList(rarity);
        if (itemList == null || itemList.Count == 0 || itemList.Count < listCount)
        {
            Debug.LogError($"None or not enough items of {rarity} rarity were found.");
            return null;
        }

        itemList = itemList.OrderBy(a => Random.value).ToList();
        itemList = itemList.Take(listCount).ToList();

        //Instantiating all the items in finalList\
        List<BaseItem> baseItems = new List<BaseItem>();
        foreach (ScriptableItem item in itemList)
        {
            if (item.ItemPrefab != null)
            {
                BaseItem baseItem = InstantiateItem(item);
                baseItems.Add(baseItem);
            }
        }

        return baseItems;
    }

    public List<BaseItem> GetRandomItemListOfType(int listCount, ItemType type, Rarity rarity)
    {
        //Making the list only contain items of a certain type and be randomized
        List<ScriptableItem> itemList = GetRarityList(rarity);
        if (itemList == null || itemList.Count == 0 || itemList.Count < listCount)
        {
            Debug.LogError($"None or not enough items of {rarity} rarity were found.");
            return null;
        }

        itemList = items.OrderBy(a => Random.value).Where(a => a.Type == type).ToList();
        itemList = itemList.Take(listCount).ToList();

        //Instantiating all the items in finalList\
        List<BaseItem> baseItems = new List<BaseItem>();
        foreach (ScriptableItem item in itemList)
        {
            if (item.ItemPrefab != null)
            {
                BaseItem baseItem = InstantiateItem(item);
                baseItems.Add(baseItem);
            }
        }

        return baseItems;
    }

    private BaseItem InstantiateItem(ScriptableItem item)
    {
        if (item != null)
        {
            BaseItem baseItem = Instantiate(item.ItemPrefab, this.transform);
            baseItem.Activate();
            return baseItem;
        }
        return null;
    }
}
