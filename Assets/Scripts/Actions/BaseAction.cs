using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class BaseAction : MonoBehaviour
{
    //Properties
    [Header("General")]
    public ActionType ActionType;
    public AffectableStat ScalingAttribute;
    public AreaType AreaShape;
    public int Range = 1;
    [Tooltip("Horizontal Area will be used as the default size for symmetrical areas")]
    [Range(0, 10)]
    public int HorizontalArea = 1;
    [Range(0, 10)]
    public int VerticalArea = 1;
    public bool CanTargetUser = false;

    [Header("Action Info")]
    public string ActionName;
    public string ActionDescription;

    [Header("Action UI")]
    public Sprite ToolbarImage;


    /// <summary>
    /// The type of area the action will take up
    /// </summary>
    public enum AreaType 
    { 
        SINGLE = 0,
        CIRCLE = 1,
        RECTANGLE = 2,
        CONE = 3,
    }


    //Methods
    public virtual void Execute(BaseUnit user, BaseUnit target)
    {
        if (target == null || user == null)
        {
            return;
        }
    }

    /// <summary>
    /// Gets the tiles for the range of the action (where our center can be)
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public virtual List<BaseTile> GetTargetTiles(BaseUnit user)
    {
        List<BaseTile> tilesInRange = GridManager.Instance.GetValidTiles(user, Range, false, true, CanTargetUser);
        return tilesInRange; 
    }

    /// <summary>
    /// Get the tiles for the action based on the center and area type + size
    /// </summary>
    /// <param name="center"></param>
    /// <returns></returns>
    public virtual List<BaseTile> GetActionTiles(BaseUnit unit, BaseTile center)
    {
        GridManager grid = GridManager.Instance;
        switch (AreaShape)
        {
            case AreaType.SINGLE:
                List<BaseTile> centerTiles = new List<BaseTile> { center };
                return centerTiles;
            case AreaType.CIRCLE:
                return grid.GetCircleArea(unit, center, HorizontalArea);
            case AreaType.RECTANGLE:
                return grid.GetRectangleArea(unit, center, HorizontalArea, VerticalArea);
            case AreaType.CONE:
                return grid.GetConeArea(unit.OccupiedTile, center, HorizontalArea, VerticalArea);
            default:
                return null;
        }
    }

    public int GetScalingAttribute(BaseUnit unit)
    {
        switch (ScalingAttribute)
        {
            case AffectableStat.STR:
                return unit.Strength;
            case AffectableStat.DEX:
                return unit.Dexterity;
            case AffectableStat.CON:
                return unit.Constitution;
            case AffectableStat.FAITH:
                return unit.Faith;
            case AffectableStat.INT:
                return unit.Intelligence;
        }

        //Not all attributes are scaled, so return this in case it's N/A
        return 0;
    }
}
