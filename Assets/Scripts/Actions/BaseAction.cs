using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BaseAction : MonoBehaviour
{
    //Properties
    [Header("General")]
    public ActionType ActionType;
    public int Range = 1;
    public int Area = 1;

    [Header("Action Info")]
    public string ActionName;
    public string ActionDescription;

    [Header("Action UI")]
    public Sprite ToolbarImage;

    //Methods
    public virtual void Execute(BaseUnit user, BaseUnit target)
    {

    }

    public virtual List<BaseTile> GetActionTiles(BaseUnit user)
    {
        return GridManager.Instance.GetValidTiles(user, Range, false);
    }
}
