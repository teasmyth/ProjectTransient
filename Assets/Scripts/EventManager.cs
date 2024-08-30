using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null & _instance != this)
        {
            Destroy(this);
        }
        else _instance = this;
    }

    public delegate void GridValueAdded();
    public static event GridValueAdded OnGridValueAdded;
    public void Trigger_GridValueAdded() => OnGridValueAdded?.Invoke();


    public delegate void PlayerSwitchAction(Player.PlayerAction newAction);
    public static event PlayerSwitchAction OnPlayerSwitchAction;
    public static void Trigger_PlayerSwitchAction(Player.PlayerAction newAction) => OnPlayerSwitchAction?.Invoke(newAction);


    public delegate void AutoSwitchGameState(IGameState currentState);
    public static event AutoSwitchGameState OnAutoSwitchGameState;
    public static void Trigger_AutoSwitchGameState(IGameState currentState) => OnAutoSwitchGameState?.Invoke(currentState);

    public delegate void ChangeGameState(IGameState newState);
    public static event ChangeGameState OnChangeGameState;
    public static void Trigger_ChangeGameState(IGameState newState) => OnChangeGameState?.Invoke(newState);

    public static void Trigger_WinGame(Player playerWon)
    {
        GameManager.Instance.StopStateMachine();
        string winText = $"{playerWon.name} has won!";
        HudManager.Instance.OnGameEndResultText(winText);
    }
    
    public static void Trigger_WinGame(string playerWon)
    {
        GameManager.Instance.StopStateMachine();
        string winText = $"{playerWon} has won!";
        HudManager.Instance.OnGameEndResultText(winText);
    }
    
    public static void Trigger_LoseGame(Player playerLost, HexGridManager.ColorType treasureColor, string extraText = "")
    {
        GameManager.Instance.StopStateMachine();
        string loseText = $"{playerLost.name} has lost! The treasure was {treasureColor}!";
        loseText += extraText;
        HudManager.Instance.OnGameEndResultText(loseText);
    }
}
