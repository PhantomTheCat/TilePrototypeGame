using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerkTree
{
    //Properties
    public PerkType Type;
    public List<BasePerk> PerkPrefabs = new List<BasePerk>();
    public List<BasePerk> Perks = new List<BasePerk>();
    public int MaxTier = 0;
    public List<ScriptablePerk> perkList = new List<ScriptablePerk>();


    //Methods
    public PerkTree(PerkType type, int maxTier)
    {
        Type = type;
        MaxTier = maxTier;
        LoadPerks();
        GetPerkPrefabs();
    }

    private void LoadPerks()
    {
        perkList = Resources.LoadAll<ScriptablePerk>($"Perks").ToList();
        perkList = perkList.Where(p => p.Perk.Type == Type).ToList();
    }

    private void GetPerkPrefabs()
    {
        for (int i = 1; i <= MaxTier; i++)
        {
            BasePerk[] perksInTier = perkList.Where(p => p.Perk.PerkTier == i).Select(p => p.Perk).ToArray();
            if (perksInTier.Length == 0)
            {
                Debug.LogWarning($"No perks found for tier {i} in perk tree of type {Type}");
                continue;
            }
            PerkPrefabs.Add(perksInTier[Random.Range(0, perksInTier.Length)]);
        }
    }
}

public enum PerkType
{
    GENERAL = 0,
    FIGHTER = 1,
    RANGER = 2,
    WIZARD = 3,
    CLERIC = 4,
}