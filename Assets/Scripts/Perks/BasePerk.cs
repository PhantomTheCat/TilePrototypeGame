using System.Collections.Generic;
using UnityEngine;

public class BasePerk : MonoBehaviour
{
    //Properties
    [Header("Perk Info")]
    public string PerkName;
    public Sprite Icon;
    public PerkType Type;
    [TextArea(3, 10)] public string Description;
    [Range(1, 6)] public int PointCost;
    [Range(1, 5)] public int PerkTier;

    [Header("Benefits")]
    [SerializeField] protected BaseAction[] Actions;
    [SerializeField] protected AffectableStat[] Affectables;
    [SerializeField] protected int[] Modifiers;
    protected Dictionary<AffectableStat, int> StatChanges = new Dictionary<AffectableStat, int>();

    [HideInInspector] public bool IsBought = false;


    //Methods
    public virtual void Activate()
    {
        if (CheckInspectorValues())
        {
            for (int i = 0; i < Affectables.Length; i++)
            {
                AffectableStat stat = Affectables[i];
                int modifier = Modifiers[i];
                StatChanges.Add(stat, modifier);
            }

            ChangeDescription();
        }
    }

    public virtual void ApplyPerk(BaseHero hero)
    {
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
                    Debug.LogError($"{PerkName} has duplicate affecting attributes. Item won't have attributes with this error.");
                    return false;
                }
            }
        }
        return true;
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
}