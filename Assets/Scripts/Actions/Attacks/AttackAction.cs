using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : BaseAction
{
    //Properties
    [Header("Attack Stats")]
    public int DamageAmount = 1;
    public AttackType RangeType;
    public DamageType TypeOfDamage;


    public enum AttackType
    {
        MELEE,
        RANGED,
    }

    public enum DamageType
    {
        PHYSICAL,
        MAGICAL,
    }

    //Methods
    public override void Execute(BaseUnit attacker, BaseUnit target)
    {
        //TODO: Implement attack logic based on RangeType, TypeOfDamage, and AreaType
        base.Execute(attacker, target);
        if (target == null || attacker == null) return;

        // Apply damage to the target unit
        int scaling = GetScalingAttribute(attacker);
        target.TakeDamage(DamageAmount + scaling);
    }
}
