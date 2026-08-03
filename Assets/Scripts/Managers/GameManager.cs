using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class that manages the overall game state and flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    //Properties
    public static GameManager Instance;
    [HideInInspector] public GameState GameState;
    [HideInInspector] public UnityEvent EnemyEndTurn = new UnityEvent();
    private bool enemyTurnEnded = false;

    [Header("Game Settings")]
    [Range(10, 120)]public int FrameRateCap = 60;
    public float TransitionDelay = 1.5f;
    public float StartDelay = 0.1f;

    //Methods
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = FrameRateCap;

        //Add listener for the EnemyEndTurn event to handle the end of the enemy's turn.
        EnemyEndTurn.AddListener(TriggerEndEnemyTurn);
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
        enemyTurnEnded = false;
        GridManager.Instance.ClearAllHighlights();
        UIManager.Instance.UpdateTurnIndicator(GameState.ENEMY_TURN);
        UnitManager.Instance.MoveEnemies();
    }

    private void StartMistTurn()
    {
        UIManager.Instance.UpdateTurnIndicator(GameState.MIST_TURN);

        MistManager.Instance.SpreadMist();
        StartCoroutine(EndMistTurn()); 
    }

    private void TriggerEndEnemyTurn()
    {
        if (enemyTurnEnded) return;
        if (UnitManager.Instance.CheckIfEnemyTurnOver())
        {
            enemyTurnEnded = true;
            UnitManager.Instance.ResetEnemyTurns();
            StartCoroutine(EndEnemyTurn());
        }
    }

    private IEnumerator EndEnemyTurn()
    {
        yield return new WaitForSeconds(TransitionDelay);

        ChangeState(GameState.MIST_TURN);
    }

    private IEnumerator EndMistTurn()
    {
        yield return new WaitForSeconds(TransitionDelay);

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

