using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    //Properties
    public static UnitManager Instance;
    public BaseHero SelectedHero;
    private List<ScriptableUnit> units;
    private List<ScriptableAction> actions;

    [Header("Spawn Settings")]
    [SerializeField] private int AmountOfHeroes = 1;
    [SerializeField] private int AmountOfEnemies = 0;

    [Header("Character Creation Settings")]
    [SerializeField] private int StartingActionsPerHero = 1;

    [HideInInspector] public List<BaseHero> Heroes { get; private set; } = new List<BaseHero>();
    [HideInInspector] public List<BaseEnemy> Enemies { get; private set; } = new List<BaseEnemy>();
    [HideInInspector] public List<BaseHero> DefeatedHeroes { get; private set; } = new List<BaseHero>();



    //Methods
    private void Awake()
    {
        Instance = this;

        //Getting all units and actions from the Resources folder
        units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
        actions = Resources.LoadAll<ScriptableAction>("Actions").ToList();
    }

    public void SpawnRandomHeroes()
    {
        for (int i = 0; i < AmountOfHeroes; i++)
        {
            BaseHero hero = GetRandomUnit<BaseHero>(Faction.HERO);
            BaseHero spawnedHero = Instantiate(hero);

            //Positioning the hero on the grid
            BaseTile spawnTile = GridManager.Instance.GetHeroSpawnTile();
            spawnedHero.Activate(spawnTile);
            spawnTile.SetUnit(spawnedHero);

            //Giving the hero some random actions
            GiveRandomHeroActions(spawnedHero);
            GiveStarterItems(spawnedHero);

            if (i == 0)
            {
                //Making the first spawned hero the selected hero
                ChangeSelectedHero(spawnedHero);
            }

            Heroes.Add(spawnedHero);
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

            //Positioning the hero on the grid
            BaseTile spawnTile = GridManager.Instance.GetEnemySpawnTile();
            spawnedEnemy.Activate(spawnTile);
            spawnTile.SetUnit(spawnedEnemy);
        }

        //Moving to next step
        GameManager.Instance.ChangeState(GameState.HERO_TURN);
    }

    private void GiveRandomHeroActions(BaseHero hero)
    {
        List<ScriptableAction> heroActions = actions.OrderBy(a => Random.value).Take(StartingActionsPerHero).ToList();
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
        BaseItem testItem = ItemManager.Instance.GetRandomItem();
        hero.Inventory.Add(testItem);
    }

    public void ChangeSelectedHero(BaseHero hero)
    {
        SelectedHero = hero;
        UIManager.Instance.UpdateSelectedHeroUI(SelectedHero);
    }

    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit
    {
        //Getting the first unit of a randomized list that matches the faction type
        return units.Where(u => u.FactionType == faction).OrderBy(u => Random.value).First().UnitPrefab as T;
    }
}
