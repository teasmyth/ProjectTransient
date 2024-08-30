using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyGameState : GameState
{
    public override void Enter(StateMachine stateMachine)
    {
        TurnIsDone = true;
    }

    public override void Execute(StateMachine stateMachine)
    {
        
    }

    public override void Exit(StateMachine stateMachine)
    {
        TurnIsDone = false;
    }
    
    

    public override string GetStateDetails()
    {
        return "Empty State";
    }
}
