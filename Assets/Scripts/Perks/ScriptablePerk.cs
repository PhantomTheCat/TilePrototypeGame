using UnityEngine;

/// <summary>
/// Base class for all scriptable actions in the game that can be created easily in inspector.
/// </summary>
[CreateAssetMenu(fileName = "New Scriptable Perk", menuName = "Scriptable Perk")]
public class ScriptablePerk : ScriptableObject
{
    //Properties
    public BasePerk Perk;
}