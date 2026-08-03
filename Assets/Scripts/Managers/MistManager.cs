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

    public void ActivateMist(GroundTile startingTile)
    {
        isMistActive = true;
        startingTile.ToggleMist(true);
        mistCoveredTiles.Add(startingTile);
    }

    public void SpreadMist()
    {
        if (!isMistActive) { return; }
        for (int i = 0; i < mistSpreadRate; i++)
        {
             SpreadMistOnce();
        }
    }

    private void SpreadMistOnce()
    {
        List<GroundTile> newMistTiles = new List<GroundTile>();

        foreach (GroundTile tile in mistCoveredTiles)
        {
            foreach (GroundTile neighbor in tile.Neighbors)
            {
                if (neighbor.HasMist) continue;
                if (newMistTiles.Contains(neighbor)) continue;

                newMistTiles.Add(neighbor);
                neighbor.ToggleMist(true);
            }
        }
        mistCoveredTiles.AddRange(newMistTiles);
    }
}
