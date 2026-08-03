using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;

public class HealAction : BaseAction
{
    //Properties
    [Header("Heal Properties")]
    public int healingAmount = 0;

    //Methods
    private void Awake()
    {
        ActionDescription = $"Heals target for {healingAmount} + {ScalingAttribute}";
    }

    public override void Execute(BaseUnit user, List<BaseTile> targetTiles)
    {
        base.Execute(user, targetTiles);
        if (targetTiles == null || user == null) return;

        foreach (BaseTile tile in targetTiles)
        {
            if (tile.OccupiedUnit != null)
            {
                BaseUnit target = tile.OccupiedUnit;

                int scaling = GetScalingAttribute(user);
                int amountOfHealing = healingAmount + scaling;
                target.Heal(amountOfHealing);
            }
        }
    }
}
