using UnityEngine;
using System.Collections.Generic;

public class MistManager : MonoBehaviour
{
    //Properties
    public static MistManager Instance;
    private bool isMistActive = false;
    [HideInInspector] public List<GroundTile> mistCoveredTiles = new List<GroundTile>();

    [Header("Mist Settings")]
    [Range(1,5)][SerializeField] private int mistSpreadRate = 1; // Number of tiles the mist spreads to per turn


    //Methods
    private void Awake()
    {
        Instance = this;
    }

    public void ActivateMist(List<GroundTile> startingTiles)
    {
        isMistActive = true;
        foreach (GroundTile tile in startingTiles)
        {
            tile.ToggleMist(true);
            mistCoveredTiles.Add(tile);
        }
    }

    public void SpreadMist()
    {
        if (!isMistActive) { return; }
        List<GroundTile> newMistTiles = new List<GroundTile>();
        for (int i = 0; i < mistSpreadRate; i++)
        {
             SpreadMistOnce(newMistTiles);
        }
    }

    private void SpreadMistOnce(List<GroundTile> newMistTiles)
    {
        foreach (GroundTile tile in mistCoveredTiles)
        {
            // Get adjacent tiles (this method needs to be implemented based on your tile system)
            List<GroundTile> adjacentTiles = GetAdjacentTiles(tile);
            foreach (GroundTile adjacent in adjacentTiles)
            {
                if (!adjacent.HasMist)
                {
                    adjacent.ToggleMist(true);
                    newMistTiles.Add(adjacent);
                }
            }
        }
        mistCoveredTiles.AddRange(newMistTiles);
    }

    private List<GroundTile> GetAdjacentTiles(GroundTile tile)
    {
        List<GroundTile> adjacentTiles = new List<GroundTile>();

        foreach (Vector2Int dir in BaseTile.Dirs)
        {
            Vector2Int adjacentCoords = tile.Coords.Pos + dir;
            BaseTile neighbor = GridManager.Instance.GetTileAtPosition(adjacentCoords);

            if (neighbor is not GroundTile) { continue; }
            GroundTile adjacentGroundTile = neighbor as GroundTile;
            if (adjacentGroundTile.HasMist) { continue; }
            adjacentTiles.Add(adjacentGroundTile);
        }

        return adjacentTiles;
    }
}
