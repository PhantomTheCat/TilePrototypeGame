using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Represents the base class for all hero units in the game.
/// </summary>
public class BaseHero : BaseUnit
{
    //Properties



    //Methods
    public override void CheckPath(BaseTile targetTile)
    {
        base.CheckPath(targetTile);

        if (MoveState == UnitState.MOVING)
        {
            GridManager.Instance.ClearAllHighlights();
        }
    }

    //TODO: Implement movement, attack, and other hero-specific behaviors
    //TODO: Add inventory selection and management
}
