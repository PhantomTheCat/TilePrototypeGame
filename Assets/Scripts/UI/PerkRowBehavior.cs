using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[RequireComponent(typeof(HorizontalLayoutGroup))]
public class PerkRowBehavior : MonoBehaviour
{
    //Properties
    private PerkTree Tree;
    [HideInInspector] public List<PerkButton> PerkButtons = new List<PerkButton>();
    [HideInInspector] public StatScreenBehavior ParentScreen;


    //Methods
    public void SetPerkRow(PerkTree perkTree, PerkButton perkPrefab)
    {
        Tree = perkTree;
        if (ParentScreen == null) Debug.LogError("PerkRowBehavior: ParentScreen is null.");

        // Clear existing perk buttons
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        PerkButtons.Clear();
        int index = 0;

        foreach (BasePerk perk in Tree.Perks)
        {
            PerkButton perkButton = Instantiate(perkPrefab, transform);
            perkButton.Activate(perk, this);
            PerkButtons.Add(perkButton);
            perkButton.Index = index;
            index++;
        }

        foreach (PerkButton button in PerkButtons)
        {
            button.UpdateButtonState();
            ParentScreen.OnPerkPurchased.AddListener(button.UpdateButtonState);
        }
    }
}
