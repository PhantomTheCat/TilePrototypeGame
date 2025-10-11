using UnityEngine;

/// <summary>
/// Class that manages the overall game state and flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    //Properties
    public static GameManager Instance;
    public GameState GameState;

    //Methods
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.GENERATING_GRID);
    }

    public void ChangeState(GameState newState)
    {
        GameState = newState;
        switch (newState)
        {
            case GameState.GENERATING_GRID:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SPAWN_HEROES:
                UnitManager.Instance.SpawnHeroes();
                break;
            case GameState.SPAWN_ENEMIES:
                UnitManager.Instance.SpawnEnemies();
                break;
            case GameState.HERO_TURN:
                break;
            case GameState.ENEMY_TURN:
                break;
            default:
                Debug.LogError("Unhandled game state: " + newState);
                break;
        }
    }
}

public enum GameState
{
    GENERATING_GRID = 0,
    SPAWN_HEROES = 1,
    SPAWN_ENEMIES = 2,
    HERO_TURN = 3,
    ENEMY_TURN = 4
}
