using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents the base class for all enemy units in the game.
/// </summary>
public class BaseEnemy : BaseUnit
{
    //Properties
    [Header("Enemy AI")]
    public Rarity DropRarity;
    public EnemyStyle Style;
    [SerializeField] protected int preferenceValue = 2;
    [SerializeField] protected ActionType[] preferredActions;
    [HideInInspector] public BaseHero TargetHero;
    [HideInInspector] public bool FinishedTurn = false;
    [HideInInspector] public bool VisibleToPlayer = false;
    protected bool actionTriggered = false;
    protected float engageDistance = 3f;
    protected float distanceFromTarget = 0f;


    /// <summary>
    /// Tells how the enemy behaves in combat. 
    /// Aggressive = Moves towards player. 
    /// Defensive = Keeps distance but can close in.
    /// Passive = Stays in place and waits for player to come to it. 
    /// </summary>
    public enum EnemyStyle
    {
        AGGRESSIVE = 0,
        DEFENSIVE = 1,
        PASSIVE = 2,
    }


    //Methods
    protected override void Update()
    {
        if (GameManager.Instance.GameState != GameState.ENEMY_TURN) return;
        base.Update();

        if (MoveState == UnitState.IDLE && currentPath == null)
        {
            EndEnemyTurn();
        }

        if (MoveState == UnitState.USING_ACTION && !actionTriggered)
        {
            actionTriggered = true;
            EnemyAction();
        }
    }

    public override void Die()
    {
        base.Die();
        WaitForSeconds wait = new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    protected void EndEnemyTurn()
    {
        MoveState = UnitState.IDLE;
        FinishedTurn = true;
        actionTriggered = false;

        //Notify the GameManager that this enemy has finished its turn
        //Will not always end the enemy turn, as it requires all active enemies to finish their turns
        GameManager.Instance.EnemyEndTurn.Invoke();
    }

    #region Movement
    /// <summary>
    /// Triggers the movement behavior of the enemy based on 
    /// its defined style (aggressive, defensive, or passive).
    /// </summary>
    public virtual void TakeEnemyMovement()
    {
        switch (Style)
        {
            case EnemyStyle.AGGRESSIVE:
                AggressiveMovement();
                break;
            case EnemyStyle.DEFENSIVE:
                DefensiveMovement();
                break;
            case EnemyStyle.PASSIVE:
                //Doesn't move or attack, just waits for the player to come to it
                MoveState = UnitState.USING_ACTION;
                break;
            default:
                Debug.LogWarning("Enemy style not recognized.");
                break;
        }
    }

    protected virtual BaseHero GetClosestHero()
    {
        BaseHero closestHero = null;
        float closestDistance = Mathf.Infinity;
        foreach (BaseHero hero in UnitManager.Instance.Heroes)
        {
            float distance = OccupiedTile.GetDistance(hero.OccupiedTile);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHero = hero;
            }
        }
        return closestHero;
    }

    protected virtual BaseTile GetClosestTileInRange(BaseTile heroTile, int range)
    {
        List<BaseTile> otherTargets = new List<BaseTile>();
        foreach (BaseEnemy enemy in UnitManager.Instance.ActiveEnemies)
        {
            if (enemy.TargetTile == null) continue;
            otherTargets.Add(enemy.TargetTile);
        }

        //Trying to find the closest tile to the hero for our enemy
        BaseTile closestTile = null;
        float closestDistance = Mathf.Infinity;
        foreach (BaseTile tile in heroTile.Neighbors)
        {
            if (otherTargets.Contains(tile)) continue;
            float distance = OccupiedTile.GetDistance(tile);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTile = tile;
            }
        }

        //Checking if that tile is within range, otherwise get closest tile in range to the tile we found
        if (closestDistance/10 > range)
        {
            BaseTile adjancentHeroTile = closestTile;
            closestDistance = Mathf.Infinity;
            List<BaseTile> rangeTiles = GetMovementRange();
            //Getting the closest tile in our range to our target tile
            foreach (BaseTile tile in rangeTiles)
            {
                if (otherTargets.Contains(tile)) continue;
                float distance = adjancentHeroTile.GetDistance(tile);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTile = tile;
                }
            }
        }

        if (closestTile == null) Debug.LogWarning($"Wasn't able to find a move tile for {this}.");
        return closestTile;
    }

    protected virtual void AggressiveMovement()
    {
        //Get closest hero to attack
        BaseHero closestHero = GetClosestHero();
        if (closestHero == null)
        {
            EndEnemyTurn();
            return;
        }
        TargetHero = closestHero;
        distanceFromTarget = OccupiedTile.GetDistance(TargetHero.OccupiedTile);

        //Get a path to a tile adjacent to the closest hero
        BaseTile heroTile = TargetHero.OccupiedTile;
        BaseTile targetTile = GetClosestTileInRange(heroTile, MoveRange);
        if (targetTile == null) EndEnemyTurn();
        else CheckPath(targetTile);
    }

    protected virtual void DefensiveMovement()
    {
        //Get closest hero
        BaseHero closestHero = GetClosestHero();
        if (closestHero == null)
        {
            EndEnemyTurn();
            return;
        }
        TargetHero = closestHero;

        //If hero is close enough, go forward to attack, otherwise stay where we are
        distanceFromTarget = OccupiedTile.GetDistance(TargetHero.OccupiedTile);
        if (distanceFromTarget <= engageDistance)
        {
            //Get a path to a tile adjacent to the closest hero
            BaseTile heroTile = TargetHero.OccupiedTile;
            BaseTile targetTile = GetClosestTileInRange(heroTile, MoveRange);
            if (targetTile == null) EndEnemyTurn();
            else CheckPath(targetTile);
        } 
        else
        {
            MoveState = UnitState.USING_ACTION;
        }
    }
    #endregion


    #region Actions
    public virtual void EnemyAction()
    {
        //For now, just attack the closest hero using an action from the enemy's action list
        //Attack closest hero using an action from the enemy's action list
        if (Actions.Count == 0)
        {
            Debug.LogWarning("Enemy has no actions to perform.");
            EndEnemyTurn();
            return;
        }

        //Assuming the first action is an attack for now
        CurrentAction = ChooseAction();

        //Get our range tiles for the action to determine if we can attack the target hero
        List<BaseTile> targetTiles = CurrentAction.GetTargetTiles(this);
        ActionType actionType = CurrentAction.ActionType;
        ActionType[] attackTypes = new ActionType[] { ActionType.MELEE_ATTACK, ActionType.RANGE_ATTACK, ActionType.SPELL_ATTACK };

        if (attackTypes.Contains(actionType))
        {
            EnemyAttack(targetTiles);
        }
        else
        {
            switch (actionType)
            {
                case ActionType.HEAL:
                    EnemyHeal(targetTiles);
                    break;
                case ActionType.SUMMON:
                    EnemySummon(targetTiles);
                    break;
                case ActionType.BUFF:
                    EnemyBuff(targetTiles);
                    break;
            }
        }

        EndEnemyTurn();
    }

    protected virtual void ExecuteAction(BaseTile selectedTile)
    {
        List<BaseTile> actionTiles = CurrentAction.GetActionTiles(this, selectedTile);
        if (actionTiles == null) return;
        PlayAnimation(CurrentAction);
        CurrentAction.Execute(this, actionTiles);
    }

    protected virtual void EnemyAttack(List<BaseTile> targetTiles)
    {
        if (targetTiles.Contains(TargetHero.OccupiedTile))
        {
            ExecuteAction(TargetHero.OccupiedTile);
        }
        else
        {
            //Get Action tiles from the tile in range that is closest to the target hero
            BaseTile closestTile = GetClosestTileInRange(TargetHero.OccupiedTile, CurrentAction.Range);
            List<BaseTile> actionTiles = CurrentAction.GetActionTiles(this, closestTile);
            if (actionTiles.Any(a => UnitManager.Instance.Heroes.Any(h => h.OccupiedTile == a)))
            {
                PlayAnimation(CurrentAction);
                CurrentAction.Execute(this, actionTiles);
            }
        }
    }

    protected virtual void EnemySummon(List<BaseTile> targetTiles)
    {
        //Getting a random open tile for the new enemy to spawn on
        List<BaseTile> emptyTiles = targetTiles.Where(t => t.Walkable).ToList();
        int index = Random.Range(0, emptyTiles.Count);
        BaseTile selectedTile = emptyTiles[index];

        ExecuteAction(selectedTile);
    }

    protected virtual void EnemyHeal(List<BaseTile> targetTiles)
    {
        //See if we need healing (prioritize ourselves first)
        if (CurrentHealth < MaxHealth && CurrentAction.CanTargetUser)
        {
            ExecuteAction(OccupiedTile);
        }
        else
        {
            //Get a nearby ally to heal them
            List<BaseUnit> allies = GetNearbyAllies(CurrentAction.Range);
            allies = allies.Where(a => a.CurrentHealth < MaxHealth).ToList();
            int index = Random.Range(0, allies.Count);
            BaseUnit allyToHeal = allies[index];

            ExecuteAction(allyToHeal.OccupiedTile);
        }
    }

    protected virtual void EnemyBuff(List<BaseTile> targetTiles)
    {
        //For now, just buff ourselves
        ExecuteAction(OccupiedTile);

        //TO-DO: Add logic for buffing other enemies if we want a feature like that
    }

    protected virtual void PlayAnimation(BaseAction action)
    {
        switch (action.ActionType)
        {
            case ActionType.MELEE_ATTACK:
                PlayMeleeAttackAnimation();
                break;
            case ActionType.RANGE_ATTACK:
                PlayRangedAttackAnimation();
                break;
            case ActionType.SPELL_ATTACK:
                PlaySpellAttackAnimation();
                break;
            case ActionType.HEAL:
                PlayHealAnimation();
                break;
            case ActionType.SUMMON:
                PlaySummonAnimation();
                break;
            case ActionType.BUFF:
                PlayBuffAnimation();
                break;
        }
    }

    protected virtual BaseAction ChooseAction()
    {
        //Making a dictionary that has the value of each action as an int
        Dictionary<BaseAction, int> appblicableActions = GetApplicableActions();
        BaseAction chosenAction = Actions[0];

        //Getting a random action based on the value of each action
        List<BaseAction> randomActions = new List<BaseAction>();
        foreach (BaseAction action in appblicableActions.Keys)
        {
            int value = appblicableActions[action];
            for (int i = 0; i < value; i++)
            {
                randomActions.Add(action);
            }
        }
        if (randomActions.Count > 0)
        {
            int index = Random.Range(0, randomActions.Count);
            chosenAction = randomActions[index];
        }

        return chosenAction;
    }

    /// <summary>
    /// Seeing which actions suit the situation best considering the enemy preferences as well, and assigning each a value.
    /// </summary>
    /// <returns></returns>
    protected Dictionary<BaseAction, int> GetApplicableActions()
    {
        Dictionary<BaseAction, int> appblicableActions = new Dictionary<BaseAction, int>();

        for (int i = 0; i < Actions.Count; i++)
        {
            BaseAction action = Actions[i];
            ActionType type = action.ActionType;

            if (type == ActionType.MELEE_ATTACK || type == ActionType.RANGE_ATTACK || type == ActionType.SPELL_ATTACK)
            {
                //Seeing if within range
                float distance = OccupiedTile.GetDistance(TargetHero.OccupiedTile);
                if (distance <= action.Range)
                {
                    if (distance < engageDistance)
                    {
                        appblicableActions.Add(action, preferenceValue);
                    }
                    else if (preferredActions.Contains(type))
                    {
                        appblicableActions.Add(action, preferenceValue);
                    }
                    else
                    {
                        appblicableActions.Add(action, 1);
                    }
                }
            }
            else if (type == ActionType.SUMMON || type == ActionType.BUFF)
            {
                if (preferredActions.Contains(type))
                {
                    appblicableActions.Add(action, preferenceValue);
                }
                else
                {
                    appblicableActions.Add(action, 1);
                }
            }
            else if (type == ActionType.HEAL)
            {
                //See if health is low for this or ally nearby
                //Get allies nearby in our healing range
                List<BaseUnit> allies = GetNearbyAllies(action.Range);
                int healingValue = 0;
                if (CurrentHealth <= MaxHealth)
                {
                    healingValue++;
                }
                if (allies.Any(e => e.CurrentHealth <= e.MaxHealth))
                {
                    healingValue++;
                }

                appblicableActions.Add(action, healingValue);
            }
        }

        return appblicableActions;
    }
    #endregion
}
