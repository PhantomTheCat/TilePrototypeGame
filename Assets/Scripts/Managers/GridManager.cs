using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Class that manages the grid generation and tile spawning.
/// </summary>
public class GridManager : MonoBehaviour
{
    //Properties
    public static GridManager Instance;

    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private int amountOfRooms = 50;
    [Range(0.01f, 1f)]
    [SerializeField] private float chanceOfRoom = .10f;
    [Tooltip("Defines what layers the game can't see through")]
    [SerializeField] private LayerMask visionMask;

    [Header("Tile Prefabs")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject wallTilePrefab;
    [SerializeField] private GameObject startingPlatformPrefab;
    [SerializeField] private GameObject lastRoomPrefab;
    [SerializeField] private GameObject[] hallwayPrefabs;
    [SerializeField] private GameObject[] roomPrefabs;


    private Dictionary<Vector2Int, BaseTile> tiles = new Dictionary<Vector2Int, BaseTile>();
    private Dictionary<Vector2Int, WallTile> wallTiles = new Dictionary<Vector2Int, WallTile>();
    private List<RoomBehavior> rooms = new List<RoomBehavior>();
    private RoomBehavior lastRoom;
    private RoomBehavior currentRoom;
    private List<GroundTile> playerStartingTiles;
    private List<BaseTile> tilesHighlighted = new List<BaseTile>();


    //Methods
    private void Awake()
    {
        Instance = this;
    }

    public BaseTile GetHeroSpawnTile()
    {
        if (playerStartingTiles == null || playerStartingTiles.Count == 0)
        {
            Debug.LogError("No player starting tiles found.");
            return null;
        }

        //Getting a random ground tile on the starting platform to spawn the hero on
        return playerStartingTiles.Where(t => t.Walkable).OrderBy(t => Random.value).First();
    }

    public BaseTile GetEnemySpawnTile()
    {
        //TODO: Implement enemy spawn tile selection based on the current room and difficulty
        return null;
    }

    public BaseTile GetTileAtPosition(Vector2Int pos) => tiles.TryGetValue(pos, out BaseTile tile) ? tile : null;



    #region Generation
    /// <summary>
    /// Generates the grid of tiles based on the specified width and height.
    /// </summary>
    public void GenerateGrid()
    {
        //Getting some organization GameObjects to hold the tiles and hallways
        GameObject gridParent = new GameObject("Grid");
        GameObject hallwayParent = new GameObject("Hallways");
        GameObject roomsParent = new GameObject("Rooms");
        GameObject lastRoomParent = new GameObject("Last Room");
        GameObject startRoomParent = new GameObject("Starting Room");
        gridParent.transform.parent = transform;
        hallwayParent.transform.parent = gridParent.transform;
        roomsParent.transform.parent = gridParent.transform;
        lastRoomParent.transform.parent = gridParent.transform;
        startRoomParent.transform.parent = gridParent.transform;

        //Spawning the starting room
        GameObject startRoom = Instantiate(startingPlatformPrefab, Vector3.zero, Quaternion.identity, startRoomParent.transform);
        RoomBehavior startRoomBehavior = startRoom.GetComponent<RoomBehavior>();
        startRoomBehavior.ActivateRoom(Direction.DOWN);

        //Getting spawn tiles for mist and player for later
        GroundTile mistStartTile = startRoomBehavior.GetMistSpawnTile();
        playerStartingTiles = startRoomBehavior.GetHeroSpawnTiles();
        AddToDictionary(startRoomBehavior);
        lastRoom = startRoomBehavior;

        int index = 0;
        chanceOfRoom *= 100;

        if (startRoomBehavior.NextDirections.Count == 0)
        {
            Debug.LogError("Starting room does not have any available directions to connect.");
            return;
        }

        if (roomPrefabs == null || hallwayPrefabs == null) return;

        while (index < amountOfRooms)
        {
            float wheel = Random.Range(1f, 100f);
            GameObject roomPrefab = null;

            if (wheel > chanceOfRoom)
            {
                roomPrefab = hallwayPrefabs[Random.Range(0, hallwayPrefabs.Length)];
            }
            else
            {
                roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            }

            //Randomly getting a new room to add based on if it can connect to last room
            RoomBehavior room = roomPrefab.GetComponent<RoomBehavior>();
            bool haveConnection = false;

            //Seeing if the last room has any available directions to connect to, if not, we need to backtrack to a previous room that does have available directions
            if (!lastRoom.CheckIfAnyDirectionsAvailable())
            {
                RoomBehavior[] previousRooms = FindObjectsByType<RoomBehavior>();

                if (previousRooms.Length == 0)
                {
                    Debug.LogError("No previous rooms found to backtrack to.");
                    index += amountOfRooms; //Ending the loop since we can't add any more rooms
                    return;
                }

                for (int i = 0; i < previousRooms.Length; i++)
                {
                    RoomBehavior prevRoom = previousRooms[i];
                    if (prevRoom.CheckIfAnyDirectionsAvailable())
                    {
                        lastRoom = prevRoom;
                        break;
                    }
                }
            }

            //Seeing which directions the last room can connect
            foreach (Direction dir in lastRoom.NextDirections)
            {
                if (haveConnection) { break; }
                if (lastRoom.unavailableDirections.Contains(dir)) { continue; }

                //Direction means the direction it is leaving from the last room
                switch (dir)
                {
                    case Direction.UP:
                        if (room.availableDirections.Contains(Direction.DOWN)) { haveConnection = true; }
                        break;
                    case Direction.DOWN:
                        if (room.availableDirections.Contains(Direction.UP)) { haveConnection = true; }
                        break;
                    case Direction.LEFT:
                        if (room.availableDirections.Contains(Direction.RIGHT)) { haveConnection = true; }
                        break;
                    case Direction.RIGHT:
                        if (room.availableDirections.Contains(Direction.LEFT)) { haveConnection = true; }
                        break;
                }

                if (haveConnection)
                {
                    bool wasSpawned = SpawnRoom(roomPrefab, dir, hallwayParent.transform);

                    if (wasSpawned)
                    {
                        index++;
                    }
                }
            }
        }

        //Setting neighbors for pathfinding
        foreach (BaseTile tile in tiles.Values)
        {
            tile.FindNeighbors();
        }

        //Want to spawn the walls around the ground tiles
        SpawnWalls();

        //Activate where the mist is before spawning heroes
        //MistManager.Instance.ActivateMist(new List<GroundTile> { mistStartTile });

        //Notifying the GameManager that the grid has been generated
        GameManager.Instance.ChangeState(GameState.SPAWN_HEROES);
    }

    /// <summary>
    /// Spawning the room based on the prefab and connecting it to previous room based on the entry direction. 
    /// If there is no previous room, it will just spawn the room at the origin.
    /// </summary>
    /// <param name="roomPrefab"></param>
    /// <param name="prevRoom"></param>
    /// <param name="dir"></param>
    /// <param name="parent"></param>
    private bool SpawnRoom(GameObject roomPrefab, Direction dir, Transform parent)
    {
        GameObject roomGO = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, parent);

        if (lastRoom == null) { Destroy(roomGO); return false; }

        currentRoom = roomGO.GetComponent<RoomBehavior>();
        if (currentRoom == null) 
        {
            Debug.LogError("Room prefab does not have a RoomBehavior component.");
            Destroy(roomGO);
            return false; 
        }

        //Getting the tiles to be connected
        GroundTile lastRoomConnectTile = null;
        GroundTile newRoomConnectTile = null;

        //Direction means the direction it is leaving from the last room
        switch (dir)
        {
            case Direction.UP:
                lastRoomConnectTile = lastRoom.UpConnectTile;
                newRoomConnectTile = currentRoom.DownConnectTile;
                break;
            case Direction.DOWN:
                lastRoomConnectTile = lastRoom.DownConnectTile;
                newRoomConnectTile = currentRoom.UpConnectTile;
                break;
            case Direction.LEFT:
                lastRoomConnectTile = lastRoom.LeftConnectTile;
                newRoomConnectTile = currentRoom.RightConnectTile;
                break;
            case Direction.RIGHT:
                lastRoomConnectTile = lastRoom.RightConnectTile;
                newRoomConnectTile = currentRoom.LeftConnectTile;
                break;
        }

        ConnectRooms(newRoomConnectTile, lastRoomConnectTile, dir);

        //Making sure the room doesn't overlap with any existing rooms
        foreach (BaseTile tile in currentRoom.GroundTiles)
        {
            int x = (int)Mathf.Round(tile.transform.position.x);
            int y = (int)Mathf.Round(tile.transform.position.y);
            Vector2Int tilePos = new Vector2Int(x, y);
            if (tiles.ContainsKey(tilePos))
            {
                //If there is already a tile at this position, we need to destroy the room and return false
                Destroy(roomGO);
                lastRoom.unavailableDirections.Add(dir);
                return false;
            }
        }

        //Adding the new room's tiles to the dictionary if it got through the overlap check
        AddToDictionary(currentRoom);

        //Updating the last room to be the current room for the next iteration
        lastRoom = currentRoom;
        return true;
    }

    private void AddToDictionary(RoomBehavior room)
    {
        rooms.Add(room);

        foreach (BaseTile tile in room.GroundTiles)
        {
            int x = (int)Mathf.Round(tile.transform.position.x);
            int y = (int)Mathf.Round(tile.transform.position.y);
            Vector2Int tilePos = new Vector2Int(x, y);
            ICoords coords = new ICoords() { Pos = tilePos };
            tile.Activate(coords);
            tiles[tilePos] = tile;
        }
    }

    /// <summary>
    /// Connecting the new room to the last room by aligning the connecting 
    /// tiles and then adjusting the position of the new room based on that connection.
    /// </summary>
    /// <param name="newConnectTile"></param>
    /// <param name="lastConnectTile"></param>
    /// <param name="dir"></param>
    private void ConnectRooms(GroundTile newConnectTile, GroundTile lastConnectTile, Direction dir)
    {
        //Getting the distance from the center of the new room to the connecting tile in the new room
        newConnectTile.transform.parent = null;
        Vector3 positionOffset = currentRoom.transform.position - newConnectTile.transform.position;

        //Direction means the direction it is leaving from the last room
        switch (dir)
        {
            case Direction.UP:
                newConnectTile.transform.position = lastConnectTile.transform.position + new Vector3(0, tileSize, 0);
                currentRoom.ActivateRoom(Direction.DOWN);
                break;
            case Direction.DOWN:
                newConnectTile.transform.position = lastConnectTile.transform.position + new Vector3(0, -tileSize, 0);
                currentRoom.ActivateRoom(Direction.UP);
                break;
            case Direction.LEFT:
                newConnectTile.transform.position = lastConnectTile.transform.position + new Vector3(-tileSize, 0, 0);
                currentRoom.ActivateRoom(Direction.RIGHT);
                break;
            case Direction.RIGHT:
                newConnectTile.transform.position = lastConnectTile.transform.position + new Vector3(tileSize, 0, 0);
                currentRoom.ActivateRoom(Direction.LEFT);
                break;
        }

        currentRoom.transform.position = newConnectTile.transform.position + positionOffset;
        newConnectTile.transform.parent = currentRoom.transform;
    }

    /// <summary>
    /// Spawning the walls around all the ground tiles,
    /// while not placing them ontop of any existing tiles.
    /// </summary>
    private void SpawnWalls()
    {
        GameObject wallParent = new GameObject("Walls");
        wallParent.transform.parent = transform;

        List<Vector2> tilePositions = new List<Vector2>();

        for (int i = 0; i < tiles.Count; i++)
        {
            BaseTile tile = GetTileAtPosition(tiles.ElementAt(i).Key);
            if (tile == null) { continue; }

            if (tile is GroundTile)
            {
                foreach (Vector2Int dir in BaseTile.Dirs)
                {
                    Vector2Int neighborPos = tile.Coords.Pos + dir;
                    neighborPos = new Vector2Int(neighborPos.x, neighborPos.y);

                    if (GetTileAtPosition(neighborPos) == null && !tilePositions.Contains(neighborPos))
                    {
                        tilePositions.Add(neighborPos);
                    }
                }
            }
        }

        foreach (Vector2 pos in tilePositions)
        {
            SpawnTile(wallTilePrefab, (int)pos.x, (int)pos.y, wallParent);
        }
    }

    private void SpawnTile(GameObject tilePrefab, int x, int y, GameObject parent)
    {
        //Instantiating the tile
        Vector3 position = new Vector3(x, y);
        GameObject spawnedTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
        spawnedTile.name = $"Tile {x} {y}";
        spawnedTile.transform.parent = parent.transform;

        //Activating the tile
        ICoords coords = new ICoords() { Pos = new Vector2Int(x, y) };
        spawnedTile.GetComponent<BaseTile>().Activate(coords);

        //Adding the tile to the dictionary
        tiles[new Vector2Int(x, y)] = spawnedTile.GetComponent<BaseTile>();

        if (spawnedTile.GetComponent<WallTile>() != null)
        {
            wallTiles[new Vector2Int(x, y)] = spawnedTile.GetComponent<WallTile>();
        }
    }
    #endregion


    #region Sight and Highlighting
    /// <summary>
    /// Highlights all tiles that are walkable for the SelectedHero and within its movement range.
    /// </summary>
    public void HighlightHeroTiles()
    {
        BaseHero selectedHero = UnitManager.Instance.SelectedHero;
        if (selectedHero == null) { return; }
        ClearAllHighlights();

        List<BaseTile> heroesTiles = selectedHero.GetMovementRange();
        foreach (BaseTile tile in heroesTiles)
        {
            if (tile.gameObject.activeInHierarchy)
            {
                HighlightTile(tile);
            }
        }
    }

    public void HighlightActionTiles(BaseAction action)
    {
        BaseHero selectedHero = UnitManager.Instance.SelectedHero;
        if (selectedHero == null) { return; }
        ClearAllHighlights();

        List<BaseTile> actionTiles = action.GetTargetTiles(selectedHero);

        foreach (BaseTile tile in actionTiles)
        {
            if (tile.gameObject.activeInHierarchy) 
            {
                HighlightTile(tile);
            }
        }
    }

    public void HighlightTile(BaseTile tile)
    {
        tilesHighlighted.Add(tile);
        tile.ShowInRange(true);
    }

    public void ClearAllHighlights()
    {
        foreach (BaseTile tile in tilesHighlighted)
        {
            tile.ShowInRange(false);
        }
        tilesHighlighted.Clear();
    }

    /// <summary>
    /// Gets a list of valid tiles for the specified unit and range
    /// </summary>
    /// <param name="user">Center point from</param>
    /// <param name="range">Max number of tiles to go out to</param>
    /// <param name="withWalls">Want the list with or without walls</param>
    /// <returns></returns>
    public List<BaseTile> GetValidTiles(BaseUnit user, int range, bool withWalls, bool withUnits, bool withUser)
    {
        List<BaseTile> validTiles = new List<BaseTile>();

        if (user.OccupiedTile == null) { return validTiles; }
        if (withUser) { validTiles.Add(user.OccupiedTile); }

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int tilePos = new Vector2Int(user.OccupiedTile.Coords.Pos.x + x, user.OccupiedTile.Coords.Pos.y + y);
                BaseTile tile = GetTileAtPosition(tilePos);

                if (tile != null && !validTiles.Contains(tile))
                {
                    if (withWalls)
                    {
                        if (tile.Walkable || tile is WallTile)
                        {
                            validTiles.Add(tile);
                        }
                    }
                    else if (tile.Walkable || tile.OccupiedChest != null)
                    {
                        validTiles.Add(tile);
                    }
                    else if (tile.OccupiedUnit != null && withUnits)
                    {
                        if (!withUser && tile == user.OccupiedTile)
                        {
                            continue;
                        }
                        validTiles.Add(tile);
                    }
                }
            }
        }

        return validTiles;
    }

    public void UpdateVision()
    {
        //Set all rooms active for sighting
        foreach (RoomBehavior room in rooms)
        {
            room.gameObject.SetActive(true);
        }

        //Get all the visible tiles
        List<BaseTile> visibleTiles = GetTilesInSight(UnitManager.Instance.Heroes);

        foreach (BaseTile tile in tiles.Values)
        {
            if (visibleTiles.Contains(tile))
            {
                tile.gameObject.SetActive(true);
                tile.IsVisible = true;
            }
            else            
            {
                tile.gameObject.SetActive(false);
                tile.IsVisible = false;
            }
        }

        //For efficiency, set all rooms with non-visible tiles to be inactive till we update vision
        foreach (RoomBehavior room in rooms)
        {
            if (room.CheckIfVisible())
            {
                room.gameObject.SetActive(true);
            }
            else
            {
                room.gameObject.SetActive(false);
            }
        }
    }

    public List<BaseTile> GetTilesInSight(List<BaseHero> heroes)
    {
        List<BaseTile> validTiles = new List<BaseTile>();

        //Set all wall tiles active to avoid raycasts shooting through them
        foreach (WallTile wallTile in wallTiles.Values)
        {
            wallTile.gameObject.SetActive(true);
        }

        foreach (BaseHero hero in heroes)
        {
            List<BaseTile> checkTiles = GetValidTiles(hero, hero.ViewRange, false, false, true);

            //Want to only check tiles within a certain distance from the hero to optimize performance
            foreach (BaseTile tile in checkTiles)
            {
                float distance = Vector3.Distance(hero.transform.position, tile.transform.position);
                Vector3 direction = (tile.transform.position - hero.transform.position);

                //Sending a raycast hit from the hero to the tile to see if there is a clear line of sight to the tile
                Vector3 origin = hero.OccupiedTile.transform.position;

                RaycastHit hitReg;
                Physics.Raycast(origin, direction, out hitReg, distance, visionMask);

                List<RaycastHit> hits = new List<RaycastHit> { hitReg };

                //Firing from the neighbors of the tile to make sure we can see around corners
                foreach (Vector2Int dir in BaseTile.Dirs)
                {
                    Vector2Int neighborPos = hero.OccupiedTile.Coords.Pos + dir;
                    BaseTile neighborTile = GetTileAtPosition(neighborPos);
                    if (neighborTile == null || !neighborTile.Walkable) { continue; }

                    float neighborDistance = Vector3.Distance(neighborTile.transform.position, tile.transform.position);
                    Vector3 neighborDirection = (tile.transform.position - neighborTile.transform.position);
                    RaycastHit neighborHit;
                    Physics.Raycast(neighborTile.transform.position, neighborDirection, out neighborHit, neighborDistance, visionMask);

                    hits.Add(neighborHit);
                }

                foreach (RaycastHit hit in hits)
                {
                    if (validTiles.Contains(tile)) { continue; }

                    //See if there is a clear line of sight to the tile (Meaning the raycast doesn't hit anything)
                    //Applying only to ground tiles
                    if (hit.collider == null)
                    {
                        validTiles.Add(tile);
                        continue;
                    }
                }
            }
        }

        //Getting wall tiles in sight
        for (int i = 0; i < validTiles.Count; i++)
        {
            BaseTile tile = validTiles[i];
            if (tile == null) continue;
            if (tile is WallTile) continue;
            foreach (Vector2Int dir in BaseTile.Dirs)
            {
                Vector2Int neighborPos = tile.Coords.Pos + dir;
                BaseTile neighbor = GetTileAtPosition(neighborPos);
                if (neighbor == null) continue;
                if (!validTiles.Contains(neighbor) && neighbor is WallTile)
                {
                    validTiles.Add(neighbor);
                }
            }
        }

        return validTiles;
    }
    #endregion

    #region Areas
    public List<BaseTile> GetCircleArea(BaseUnit unit, BaseTile center, int size)
    {
        List<BaseTile> validTiles = new List<BaseTile>();
        validTiles.Add(center);

        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                Vector2Int tilePos = new Vector2Int(center.Coords.Pos.x + x, center.Coords.Pos.y + y);
                BaseTile tile = GetTileAtPosition(tilePos);

                if (tile == null) continue;
                if (tile is WallTile) continue;
                if (validTiles.Contains(tile)) continue;

                //Get distance measure
                float distance = center.GetDistance(tile);
                if (distance <= size * 10)
                {
                    validTiles.Add(tile);
                }
            }
        }
        
        return validTiles;
    }

    public List<BaseTile> GetRectangleArea(BaseUnit unit, BaseTile center, int horSize, int vertSize)
    {
        List<BaseTile> validTiles = new List<BaseTile>();
        validTiles.Add(center);

        for (int x = -horSize; x <= horSize; x++)
        {
            for (int y = -vertSize; y <= vertSize; y++)
            {
                Vector2Int tilePos = new Vector2Int(center.Coords.Pos.x + x, center.Coords.Pos.y + y);
                BaseTile tile = GetTileAtPosition(tilePos);

                if (tile is WallTile) continue;
                if (validTiles.Contains(tile)) continue;
                validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    public List<BaseTile> GetConeArea(BaseTile userTile, BaseTile start, int horSize, int vertSize)
    {
        List<BaseTile> validTiles = new List<BaseTile>();
        validTiles.Add(start);

        //Get the direction for the cone based on where the start tile is to the user's tile
        Vector2Int direction = start.Coords.Pos - userTile.Coords.Pos;

        Vector2Int negaTilePos = new Vector2Int(0, 0);
        Vector2Int posiTilePos = new Vector2Int(0, 0);

        if (direction == Vector2.up || direction == new Vector2(1, 1) || direction == new Vector2(-1, 1))
        {
            negaTilePos = new Vector2Int(start.Coords.Pos.x + -horSize, start.Coords.Pos.y + vertSize);
            posiTilePos = new Vector2Int(start.Coords.Pos.x + horSize, start.Coords.Pos.y + vertSize);
        }
        else if (direction == Vector2.down || direction == new Vector2(1, -1) || direction == new Vector2(-1, -1))
        {
            negaTilePos = new Vector2Int(start.Coords.Pos.x + -horSize, start.Coords.Pos.y + -vertSize);
            posiTilePos = new Vector2Int(start.Coords.Pos.x + horSize, start.Coords.Pos.y + -vertSize);
        }
        else if (direction == Vector2.right)
        {
            negaTilePos = new Vector2Int(start.Coords.Pos.x + horSize, start.Coords.Pos.y + -vertSize);
            posiTilePos = new Vector2Int(start.Coords.Pos.x + horSize, start.Coords.Pos.y + vertSize);
        }
        else if (direction == Vector2.left)
        {
            negaTilePos = new Vector2Int(start.Coords.Pos.x + -horSize, start.Coords.Pos.y + -vertSize);
            posiTilePos = new Vector2Int(start.Coords.Pos.x + -horSize, start.Coords.Pos.y + vertSize);
        }
        else return validTiles;

        //Get all tiles inside this area
        List<BaseTile> insideConeTiles = GetTilesInsideVertices(start.Coords.Pos, negaTilePos, posiTilePos);
        if (insideConeTiles != null && insideConeTiles.Count > 0) validTiles.AddRange(insideConeTiles);

        return validTiles;
    }

    private List<BaseTile> GetTilesInsideVertices(Vector2Int start, Vector2Int vertice1, Vector2Int vertice2)
    {
        List<BaseTile> validTiles = new List<BaseTile>();

        //Get the extremes, so we can get a list of BaseTiles
        Vector2Int[] vertexs = new Vector2Int[3] {start, vertice1, vertice2};
        int maxHor = 0;
        int minHor = 0;
        int maxVer = 0;
        int minVer = 0;

        for (int i = 0; i < vertexs.Length; i++)
        {
            Vector2Int vertice = vertexs[i];
            if (i == 0)
            {
                maxHor = vertice.x;
                minHor = vertice.x;
                maxVer = vertice.y;
                minVer = vertice.y;
                continue;
            }

            //See if above or below horizontal values (new extreme)
            if (vertice.x > maxHor)
            {
                maxHor = vertice.x;
            }
            else if (vertice.x < minHor)
            {
                minHor = vertice.x;
            }
            //See if above or below vertical values (new extreme)
            if (vertice.y  > maxVer)
            {
                maxVer = vertice.y;
            }
            else if (vertice.y < minVer)
            {
                minVer = vertice.y;
            }
        }

        Vector2[] angles = new Vector2[4] 
        { 
            new Vector2(0, tileSize * 0.5f), 
            new Vector2(0, -tileSize * 0.5f), 
            new Vector2(-tileSize * 0.5f, 0), 
            new Vector2(tileSize * 0.5f, 0) 
        };

        //Get a list of all the BaseTiles that are within the max ranges
        for (int x = minHor; x <= maxHor; x++)
        {
            for (int y = minVer; y <= maxVer; y++)
            {
                //See if in cone
                Vector2Int tilePos = new Vector2Int(x, y);
                bool isInCone = false;

                foreach (Vector2 angle in angles)
                {
                    Vector2 checkPos = new Vector2(tilePos.x + angle.x, tilePos.y + angle.y);
                    bool inCone = CheckIfInCone(checkPos, start, vertice1, vertice2);
                    if (inCone)
                    {
                        isInCone = true;
                        break;
                    }
                }
                if (!isInCone) continue;

                //Final checks, then add tile to valid tiles
                BaseTile tile = GetTileAtPosition(tilePos);
                if (tile == null) continue;
                if (tile is WallTile) continue;
                if (validTiles.Contains(tile)) continue;
                validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    public bool CheckIfInCone(Vector2 point, Vector2 A, Vector2 B, Vector2 C)
    {
        //Using a Barycentric Coordinate System here, where we input the formula,
        //and if the point is inside the triangle, then a, b, and c should all be between or equal to 0 and 1
        float denominator = ((B.y - C.y) * (A.x - C.x) + (C.x - B.x) * (A.y - C.y));
        float a = ((B.y - C.y) * (point.x - C.x) + (C.x - B.x) * (point.y - C.y)) / denominator;
        float b = ((C.y - A.y) * (point.x - C.x) + (A.x - C.x) * (point.y - C.y)) / denominator;
        float c = 1f - a - b;

        return 0 <= a && a <= 1 && 0 <= b && b <= 1 && 0 <= c && c <= 1;
    }

    #endregion
}
