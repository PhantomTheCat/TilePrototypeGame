using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

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

        if (target == null || attacker == null)
        {
            Debug.LogWarning($"{ActionName}: Target or Attacker is null.");
            return;
        }

        // Apply damage to the target unit
        target.TakeDamage(DamageAmount);
    }

    public override List<BaseTile> GetActionTiles(BaseUnit user)
    {
        List<BaseTile> actionTiles = GridManager.Instance.GetValidTiles(user, Range, false);
        return actionTiles;
    }
}
