using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Represents the base class for all tile objects in the game.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public abstract class BaseTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    //Properties
    [Header("General")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected GameObject highlightGO;
    [SerializeField] protected GameObject rangeIndicatorGO;
    //[SerializeField] protected GameObject moveIndicatorGO;
    [SerializeField] protected bool isWalkable = true;
    public BaseUnit OccupiedUnit;


    [Header("Pathfinding")] //Using A* pathfinding
    [HideInInspector] public List<BaseTile> Neighbors { get; protected set; }
    [HideInInspector] public BaseTile Connection { get; private set; }
    [HideInInspector] public int G { get; private set; } // Cost from start node
    [HideInInspector] public int H { get; private set; } // Heuristic cost to end node
    [HideInInspector] public int F => G + H; // Total cost
    [HideInInspector] public ICoords Coords;
    private static readonly List<Vector2> Dirs = new List<Vector2>() {
            new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0),
        };




    //Methods
    #region Pathfinding
    public bool Walkable => isWalkable && OccupiedUnit == null;

    public float GetDistance(BaseTile other) => Coords.GetDistance(other.Coords);

    public virtual void Activate(ICoords coords)
    {
        Coords = coords;
    }

    public void SetUnit(BaseUnit unit)
    {
        //Setting the previous tile that the unit was on to have no occupied unit
        if (unit.OccupiedTile != null)
        {
            unit.OccupiedTile.OccupiedUnit = null;
        }

        unit.transform.position = transform.position;
        OccupiedUnit = unit;
    }

    public void SetConnection(BaseTile tile)
    {
        Connection = tile;
    }

    public void SetG(int g) => G = g;

    public void SetH(int h) => H = h;

    public void FindNeighbors(List<BaseTile> allTiles, int gridWidth, int gridHeight)
    {
        Neighbors = new List<BaseTile>();

        //Getting the 4 possible directions and checking if there's a tile there
        foreach (BaseTile tile in Dirs.Select(dir => GridManager.Instance.GetTileAtPosition(Coords.Pos + dir)).Where(t => t != null))
        {
            Neighbors.Add(tile);
        }
    }
    #endregion



    #region Pointer Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightGO.SetActive(true);

        CheckForArrowPath();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightGO.SetActive(false);
        LineManager.Instance.ClearLine();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Making sure it's the hero turn
        if (GameManager.Instance.GameState != GameState.HERO_TURN) { return; }

        if (OccupiedUnit != null)
        {
            //Seeing if clicking on a hero or enemy
            if (OccupiedUnit.FactionType == Faction.HERO)
            {
                UnitManager.Instance.ChangeSelectedHero(OccupiedUnit as BaseHero);
            }
            else if (OccupiedUnit.FactionType == Faction.ENEMY)
            {
                CheckIntentionForEnemy();
            }
        }

        if (Walkable)
        {
            //Making sure the selected hero isn't already moving
            if (UnitManager.Instance.SelectedHero.MoveState == BaseUnit.UnitState.MOVING) { return; }

            //Checking path to this tile and selecting it if valid
            UnitManager.Instance.SelectedHero.CheckPath(this);
        }
    }
    #endregion

    public void ShowInRange(bool show)
    {
        if (show)
        {
            rangeIndicatorGO.SetActive(true);
        }
        else
        {
            rangeIndicatorGO.SetActive(false);
        }
    }

    /// <summary>
    /// Seeing if the arrow path should be shown to this tile
    /// </summary>
    protected void CheckForArrowPath()
    {
        //Conditions for getting the arrow along the path to this tile
        if (GameManager.Instance.GameState != GameState.HERO_TURN) { return; }
        if (!Walkable) { return; }
        if (UnitManager.Instance.SelectedHero == null) { return; }
        if (UnitManager.Instance.SelectedHero.MoveState == BaseUnit.UnitState.MOVING) { return; }
        if (!UnitManager.Instance.SelectedHero.GetMovementRange().Contains(this)) { return; }

        BaseTile startTile = UnitManager.Instance.SelectedHero.OccupiedTile;
        List<BaseTile> path = UnitManager.Instance.SelectedHero.GetPath(startTile, this);
        if (path == null) { return; }

        //Getting the direction of the first tile in the path
        Vector2 firstDir = Vector2.zero;

        foreach (Vector2 dir in Dirs)
        {
            if (startTile.Coords.Pos + dir == path[0].Coords.Pos)
            {
                firstDir = dir;
            }
        }

        if (firstDir == Vector2.zero) { return; }

        LineManager.Instance.DrawLine(path, startTile, firstDir);
    }

    protected void CheckIntentionForEnemy()
    {
        //TODO: Implement enemy selection, targetting, and attacking
    }
}

public struct ICoords
{
    public float GetDistance(ICoords other)
    {
        var dist = new Vector2Int(Mathf.Abs((int)Pos.x - (int)other.Pos.x), Mathf.Abs((int)Pos.y - (int)other.Pos.y));

        var lowest = Mathf.Min(dist.x, dist.y);
        var highest = Mathf.Max(dist.x, dist.y);

        var horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }
    public Vector2 Pos { get; set; }
}
