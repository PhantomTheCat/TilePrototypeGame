using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AttackAction : BaseAction
{
    //Properties
    [Header("Attack Stats")]
    public int DamageAmount = 1;
    public DamageType TypeOfDamage;

    public enum DamageType
    {
        PHYSICAL = 0,
        FIRE = 1,
        ICE = 2,
    }

    //Methods
    public override void Execute(BaseUnit attacker, List<BaseTile> targetTiles)
    {
        base.Execute(attacker, targetTiles);
        if (targetTiles == null || attacker == null) return;

        foreach (BaseTile tile in targetTiles)
        {
            tile.ActivateEffect(TypeOfDamage);

            if (tile.OccupiedUnit != null)
            {
                BaseUnit target = tile.OccupiedUnit;

                // Apply damage to the target unit
                int scaling = GetScalingAttribute(attacker);
                int damage = DamageAmount + scaling;
                target.TakeDamage(damage);
            }
        }
    }
}
