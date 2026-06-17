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

    /// <summary>
    /// Change the current game state and trigger any necessary actions for that state.
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(GameState newState)
    {
        GameState = newState;
        switch (newState)
        {
            case GameState.GENERATING_GRID:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SPAWN_HEROES:
                UnitManager.Instance.SpawnRandomHeroes();
                break;
            case GameState.SPAWN_ENEMIES:
                UnitManager.Instance.SpawnEnemies();
                break;
            case GameState.HERO_TURN:
                StartHeroTurn();
                break;
            case GameState.ENEMY_TURN:
                StartEnemyTurn();
                break;
            case GameState.MIST_TURN:
                StartMistTurn();
                break;
            default:
                Debug.LogError("Unhandled game state: " + newState);
                break;
        }
    }

    private void StartHeroTurn()
    {
        UIManager.Instance.UpdateTurnIndicator(GameState.HERO_TURN);
        GridManager.Instance.UpdateVision();
        GridManager.Instance.HighlightHeroTiles();
    }

    private void StartEnemyTurn()
    {
        GridManager.Instance.ClearAllHighlights();
        UIManager.Instance.UpdateTurnIndicator(GameState.ENEMY_TURN);

        //TODO: Implement references to enemy AI and trigger their actions here, for now we wait a few seconds and then end the turn
        Invoke(nameof(EndEnemyTurn), 2f);
    }

    private void StartMistTurn()
    {
        UIManager.Instance.UpdateTurnIndicator(GameState.MIST_TURN);

        MistManager.Instance.SpreadMist();

        EndMistTurn();
    }

    private void EndEnemyTurn()
    {
        //TODO: Remove this method once enemy AI is implemented and instead call ChangeState(GameState.MIST_TURN) at the end of the enemy AI's turn.
        ChangeState(GameState.MIST_TURN);
    }

    private void EndMistTurn()
    {
        ChangeState(GameState.HERO_TURN);
    }
}

/// <summary>
/// Represents the current state of the game with 
/// Heroes, Enemies, and Mist taking turns after everything is spawned.
/// </summary>
public enum GameState
{
    GENERATING_GRID = 0,
    SPAWN_HEROES = 1,
    SPAWN_ENEMIES = 2,
    HERO_TURN = 3,
    ENEMY_TURN = 4,
    MIST_TURN = 5
}

