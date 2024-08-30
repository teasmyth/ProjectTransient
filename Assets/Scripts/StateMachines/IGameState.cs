using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameState
{
    public void Enter(StateMachine stateMachine);
    /// <summary>
    /// This runs at Update
    /// </summary>
    /// <param name="stateMachine"></param>
    public void Execute(StateMachine stateMachine);
    public void Exit(StateMachine stateMachine);
    
    public void SetupState(StateMachine stateMachine);
    public string GetStateDetails();
}
