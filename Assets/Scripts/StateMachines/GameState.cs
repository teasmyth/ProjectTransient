using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState : MonoBehaviour, IGameState
{
    //State Machine
    
    public abstract void Enter(StateMachine stateMachine);
    public abstract void Execute(StateMachine stateMachine);
    
    public abstract void Exit(StateMachine stateMachine);

    public virtual void SetupState(StateMachine stateMachine)
    {
        Player = Player.Instance;
        HexGridManager = HexGridManager.Instance;
        Treasure = Treasure.Instance;
    }

    public abstract string GetStateDetails();

    public bool TurnIsDone { get; protected set; }
    protected void TurnDone() { EventManager.Trigger_AutoSwitchGameState(GetComponent<IGameState>()); }
    
    public HexGridObject GetHexGridObject()
    {
        return HexGridManager.Instance.HexGrid.GetGridObject(transform.position);
    }
    
    public HexGridObject PrevPos { get; protected set; }

    //
    //
    //

    //Referencing the Instances
    private protected Player Player;
    private protected HexGridManager HexGridManager;
    private protected Enemy Enemy;
    private protected Treasure Treasure;
    
    public enum PlayerAction
    {
        Waiting = 0,
        Moving = 1,
        Dashing = 2,
        Teleporting = 3,
        Jumping = 4,
        Cloning = 5
    }

    public PlayerAction PlayerActionType { get; protected set; }
    public void SetPlayerAction(PlayerAction newAction) => PlayerActionType = newAction;
    

    protected virtual void Start()
    {
        // Player = Player.Instance;
        // HexGridManager = HexGridManager.Instance;
        // Enemy = Enemy.Instance;
        // Treasure = Treasure.Instance;
    }

    public void SkipTurn() { }
}
