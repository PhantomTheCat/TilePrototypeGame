using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class HotbarButtonBehavior : Button
{
    //Properties
    [SerializeField] public Image Icon;
    [HideInInspector] public BaseAction AssociatedAction;
    [HideInInspector] private Vector3 offset = new Vector3(0, 200, 0);

    //Methods
    protected override void Awake()
    {
        base.Awake();

        //Finding the child Image component to set the icon sprite later
        GameObject iconObj = transform.Find("Icon").gameObject;
        if (iconObj != null)
        {
            Icon = iconObj.GetComponent<Image>();
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (GameManager.Instance.GameState != GameState.HERO_TURN) return;
        if (AssociatedAction == null) return;

        UIManager.Instance.ActionButtonClicked(AssociatedAction);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (AssociatedAction != null)
        {
            Vector3 newPlacement = transform.position + offset;
            UIManager.Instance.ShowDescriptionBox(AssociatedAction, newPlacement);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        UIManager.Instance.HideHotbarDescription();
    }
}
