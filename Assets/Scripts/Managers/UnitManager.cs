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
    [SerializeField] private int AmountOfHeroes = 1;
    [SerializeField] private int AmountOfEnemies = 0;

    //Methods
    private void Awake()
    {
        Instance = this;
        units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    public void SpawnHeroes()
    {
        for (int i = 0; i < AmountOfHeroes; i++)
        {
            BaseHero hero = GetRandomUnit<BaseHero>(Faction.HERO);
            BaseHero spawnedHero = Instantiate(hero);

            //Positioning the hero on the grid
            BaseTile spawnTile = GridManager.Instance.GetHeroSpawnTile();
            spawnedHero.Activate(spawnTile);
            spawnTile.SetUnit(spawnedHero);

            if (i == 0)
            {
                //Making the first spawned hero the selected hero
                ChangeSelectedHero(spawnedHero);
            }
        }

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
