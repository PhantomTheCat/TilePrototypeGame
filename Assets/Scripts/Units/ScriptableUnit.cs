using UnityEngine;

/// <summary>
/// Base class for all scriptable units in the game that can be created easily in inspector.
/// </summary>
[CreateAssetMenu(fileName = "New Unit", menuName = "Scriptable Unit")]
public class ScriptableUnit : ScriptableObject
{
    //Properties
    public Faction FactionType;
    public BaseUnit UnitPrefab;
}

public enum Faction
{
    HERO = 0,
    ENEMY = 1
}
