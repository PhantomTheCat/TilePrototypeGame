using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Class that manages the grid generation and tile spawning.
/// </summary>
public class GridManager : MonoBehaviour
{
    //Properties
    public static GridManager Instance;

    [Header("Grid Settings")]
    [SerializeField] private int width = 0;
    [SerializeField] private int height = 0;
    [SerializeField] private Transform mainCamera;

    [Header("Tile Prefabs")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject wallTilePrefab;

    private Dictionary<Vector2, BaseTile> tiles = new Dictionary<Vector2, BaseTile>();


    //Methods
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (mainCamera == null) { mainCamera = Camera.main.transform; }
    }

    /// <summary>
    /// Generates the grid of tiles based on the specified width and height.
    /// </summary>
    public void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            //Making an empty GameObject to hold the column of tiles
            GameObject columnGO = new GameObject($"Column {x}");
            columnGO.transform.parent = transform;

            for (int y = 0; y < height; y++)
            {
                //Randomly choosing if new tile is a wall or ground tile
                int randomTile = Random.Range(0, 10);
                if (randomTile <= 1)
                {
                    //Making a wall tile
                    SpawnTile(wallTilePrefab, x, y, columnGO);
                }
                else
                {
                    //Making a ground tile
                    SpawnTile(groundTilePrefab, x, y, columnGO);
                }
            }
        }

        //Positioning the camera to be in the center of the grid
        mainCamera.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);

        //Setting neighbors for pathfinding
        foreach (BaseTile tile in tiles.Values)
        {
            tile.FindNeighbors(tiles.Values.ToList(), width, height);
        }

        //Notifying the GameManager that the grid has been generated
        GameManager.Instance.ChangeState(GameState.SPAWN_HEROES);
    }

    public BaseTile GetHeroSpawnTile()
    {
        //Getting a random ground tile from the grid on right side
        return tiles.Where(t => t.Key.x < width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public BaseTile GetEnemySpawnTile()
    {
        //Getting a random ground tile from the grid on left side
        return tiles.Where(t => t.Key.x > width / 2 && t.Value.Walkable).OrderBy(t => Random.value).First().Value;
    }

    public BaseTile GetTileAtPosition(Vector2 pos) => tiles.TryGetValue(pos, out BaseTile tile) ? tile : null;

    private void SpawnTile(GameObject tilePrefab, int x, int y, GameObject parent)
    {
        //Instantiating the tile
        Vector3 position = new Vector3(x, y);
        GameObject spawnedTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
        spawnedTile.name = $"Tile {x} {y}";
        spawnedTile.transform.parent = parent.transform;

        //Activating the tile
        ICoords coords = new ICoords() { Pos = new Vector2(x, y) };
        spawnedTile.GetComponent<BaseTile>().Activate(coords);

        //Adding the tile to the dictionary
        tiles[new Vector2(x, y)] = spawnedTile.GetComponent<BaseTile>();
    }

    /// <summary>
    /// Highlights all tiles that are walkable for the SelectedHero and within its movement range.
    /// </summary>
    public void HighlightHeroTiles()
    {
        if (UnitManager.Instance.SelectedHero == null) { return; }

        ClearAllHighlights();
        List<BaseTile> heroesTiles = UnitManager.Instance.SelectedHero.GetMovementRange();
        foreach (BaseTile tile in heroesTiles)
        {
            tile.ShowInRange(true);
        }
    }

    public void ClearAllHighlights()
    {
        foreach (BaseTile tile in tiles.Values)
        {
            tile.ShowInRange(false);
        }
    }
}
