using UnityEngine;

/// <summary>
/// Base class for all scriptable actions in the game that can be created easily in inspector.
/// </summary>
[CreateAssetMenu(fileName = "New Scriptable Action", menuName = "Scriptable Action")]
public class ScriptableAction : ScriptableObject
{
    public ActionType actionType;
    public BaseAction actionPrefab;
    public bool CanStartOnPlayer = false;
}

public enum ActionType
{
    MELEE_ATTACK = 0,
    RANGE_ATTACK = 1,
    SPELL_ATTACK = 2,
    HEAL = 3,
    SUMMON = 4,
    BUFF = 5,
}
