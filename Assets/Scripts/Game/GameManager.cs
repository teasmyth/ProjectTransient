using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameRulesetSettings manualGameRuleset;
    public GameModeSettings manualGameMode;

    public GameRulesetSettings GameRuleset { get; private set; }
    public GameModeSettings GameModeSettings { get; private set; }

    public GameRulesetSettings DebugRuleSet;
    
    public bool SinglePlayerMode => GameModeSettings.gameMode is GameModeSettings.GameMode.SinglePlayer or GameModeSettings.GameMode.CustomOnlyBot;

    public void SetGameRuleset(GameRulesetSettings rulesetSettings)
    {
        GameRuleset = rulesetSettings;
        DebugRuleSet = rulesetSettings;
        TurnCount = 0;
    }

    public bool playingFromMainMenu;
    public int TurnCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private StateMachine _stateMachine;
    [SerializeField] private string currentState;

    [SerializeField] GameObject hexGridPrefab, treasurePrefab, playerManagerPrefab;

    void Start()
    {
        if (playingFromMainMenu) return;


        GameRuleset = manualGameRuleset;
        GameModeSettings = manualGameMode;
        SetUpStateMachine();
        _stateMachine?.ChangeState(_stateMachine.PlayerManagerState);
        // Application.targetFrameRate = 60;
    }

    public void SetupGame()
    {
        SetUpStateMachine();
        _stateMachine?.ChangeState(_stateMachine.PlayerManagerState);
        playingFromMainMenu = false;
    }


    // Update is called once per frame
    private void Update()
    {
        if (_stateMachine == null) return;

        currentState = _stateMachine.GetCurrentState().ToString();
        _stateMachine.Update();
    }

    private void SetUpStateMachine()
    {
        var hexManagerObj = HexGridManager.Instance == null
            ? Instantiate(hexGridPrefab)
            : HexGridManager.Instance.gameObject;

        var playerManagerObj = playerManagerPrefab == null ? new GameObject() : Instantiate(playerManagerPrefab);
        var treasureObj = Treasure.Instance == null ? Instantiate(treasurePrefab) : Treasure.Instance.gameObject;


        if (_stateMachine == null)
        {
            _stateMachine = new StateMachine(GetState(playerManagerObj), GetState(hexManagerObj), GetState(treasureObj));
        }
        else
        {
            _stateMachine.ResetPointers(GetState(playerManagerObj), GetState(hexManagerObj), GetState(treasureObj));
        }
    }

    private IGameState GetState(GameObject GO)
    {
        return GO.TryGetComponent<IGameState>(out IGameState state) ? state : null;
    }

    public void RunStateMachine() => _stateMachine.RunStateMachine();
    public void StopStateMachine() => _stateMachine.StopStateMachine();

    public void IncrementTurnCount() => TurnCount++;

    public IGameState GetCurrentState() => _stateMachine.GetCurrentState();
}