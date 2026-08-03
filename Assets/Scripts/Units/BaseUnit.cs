using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for all units in the game.
/// </summary>
[RequireComponent(typeof(Animator))]
public class BaseUnit : MonoBehaviour
{
    //Properties
    [Header("General")]
    public Faction FactionType;
    public string UnitName;
    public UnitHealthbar Healthbar;
    public UnitState MoveState = UnitState.IDLE;
    public Sprite UnitPortrait;
    [HideInInspector] public int InteractableRange = 1;

    [Header("Stats")]
    public int MoveRange = 5;
    public int ViewRange = 15;
    public int MaxHealth = 100;
    public int MaxMana = 50;
    public int MaxActionPoint = 5;
    public int Level = 1;
    [HideInInspector] public int MoveRangeLeft = 5;
    [HideInInspector] public int CurrentHealth = 100;
    [HideInInspector] public int CurrentMana = 50;
    [HideInInspector] public int CurrentActionPoint = 5;

    [Header("Abilities")]
    public int Strength = 1;
    public int Dexterity = 1;
    public int Constitution = 1;
    public int Faith = 1;
    public int Intelligence = 1;
    public float CritChance = 5f;
    public int CritMultiplier = 2;

    [Header("Pathfinding")]
    public BaseTile OccupiedTile;
    [HideInInspector] public BaseTile TargetTile;
    [SerializeField] protected float moveSpeed = 5f;
    protected List<BaseTile> currentPath;
    protected int currentPathIndex = 0;

    [Header("Actions")]
    [Tooltip("Actions this unit can perform. Only need to specify these for enemy type units")]
    public List<BaseAction> Actions;
    [HideInInspector] public BaseAction CurrentAction;

    public List<BaseItem> Inventory = new List<BaseItem>();
    [HideInInspector] public int InventorySize = 25;

    [Header("Animation Stats")]
    [Range(0, 5)][SerializeField] protected int meleeAttackAnimCount = 1;
    [Range(0, 5)][SerializeField] protected int rangedAttackAnimCount = 1;
    [Range(0, 5)][SerializeField] protected int spellAttackAnimCount = 1;
    protected Animator animator;


    //Methods
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        CurrentHealth = MaxHealth;
        CurrentMana = MaxMana;
        CurrentActionPoint = MaxActionPoint;

        if (Healthbar != null)
        {
            Healthbar.AssignUnit(this);
        }
        else
        {
            Healthbar = GetComponentInChildren<UnitHealthbar>();
            if (Healthbar != null) Healthbar.AssignUnit(this);
        }
    }

    protected virtual void Update()
    {
        //If we are moving, move towards the target tile
        if (MoveState == UnitState.MOVING && currentPath != null && currentPathIndex < currentPath.Count)
        {
            BaseTile targetTile = currentPath[currentPathIndex];
            Vector3 targetPosition = targetTile.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;

            animator.SetBool("Walking", true);

            transform.Translate((direction * moveSpeed) * Time.deltaTime);

            //If we reached the target tile, move to the next tile in the path
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                currentPathIndex++;

                if (currentPathIndex >= currentPath.Count)
                {
                    SetNewTile(targetTile, true);

                    //Reached the end of the path
                    currentPath = null;
                    TargetTile = null;
                    currentPathIndex = 0;
                    animator.SetBool("Walking", false);

                    if (FactionType == Faction.HERO)
                    {
                        MoveState = UnitState.IDLE;
                        MinimapManager.Instance.TakePicture();
                        StartCoroutine(DelayVision());
                    }
                    else if (FactionType == Faction.ENEMY)
                    {
                        //Moves to next state for enemy units, which is using an action
                        MoveState = UnitState.USING_ACTION;
                    }
                }
                else
                {
                    SetNewTile(targetTile, false);
                }
            }
        }
    }

    public virtual void Activate(BaseTile spawnTile)
    {
        OccupiedTile = spawnTile;
        spawnTile.SetUnit(this);
        transform.parent = spawnTile.transform;
    }

    public virtual void TakeDamage(int damage)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if (Healthbar != null) Healthbar.UpdateBar();

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }

    public virtual void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        if (Healthbar != null) Healthbar.UpdateBar();
    }

    public virtual void Die()
    {
        CurrentHealth = 0;
        MoveState = UnitState.DEAD;
        animator.SetTrigger("Death");
        OccupiedTile.OccupiedUnit = null;
        OccupiedTile = null;
    }

    public virtual void Revive(BaseTile spawnTile)
    {
        CurrentHealth = MaxHealth;
        MoveState = UnitState.IDLE;
        OccupiedTile = spawnTile;
        spawnTile.SetUnit(this);
        gameObject.SetActive(true);
        if (Healthbar != null) Healthbar.UpdateBar();
    }

    public void SetActions(List<BaseAction> newActions)
    {
        if (newActions == null)
        {
            Debug.LogWarning("Actions list cannot be null!");
            return;
        }
        if (newActions.Count > 9)
        {
            Debug.LogWarning("Not enough action slots for the new actions!");
            return;
        }

        Actions = newActions;
    }

    public void AddAction(BaseAction action)
    {
        if (Actions.Count > 10) return;
        Actions.Add(action);
    }

    public void RemoveAction(BaseAction action)
    {
        if (Actions.Contains(action))
        {
            Actions.Remove(action);
        }
    }

    /// <summary>
    /// Either increases or decreases a stat by a set amount
    /// </summary>
    /// <param name="isIncrease"></param>
    /// <param name="statAffected"></param>
    /// <param name="amount"></param>
    public void StatUpdate(AffectableStat statAffected, int amount)
    {
        switch (statAffected)
        {
            case AffectableStat.HP:
                MaxHealth += amount;
                CurrentHealth += amount;
                break;
            case AffectableStat.MANA:
                MaxMana += amount;
                CurrentMana += amount;
                break;
            case AffectableStat.MOVEMENT:
                MoveRange += amount;
                MoveRangeLeft += amount;
                break;
            case AffectableStat.ACTION:
                MaxActionPoint += amount;
                CurrentActionPoint += amount;
                break;
            case AffectableStat.STR:
                Strength += amount;
                break;
            case AffectableStat.DEX:
                Dexterity += amount;
                break;
            case AffectableStat.CON:
                Constitution += amount;
                break;
            case AffectableStat.FAITH:
                Faith += amount;
                break;
            case AffectableStat.INT:
                Intelligence += amount;
                break;
            case AffectableStat.CRITCHANCE:
                CritChance += amount;
                break;
            case AffectableStat.CRITMULT:
                CritMultiplier += amount;
                break;
        }
    }

    protected IEnumerator DelayVision()
    {
        yield return new WaitForFixedUpdate();
        GridManager.Instance.UpdateVision();
        GridManager.Instance.HighlightHeroTiles();
    }

    protected List<BaseUnit> GetNearbyAllies(int withinDistance)
    {
        List<BaseUnit> units = new List<BaseUnit>();
        if (FactionType == Faction.HERO)
        {
            units.AddRange(UnitManager.Instance.Heroes);
        }
        else if (FactionType == Faction.ENEMY)
        {
            units.AddRange(UnitManager.Instance.Enemies);
        }
        
        units = units.Where(e => e.OccupiedTile.GetDistance(OccupiedTile) <= withinDistance).ToList();
        return units;
    }

    #region Animations
    protected virtual void PlayMeleeAttackAnimation()
    {
        if (meleeAttackAnimCount <= 0) return;
        //Randomly plays one of the basic attacks
        int randomAnim = Random.Range(1, meleeAttackAnimCount + 1);
        animator.SetTrigger($"Attack{randomAnim}");
    }

    protected virtual void PlayRangedAttackAnimation()
    {
        if (rangedAttackAnimCount <= 0) return;
        //Randomly plays one of the basic attacks
        int randomAnim = Random.Range(1, rangedAttackAnimCount + 1);
        animator.SetTrigger($"RangeAttack{randomAnim}");
    }

    protected virtual void PlaySpellAttackAnimation()
    {
        if (spellAttackAnimCount <= 0) return;
        //Randomly plays one of the basic attacks
        int randomAnim = Random.Range(1, spellAttackAnimCount + 1);
        animator.SetTrigger($"SpellAttack{randomAnim}");
    }

    protected virtual void PlayHealAnimation()
    {
        animator.SetTrigger("Healing");
    }

    protected virtual void PlaySummonAnimation()
    {
        animator.SetTrigger("Summoning");
    }

    protected virtual void PlayBuffAnimation()
    {
        animator.SetTrigger("Buff");
    }
    #endregion


    #region Pathfinding
    /// <summary>
    /// Validates the path to the specified target tile and initiates movement if the path is valid.
    /// </summary>
    /// <remarks>If the target tile is not walkable, or if no valid path to the target tile exists,  the
    /// method logs a warning and does not initiate movement. When a valid path is found,  the unit's state is set to
    /// <see cref="UnitState.MOVING"/> and the movement path is updated.</remarks>
    /// <param name="targetTile">The target tile to which the path is being checked. Must be walkable.</param>
    public virtual void CheckPath(BaseTile targetTile)
    {
        //Making sure the target tile is walkable and not null
        if (targetTile == null) return;
        if (!targetTile.Walkable) return;

        //Getting the movement range
        List<BaseTile> movementRange = GetMovementRange();
        if (!movementRange.Contains(targetTile)) return;

        //Getting the path to the target tile
        List<BaseTile> path = GetPath(OccupiedTile, targetTile);
        if (path == null) return;

        //Moving the unit along the path
        MoveState = UnitState.MOVING;
        LineManager.Instance.ClearLine();
        currentPath = path;
        TargetTile = targetTile;
    }

    /// <summary>
    /// Gets Path from start tile to end tile using A* pathfinding algorithm
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public List<BaseTile> GetPath(BaseTile start, BaseTile end)
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
        return null;
    }

    /// <summary>
    /// Gets a list of all tiles that are within the unit's movement range
    /// </summary>
    /// <returns></returns>
    public List<BaseTile> GetMovementRange()
    {
        List<BaseTile> reachableTiles = new List<BaseTile>();

        //Getting all the tiles within positive movement range
        for (int x = -MoveRange; x <= MoveRange; x++)
        {
            for (int y = -MoveRange; y <= MoveRange; y++)
            {
                BaseTile tile = CheckTileReachable(x, y);

                if (tile != null && tile.Walkable && !reachableTiles.Contains(tile))
                {
                    reachableTiles.Add(tile);
                }
            }
        }

        if (reachableTiles.Contains(OccupiedTile))
        {
            reachableTiles.Remove(OccupiedTile);
        }

        //Making sure all tiles are actually reachable via pathfinding
        foreach (BaseTile tile in reachableTiles.ToList())
        {
            List<BaseTile> path = GetPath(OccupiedTile, tile);
            if (path == null || path.Count > MoveRange)
            {
                reachableTiles.Remove(tile);
            }
        }

        return reachableTiles;
    }

    /// <summary>
    /// Function for setting a tile as the new occupied tile for this unit
    /// </summary>
    /// <param name="tile"></param>
    protected void SetNewTile(BaseTile tile, bool isLast)
    {
        OccupiedTile.OccupiedUnit = null;
        OccupiedTile = tile;
        tile.SetUnit(this);

        if (isLast) transform.parent = tile.transform;
        else transform.parent = null;
    }

    /// <summary>
    /// Checking if a tile at the given offset from the unit's current position is reachable
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private BaseTile CheckTileReachable(int x, int y)
    {
        return GridManager.Instance.GetTileAtPosition(new Vector2Int(OccupiedTile.Coords.Pos.x + x, OccupiedTile.Coords.Pos.y + y));
    }
    #endregion

    /// <summary>
    /// Represents the various states that a unit can be in during its lifecycle.
    /// </summary>
    /// <remarks>This enumeration defines the possible states of a unit, such as being idle, moving,
    /// attacking, or dead.  The state can be used to determine the current behavior or activity of the unit in a game
    /// or simulation.</remarks>
    public enum UnitState
    {
        IDLE = 0,
        MOVING = 1,
        USING_ACTION = 2,
        DEAD = 3
    }
}

public enum AffectableStat 
{ 
    HP = 0,
    MANA = 1,
    MOVEMENT = 2,
    ACTION = 3,
    STR = 4,
    DEX = 5,
    CON = 6,
    FAITH = 7,
    INT = 8,
    CRITCHANCE = 9,
    CRITMULT = 10
}

