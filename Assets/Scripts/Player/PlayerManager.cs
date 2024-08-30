using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : GameState
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemy2Prefab;

    public GameState ActivePlayer { get; private set; }
    public GameState InactivePlayer { get; private set; }

    public GameObject DebugActivePlayer;

    public bool Multiplayer { get; private set; }

    private GameObject _player1;
    private GameObject _player2;
    private bool _switch;
    private bool firstTurn = true;

    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    public override void Enter(StateMachine stateMachine)
    {
        if (!firstTurn && _switch)
        {
            (ActivePlayer, InactivePlayer) = (InactivePlayer, ActivePlayer);
            DebugActivePlayer = ActivePlayer.gameObject;
        }

        if (firstTurn) firstTurn = false;
        
        ActivePlayer.Enter(stateMachine);
    }

    public override void Execute(StateMachine stateMachine)
    {
        ActivePlayer.Execute(stateMachine);

        if (ActivePlayer.TurnIsDone)
        {
            ActivePlayer.Exit(stateMachine);
            TurnDone();
        }
    }

    public override void Exit(StateMachine stateMachine)
    {
       
    }

    public override void SetupState(StateMachine stateMachine)
    {
        base.SetupState(stateMachine);

        switch (GameManager.Instance.GameModeSettings.gameMode)
        {
            case GameModeSettings.GameMode.CustomOnlyBot:
                _player1 = Instantiate(enemyPrefab);
                _player1.transform.position = HexGridManager.Instance.GetMiddlePos() + new Vector3(0, _player1.transform.localScale.x / 2, 0);
                _player1.name = "Bot 1";
                ActivePlayer = _player1.GetComponent<GameState>();
                break;
            case GameModeSettings.GameMode.SinglePlayer:
                _player1 = Instantiate(playerPrefab);
                _player1.transform.position = HexGridManager.Instance.GetMiddlePos() + new Vector3(0, _player1.transform.localScale.x / 2, 0);
                _player1.name = "Player 1";
                ActivePlayer = _player1.GetComponent<GameState>();
                break;
            case GameModeSettings.GameMode.CustomBotVBot:
                _player1 = Instantiate(enemyPrefab);
                _player1.transform.position = HexGridManager.Instance.GetBottomLeftPos() + new Vector3(0, _player1.transform.localScale.x / 2, 0);
                _player1.name = "Bot 1";

                _player2 = Instantiate(enemy2Prefab);
                _player2.transform.position = HexGridManager.Instance.GetTopRightPos() + new Vector3(0, _player2.transform.localScale.x / 2, 0);
                _player2.name = "Bot 2";
                _switch = true;
                Multiplayer = true;
                break;
            default:
                _player1 = Instantiate(playerPrefab);
                _player1.transform.position = HexGridManager.Instance.GetBottomLeftPos() + new Vector3(0, _player1.transform.localScale.x / 2, 0);
                _player1.name = "Player 1";

                _player2 = Instantiate(enemyPrefab);
                _player2.transform.position = HexGridManager.Instance.GetTopRightPos() + new Vector3(0, _player2.transform.localScale.x / 2, 0);
                _player2.name = "Player 2";
                _switch = true;
                Multiplayer = true;
                break;
        }

        if (_player1 != null)
        {
            _player1.GetComponent<GameState>().SetupState(stateMachine);
            ActivePlayer = _player1.GetComponent<GameState>();
        }

        if (_player2 != null)
        {
            _player2.GetComponent<GameState>().SetupState(stateMachine);
            InactivePlayer = _player2.GetComponent<GameState>();
        }
    }

    public override string GetStateDetails()
    {
        return "";
    }
}