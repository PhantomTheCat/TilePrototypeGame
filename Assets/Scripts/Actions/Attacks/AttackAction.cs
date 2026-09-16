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
                bool isCrit = SeeIfCrit(attacker);
                if (isCrit)
                {
                    damage *= attacker.CritMultiplier;
                }
                target.TakeDamage(damage, isCrit);
            }
        }
    }

    public bool SeeIfCrit(BaseUnit attacker)
    {
        if (attacker == null) return false;
        float critChance = attacker.CritChance;
        float roll = Random.Range(0f, 100f);
        return roll <= critChance;
    }
}

public enum DamageType
{
    PHYSICAL = 0,
    FIRE = 1,
    ICE = 2,
    THUNDER = 3,
    POISON = 4,
    LIGHT = 5,
    BLOOD = 6,
    DARK = 7,
}
