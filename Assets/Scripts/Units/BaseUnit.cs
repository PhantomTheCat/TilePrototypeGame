using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for all units in the game.
/// </summary>
public class BaseUnit : MonoBehaviour
{
    //Properties
    [Header("General")]
    public Faction FactionType;
    public string UnitName;
    public UnitState MoveState = UnitState.IDLE;

    [Header("Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth = 100;

    [Header("Pathfinding")]
    public BaseTile OccupiedTile;
    [SerializeField] protected float moveSpeed = 5f;
    private List<BaseTile> currentPath;
    private int currentPathIndex = 0;


    //Methods
    protected virtual void Update()
    {
        //If we are moving, move towards the target tile
        if (MoveState == UnitState.MOVING && currentPath != null && currentPathIndex < currentPath.Count)
        {
            BaseTile targetTile = currentPath[currentPathIndex];
            Vector3 targetPosition = targetTile.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;

            transform.Translate((direction * moveSpeed) * Time.deltaTime);

            //If we reached the target tile, move to the next tile in the path
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                //Setting the occupied tile to the new tile
                OccupiedTile.OccupiedUnit = null;
                OccupiedTile = targetTile;
                targetTile.SetUnit(this);

                currentPathIndex++;

                if (currentPathIndex >= currentPath.Count)
                {
                    //Reached the end of the path
                    MoveState = UnitState.IDLE;
                    currentPath = null;
                    currentPathIndex = 0;
                }
            }
        }
    }

    public virtual void Activate(BaseTile spawnTile)
    {
        OccupiedTile = spawnTile;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public virtual void Die()
    {
        //TODO: Implement death logic here
    }

    public virtual void CheckPath(BaseTile targetTile)
    {
        //Making sure the target tile is walkable
        if (!targetTile.Walkable)
        {
            Debug.LogWarning("Target tile is not walkable!");
            return;
        }

        //Getting the path to the target tile
        List<BaseTile> path = GetPath(OccupiedTile, targetTile);
        if (path == null)
        {
            Debug.LogWarning("No path to target tile!");
            return;
        }

        //Moving the unit along the path
        MoveState = UnitState.MOVING;
        currentPath = path;
    }

    public static List<BaseTile> GetPath(BaseTile start, BaseTile end)
    {
        List<BaseTile> tilesToSearch = new List<BaseTile>() { start };
        List<BaseTile> tilesProcessed = new List<BaseTile>();

        while (tilesToSearch.Any())
        {
            BaseTile current = tilesToSearch[0];
            foreach (BaseTile tile in tilesToSearch)
            {
                if (tile.F < current.F || (tile.F == current.F && tile.H < current.H))
                {
                    current = tile;
                }
            }

            tilesProcessed.Add(current);
            tilesToSearch.Remove(current);

            //Seeing if we've reached the end tile
            if (current == end)
            {
                List<BaseTile> path = new List<BaseTile>();
                while (current != start)
                {
                    path.Add(current);
                    current = current.Connection;
                }
                path.Reverse();
                return path;
            }

            //Looking at neighbors that are walkable and not already processed
            foreach (BaseTile neighbor in current.Neighbors.Where(t => t.Walkable && !tilesProcessed.Contains(t)))
            {
                bool inSearch = tilesToSearch.Contains(neighbor);

                float costToNeighbor = current.G + current.GetDistance(neighbor);

                //Getting new values for neighbor's G and H, and connecting it to current if it's a better path
                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.SetConnection(current);
                    neighbor.SetG((int)costToNeighbor);
                    if (!inSearch)
                    {
                        neighbor.SetH((int)neighbor.GetDistance(end));
                        tilesToSearch.Add(neighbor);
                    }
                }
            }
        }

        //No path found
        Debug.LogWarning("No path found!");
        return null;
    }

    public enum UnitState
    {
        IDLE = 0,
        MOVING = 1,
        ATTACKING = 2,
        DEAD = 3
    }
}
