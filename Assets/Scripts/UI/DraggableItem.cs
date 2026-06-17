using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //Properties
    [HideInInspector] public BaseItem ItemData;
    [HideInInspector] public int Quantity = 1;
    [HideInInspector] public BaseHero HeroTied;
    [HideInInspector] public Transform ParentAfterDrag;
    private Image image;

    //Methods
    public void Initialize(BaseItem itemData, BaseHero hero)
    {
        image = GetComponent<Image>();

        if (itemData == null) { Destroy(this); return; }

        ItemData = itemData;
        image.sprite = itemData.ItemIcon;
        HeroTied = hero;

        if (itemData.CurrentStackSize <= itemData.MaxStackSize)
        {
            Quantity = itemData.CurrentStackSize;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ItemData != null) { UIManager.Instance.ShowInventoryItemDescription(ItemData); }
        ParentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Mouse.current.position.ReadValue();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(ParentAfterDrag);
        image.raycastTarget = true;
    }
}
