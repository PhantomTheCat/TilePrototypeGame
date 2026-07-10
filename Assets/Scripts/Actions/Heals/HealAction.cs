using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

    public override void Execute(BaseUnit user, BaseUnit target)
    {
        base.Execute(user, target);
        if (target == null || user == null) return;

        int scaling = GetScalingAttribute(user);
        int amountOfHealing = healingAmount + scaling;
        target.Heal(amountOfHealing);
    }
}
