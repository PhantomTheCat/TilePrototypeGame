using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;

public class SummonAction : BaseAction
{
    //Properties
    [Header("Summons")]
    [SerializeField] protected BaseUnit[] summonPrefabs;

    //Methods
    private void Awake()
    {
        if (summonPrefabs == null || summonPrefabs.Length == 0)
        {
            Debug.LogWarning($"{ActionName} has no prefabs to summon!");
        }
    }

    public override void Execute(BaseUnit user, List<BaseTile> targetTiles)
    {
        base.Execute(user, targetTiles);
        if (targetTiles == null || user == null || summonPrefabs == null) return;

        for(int i = 0; i < targetTiles.Count; i++)
        {
            BaseTile tile = targetTiles[i];
            if (tile == null) continue;
            if (!tile.Walkable) continue;

            int index = Random.Range(0, summonPrefabs.Length);
            SummonUnit(summonPrefabs[index], tile);
        }
    }

    protected virtual void SummonUnit(BaseUnit unit, BaseTile spawnTile)
    {
        if (unit == null || spawnTile == null) return;
        if (!spawnTile.Walkable) return;
        if (UnitManager.Instance.IsOnEnemyPath(spawnTile)) return;

        BaseUnit newUnit = Instantiate(unit);
        if (newUnit.FactionType == Faction.ENEMY)
        {
            UnitManager.Instance.Enemies.Add(newUnit as BaseEnemy);
        }
        newUnit.Activate(spawnTile);
    }
}