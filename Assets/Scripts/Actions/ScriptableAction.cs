using UnityEngine;

/// <summary>
/// Base class for all scriptable actions in the game that can be created easily in inspector.
/// </summary>
[CreateAssetMenu(fileName = "New Scriptable Action", menuName = "Scriptable Action")]
public class ScriptableAction : ScriptableObject
{
    public ActionType actionType;
    public BaseAction actionPrefab;
}

public enum ActionType
{
    ATTACK = 0,
    UTILITY = 1,
    USEOBJECT = 2
}
