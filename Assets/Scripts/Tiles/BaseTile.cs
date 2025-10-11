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
    [SerializeField] protected bool isWalkable = true;
    public BaseUnit OccupiedUnit;


    [Header("Pathfinding")] //Using A* pathfinding
    public List<BaseTile> Neighbors { get; protected set; }
    public BaseTile Connection { get; private set; }
    public int G { get; private set; } // Cost from start node
    public int H { get; private set; } // Heuristic cost to end node
    public int F => G + H; // Total cost
    public ICoords Coords;
    private static readonly List<Vector2> Dirs = new List<Vector2>() {
            new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0),
        };




    //Methods
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightGO.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightGO.SetActive(false);
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
