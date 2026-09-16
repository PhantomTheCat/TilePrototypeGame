using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    //Properties
    public static UnitManager Instance;
    public BaseHero SelectedHero;
    private List<ScriptableUnit> units;
    private List<ScriptableAction> actions;
    private List<ScriptableAction> playerStartActions;

    [Header("Spawn Settings")]
    [SerializeField] private int AmountOfHeroes = 1;
    [SerializeField] private int AmountOfEnemies = 0;

    [Header("Character Creation Settings")]
    [SerializeField] private int StartingActionsPerHero = 1;
    [Range(1, 5)][SerializeField] private int MaxPerkTreesPerHero = 5;
    [Range(1, 5)][SerializeField] private int MaxPerksPerTree = 5;

    [HideInInspector] public List<BaseHero> Heroes { get; private set; } = new List<BaseHero>();
    [HideInInspector] public List<BaseEnemy> Enemies { get; private set; } = new List<BaseEnemy>();
    [HideInInspector] public List<BaseEnemy> ActiveEnemies { get; private set; } = new List<BaseEnemy>();
    [HideInInspector] public List<BaseHero> DefeatedHeroes { get; private set; } = new List<BaseHero>();



    //Methods
    private void Awake()
    {
        Instance = this;

        //Getting all units and actions from the Resources folder
        units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
        actions = Resources.LoadAll<ScriptableAction>("Actions").ToList();
        playerStartActions = actions.Where(a => a.CanStartOnPlayer).ToList();
    }

    public void SpawnRandomHeroes()
    {
        BaseHero selectedHero = null;

        for (int i = 0; i < AmountOfHeroes; i++)
        {
            BaseHero hero = GetRandomUnit<BaseHero>(Faction.HERO);
            BaseHero spawnedHero = Instantiate(hero);

            //Positioning the hero on the grid
            BaseTile spawnTile = GridManager.Instance.GetHeroSpawnTile();
            spawnedHero.Activate(spawnTile);

            //Giving the hero some random actions
            GiveRandomHeroActions(spawnedHero);
            GiveStarterItems(spawnedHero);

            if (i == 0)
            {
                //Making the first spawned hero the selected hero
                selectedHero = spawnedHero;
            }

            Heroes.Add(spawnedHero);
            spawnedHero.PerkTrees = spawnedHero.GetPerkTrees(MaxPerkTreesPerHero, MaxPerksPerTree);
        }

        if (selectedHero != null)
        {
            SelectedHero = selectedHero;
            StartCoroutine(DelayUIChanges(selectedHero));
        }

        //Updating the UI with the spawned hero
        UIManager.Instance.MakeCharacterButtons();

        //Moving to next step
        GameManager.Instance.ChangeState(GameState.SPAWN_ENEMIES);
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < AmountOfEnemies; i++)
        {
            BaseEnemy enemy = GetRandomUnit<BaseEnemy>(Faction.ENEMY);
            BaseEnemy spawnedEnemy = Instantiate(enemy);

            //Positioning the enemy on the grid
            BaseTile spawnTile = GridManager.Instance.GetEnemySpawnTile();
            spawnedEnemy.Activate(spawnTile);
            Enemies.Add(spawnedEnemy);
        }

        //Moving to next step
        GameManager.Instance.ChangeState(GameState.HERO_TURN);
    }

    public void MoveEnemies()
    {
        ActiveEnemies.Clear();
        ActiveEnemies.AddRange(Enemies.Where(e => e.VisibleToPlayer));
        ActiveEnemies.RemoveAll(e => e.MoveState != BaseUnit.UnitState.IDLE);
        if (ActiveEnemies.Count <= 0) GameManager.Instance.EnemyEndTurn.Invoke();

        foreach (BaseEnemy enemy in ActiveEnemies)
        {
            enemy.TakeEnemyMovement();
        }
    }

    public bool CheckIfEnemyTurnOver()
    {
        //If any enemy is still active, the turn is not over
        foreach (BaseEnemy enemy in ActiveEnemies)
        {
            if (enemy.FinishedTurn == false)
            {
                return false;
            }
        }
        return true;
    }

    public void ResetEnemyTurns()
    {
        foreach (BaseEnemy enemy in Enemies)
        {
            enemy.FinishedTurn = false;
        }
    }

    /// <summary>
    /// Returns if a tile is on the path of any active enemy for spawning and more
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool IsOnEnemyPath(BaseTile tile)
    {
        bool onEnemyPath = false;
        if (ActiveEnemies == null) return onEnemyPath;

        foreach (BaseEnemy enemy in ActiveEnemies)
        {
            List<BaseTile> enemyPath = enemy.ReturnPathOn();
            if (enemyPath == null || enemyPath.Count == 0) continue;
            if (enemy.ReturnPathOn().Contains(tile))
            {
                onEnemyPath = true;
            }
        }
        return onEnemyPath;
    }

    private void GiveRandomHeroActions(BaseHero hero)
    {
        if (playerStartActions.Count == 0)
        {
            Debug.LogWarning("No player start actions found. Please add some ScriptableActions with CanStartOnPlayer set to true.");
            return;
        }
        if (playerStartActions.Count < StartingActionsPerHero)
        {
            StartingActionsPerHero = playerStartActions.Count;
        }
        List<ScriptableAction> heroActions = playerStartActions.OrderBy(a => Random.value).Take(StartingActionsPerHero).ToList();
        List<BaseAction> instantiatedActions = new List<BaseAction>();
        foreach (ScriptableAction action in heroActions)
        {
            if (action.actionPrefab != null)
            {
                BaseAction instantiatedAction = Instantiate(action.actionPrefab);
                instantiatedActions.Add(instantiatedAction);
                instantiatedAction.transform.SetParent(hero.transform);
            }
        }

        hero.SetActions(instantiatedActions);
    }

    private void GiveStarterItems(BaseHero hero)
    {
        //Just giving one item for now for testing
        BaseItem testItem = ItemManager.Instance.GetRandomItemOfType(ItemType.WEAPON, Rarity.COMMON);
        hero.Inventory.Add(testItem);
    }

    public void ChangeSelectedHero(BaseHero hero)
    {
        SelectedHero = hero;
        UIManager.Instance.UpdateSelectedHeroUI(SelectedHero);
    }

    private IEnumerator DelayUIChanges(BaseHero hero)
    {
        yield return new WaitForSeconds(GameManager.Instance.StartDelay);
        ChangeSelectedHero(hero);
    }

    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit
    {
        //Getting the first unit of a randomized list that matches the faction type
        return units.Where(u => u.FactionType == faction).OrderBy(u => Random.value).First().UnitPrefab as T;
    }
}
