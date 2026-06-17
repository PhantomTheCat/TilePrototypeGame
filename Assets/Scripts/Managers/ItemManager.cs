using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    //Properties
    public static ItemManager Instance;
    private List<ScriptableItem> items;


    //Methods
    private void Awake()
    {
        Instance = this;
        items = Resources.LoadAll<ScriptableItem>("Items").ToList();
    }

    /// <summary>
    /// Gives a random Item from our examples
    /// </summary>
    /// <returns></returns>
    public BaseItem GetRandomItem()
    {
        int randomInt = Random.Range(0, items.Count);
        ScriptableItem scriptItem = items[randomInt];

        //Instantiating the item
        BaseItem item = InstantiateItem(scriptItem);
        return item;
    }

    public BaseItem GetRandomItemOfType(ItemType type)
    {
        //Making the list only contain items of a certain type and be randomized
        List<ScriptableItem> itemList = items.OrderBy(a => a.Type == type).ToList();
        int randomInt = Random.Range(0, itemList.Count);
        ScriptableItem scriptableItem = itemList[randomInt];

        BaseItem item = InstantiateItem(scriptableItem);
        return item;
    }


    public List<BaseItem> GetRandomItemList(int listCount)
    {
        List<ScriptableItem> itemList = items.OrderBy(a => Random.value).ToList();
        List<ScriptableItem> finalList = itemList.Take(listCount).ToList();

        //Instantiating all the items in finalList\
        List<BaseItem> baseItems = new List<BaseItem>();
        foreach (ScriptableItem item in finalList)
        {
            if (item.ItemPrefab != null)
            {
                BaseItem baseItem = InstantiateItem(item);
                baseItems.Add(baseItem);
            }
        }

        return baseItems;
    }

    public List<BaseItem> GetRandomItemListOfType(int listCount, ItemType type)
    {
        //Making the list only contain items of a certain type and be randomized
        List<ScriptableItem> itemList = items.OrderBy(a => Random.value).ThenBy(a => a.Type == type).ToList();
        List<ScriptableItem> finalList = itemList.Take(listCount).ToList();

        //Instantiating all the items in finalList\
        List<BaseItem> baseItems = new List<BaseItem>();
        foreach (ScriptableItem item in finalList)
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
            return baseItem;
        }
        return null;
    }
}
