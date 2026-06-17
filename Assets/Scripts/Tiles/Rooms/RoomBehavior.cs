using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents selections of tiles that form a room, and contains any room-specific logic or properties.
/// </summary>
public class RoomBehavior : MonoBehaviour
{
    //Properties
    [Header("Room Settings")]
    public RoomType roomType;

    [Header("Generation Settings")]
    /// <summary>
    /// Shows the directions that are available for the next room to be generated in, based on the current room's position and the grid boundaries.
    /// </summary>
    public List<Direction> availableDirections;
    [Range(0,3)][SerializeField] private int numDoorsOut = 1;

    [Header("Tile Selections")]
    [HideInInspector] public GroundTile[] GroundTiles;
    public GroundTile UpConnectTile;
    public GroundTile DownConnectTile;
    public GroundTile LeftConnectTile;
    public GroundTile RightConnectTile;

    /// <summary>
    /// Records the direction from which the player entered the room.
    /// </summary>
    private Direction entryDirection;
    [HideInInspector] public List<Direction> NextDirections = new List<Direction>();
    [HideInInspector] public List<Direction> unavailableDirections = new List<Direction>();
    bool isActivated = false;


    //Enums
    public enum RoomType
    {
        HALLWAY = 0,
        BOSS = 1,
        TREASURE = 2,
        SPAWN = 3,
    }


    //Methods
    private void Awake()
    {
        GroundTiles = GetComponentsInChildren<GroundTile>(true);
    }

    public void ActivateRoom(Direction entry)
    {
        if (CheckIfValid())
        {
            entryDirection = entry;
            unavailableDirections.Add(entryDirection);
            isActivated = true;
            GetNextDirections();
        }
        else
        {
            Debug.LogError("Invalid room configuration. Please check the room settings and tile selections.");
        }
    }

    public GroundTile GetMistSpawnTile()
    {
        if (roomType != RoomType.SPAWN) { return null; }

        switch (entryDirection)
        {
            case Direction.UP:
                return UpConnectTile;
            case Direction.DOWN:
                return DownConnectTile;
            case Direction.LEFT:
                return LeftConnectTile;
            case Direction.RIGHT:
                return RightConnectTile;
            default:
                Debug.LogError("Invalid entry direction");
                return null;
        }
    }

    public List<GroundTile> GetHeroSpawnTiles()
    {
        if (roomType != RoomType.SPAWN) { return null; }
        List<GroundTile> spawnTiles = new List<GroundTile>();
        foreach (GroundTile tile in GroundTiles)
        {
            if (tile.Walkable)
            {
                spawnTiles.Add(tile);
            }
        }
        return spawnTiles;
    }

    private bool CheckIfValid()
    {
        if (GroundTiles == null) { return false; }
        if (availableDirections == null) { return false; }
        if (numDoorsOut > availableDirections.Count) { return false; }

        foreach (Direction direction in availableDirections)
        {
            bool isWorking = true;
            if (direction == Direction.UP && UpConnectTile == null) { isWorking = false; }
            if (direction == Direction.DOWN && DownConnectTile == null) { isWorking = false; }
            if (direction == Direction.LEFT && LeftConnectTile == null) { isWorking = false; }
            if (direction == Direction.RIGHT && RightConnectTile == null) { isWorking = false; }

            if (!isWorking)
            {
                Debug.LogError($"{direction} doesn't have a connecting tile for {this.name}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// For activated rooms, checks if there are any available directions
    /// </summary>
    /// <returns></returns>
    public bool CheckIfAnyDirectionsAvailable()
    {
        if (isActivated == false) { Debug.LogError("Room not activated yet"); return false; }
        bool isAvailableRoute = false;

        foreach (Direction direction in availableDirections)
        {
            if (direction != entryDirection && !unavailableDirections.Contains(direction))
            {
                isAvailableRoute = true;
            }
        }

        return isAvailableRoute;
    }

    private void GetNextDirections()
    {
        //Seeing if there are any directions to go in
        if (numDoorsOut == 0) { return; }

        //If there are only as many directions as there are doors out,
        //then we can just add all the available directions except for the entry direction.
        else if (availableDirections.Count - 1 == numDoorsOut)
        {
            foreach (Direction direction in availableDirections)
            {
                if (direction == entryDirection) { continue; }
                NextDirections.Add(direction);
            }
            return;
        }

        //If there are more directions than doors out, then we can randomly select from
        //the available directions until we have enough for the number of doors out.
        else if (availableDirections.Count - 1 > numDoorsOut)
        {
            while (NextDirections.Count < numDoorsOut)
            {
                int randIndex = Random.Range(0, availableDirections.Count);
                Direction randDirection = availableDirections[randIndex];
                if (randDirection != entryDirection && !NextDirections.Contains(randDirection))
                {
                    NextDirections.Add(randDirection);
                }
            }
        }
    }
}

public enum Direction
{
    UP = 0,
    DOWN = 1,
    LEFT = 2,
    RIGHT = 3
}