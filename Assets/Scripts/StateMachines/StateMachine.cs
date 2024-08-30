using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public IGameState PlayerManagerState
    {
        get { return _playerManagerState; }
    }

    public IGameState HexGridState
    {
        get { return _hexGridState; }
    }
    

    public IGameState TreasureState
    {
        get { return _treasureState; }
    }

   

    private IGameState _playerManagerState;
    private IGameState _hexGridState;
    private IGameState _treasureState;

    private IGameState _currentState;
    private bool _changeStateDone = false;
    private bool _runStateMachine = false;

    public StateMachine(IGameState playerManagerManagerState, IGameState hexGridState, IGameState treasureState, bool manualSwitch = false)
    {
        _playerManagerState = playerManagerManagerState;
        _hexGridState = hexGridState;
        _treasureState = treasureState;


        EventManager.OnAutoSwitchGameState += AutoSwitchState;
        if (!manualSwitch) EventManager.OnChangeGameState += ChangeState;
        
        SetupGame();

        RunStateMachine();
    }

    public IGameState GetCurrentState() => _currentState;

    public void ChangeState(IGameState newState)
    {
        if (!_runStateMachine || newState == null) return;

        _changeStateDone = false;

        if (_currentState != null) _currentState.Exit(this);

        _currentState = newState;
        _currentState.Enter(this);
        _changeStateDone = true;
    }

    public void Update()
    {
        if (_runStateMachine && _currentState != null && _changeStateDone)
        {
            _currentState.Execute(this);
        }
    }

    public void AutoSwitchState(IGameState currentState)
    {
        if (!_runStateMachine) return;

        switch (currentState)
        {
            case IGameState when currentState == _playerManagerState:
                ChangeState(_treasureState);
                break;
            case IGameState when currentState == _hexGridState:
                ChangeState(_playerManagerState);
                break;
            case IGameState when currentState == _treasureState:
                ChangeState(_hexGridState);
                break;
            default: throw new System.Exception("Error with the state machine: Auto Switch State");
        }
    }

    public void ResetPointers(IGameState playerManagerState, IGameState hexGridState, IGameState treasureState)
    {
        _playerManagerState = playerManagerState;
        _hexGridState = hexGridState;
        _treasureState = treasureState;
        
        _currentState = null;
        
        SetupGame();
        RunStateMachine();
    }

    private void SetupGame()
    {
        _hexGridState.SetupState(this);
        _playerManagerState.SetupState(this);
        _treasureState.SetupState(this);
    }

    public void RunStateMachine() => _runStateMachine = true;
    public void StopStateMachine() => _runStateMachine = false;
}