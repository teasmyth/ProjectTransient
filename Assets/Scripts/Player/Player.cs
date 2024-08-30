using System.Collections;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;

public sealed class Player : GameState
{
    public static Player Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
        Movement = GetComponent<PlayerMovement>();

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false;
    }

    public PlayerMovement Movement { get; private set; }
    public float LocalScale { get; private set; }

    private Vector3 _targetPos;
    private List<HexGridObject> _allowedMovementGridObjects = new();
    private List<HexGridObject> _gridObjectsColorToShow = new();

    private bool _allowedMovementGridObjectsSet;

    

    private MeshRenderer _meshRenderer;


    private HexGridObject _selectedGridObject;
    private bool _canAct;
    
   

   
    public delegate void PlayerTurnStart();
    public static event PlayerTurnStart OnPlayerTurnStart;

    public delegate bool PlayeClick(HexGridObject selectedHex);
    public static event PlayeClick OnPlayerClick;


    //State Machine Implementation
    public override void Enter(StateMachine stateMachine)
    {
        if (GetHexGridObject().IsHidden())
        {
            EventManager.Trigger_LoseGame(this, GetHexGridObject().GetColorType(), " You moved to an empty tile!");
        }
        
        PlayerActionType = PlayerAction.Moving;

        _allowedMovementGridObjectsSet = false;
        HexGridManager.HexGrid.GetGridObject(transform.position).SetOccupied(false);
        PrevPos = HexGridManager.HexGrid.GetGridObject(transform.position);
        SetColors(false);
        _gridObjectsColorToShow = HexGridManager.HexGrid.GetNeighborGridObjects(transform.position);
        _gridObjectsColorToShow.Add(GetHexGridObject());
        SetColors(true);
        PlayerActionType = PlayerAction.Moving;
        HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Movement);
        _allowedMovementGridObjectsSet = false;
        _canAct = false;

        OnPlayerTurnStart?.Invoke();
    }


    public override void Execute(StateMachine stateMachine)
    {
        if (PlayerActionType == PlayerAction.Cloning)
        {
            TurnIsDone = true;
        }

        if (!_allowedMovementGridObjectsSet) SetAllowedMovementGridObjects(PlayerActionType);
        if (!_canAct) SetTarget();
        if (_canAct && !TurnIsDone)
        {
            TurnIsDone = Movement.DoSelectedAction(PlayerActionType, _targetPos, LocalScale);
        }
    }

    public override void Exit(StateMachine stateMachine)
    {
        if (GetHexGridObject() == Treasure.Instance.GetHexGridObject())
        {
            EventManager.Trigger_WinGame(this);
        }
        
        if (PlayerManager.Instance.Multiplayer && GetHexGridObject() == PlayerManager.Instance.InactivePlayer.GetHexGridObject())
        {
            EventManager.Trigger_WinGame(this);
        }

        if (PlayerActionType == PlayerAction.Dashing && !GameManager.Instance.GameRuleset.GetDashPreventsFall() &&
            GetHexGridObject().GetColorType() == PrevPos.GetColorType())
        {
            EventManager.Trigger_LoseGame(this, GetHexGridObject().GetColorType(), " You dashed to the same color!");
        }

        if (PlayerActionType == PlayerAction.Teleporting)
        {
            //previous if already checks if it is the treasure
            if (GetHexGridObject().IsHidden())
            {
                EventManager.Trigger_LoseGame(this, GetHexGridObject().GetColorType(),
                    " That wasn't the treasure!");
            }
            
            //Not hidden but not the same color as the player.
            if (GameManager.Instance.GameRuleset.GetFailedTpFailsGame() &&
                GetHexGridObject().GetColorType() != PrevPos.GetColorType())

            {
                EventManager.Trigger_LoseGame(this, GetHexGridObject().GetColorType(),
                    " You tried to teleport to a different color!");
            }

            if (GameManager.Instance.GameRuleset.GetSuccessfulTpUnlocksColor() &&
                GetHexGridObject().GetColorType() == PrevPos.GetColorType())
            {
                GetHexGridObject().SetColorPermaShown(true);
            }
        }


        SetColors(false);

        _allowedMovementGridObjects.Clear();
        _canAct = false;
        TurnIsDone = false;
        HexGridManager.HexGrid.GetGridObject(transform.position).SetOccupied(true);
    }

    public override void SetupState(StateMachine stateMachine)
    {
        base.SetupState(stateMachine);
        LocalScale = transform.localScale.x;
        _meshRenderer.enabled = true;
        PrevPos = HexGridManager.HexGrid.GetGridObject(transform.position);
        transform.localScale *= HexGridManager.Instance.GetScale();
    }

    public override string GetStateDetails()
    {
        string txt = "Player State";
        txt += "\n Player Action: " + PlayerActionType + "\n";
        txt += "Allowed Movement Grid Objects: " + _allowedMovementGridObjects.Count + "\n";
        txt += "Selected Grid Object: " + _selectedGridObject + "\n";
        txt += "Can Act: " + _canAct + "\n";
        txt += "Allowed Movement Grid Objects Set: " + _allowedMovementGridObjectsSet + "\n";
        txt += "Prev Pos: " + PrevPos + "\n";
        txt += "Target Pos: " + _targetPos + "\n";
        txt += "Is Turn Done: " + TurnIsDone + "\n";
        
        
        return txt;
    }

    private void SetTarget()
    {
        if (TutorialManager.Instance != null && !TutorialManager.Instance.CanPlayerMove()) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _selectedGridObject = HexGridManager.HexGrid.GetGridObject(hit.point);

                if (_selectedGridObject == null || !_allowedMovementGridObjects.Contains(_selectedGridObject))
                {
                    return;
                }
                
                if (OnPlayerClick != null && !OnPlayerClick.Invoke(_selectedGridObject))
                {
                    return;
                }

                _targetPos = _selectedGridObject.GetPos() + new Vector3(0, transform.localScale.x / 2, 0);
                _canAct = true;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ResetTarget();
            PlayerActionType = PlayerAction.Moving;
            HexGridManager.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Movement);
            HexGridManager.HexGrid.TriggerDeselectAll();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HexGridObject hex = HexGridManager.HexGrid.GetGridObject(hit.point);
                if (IsHexAllowed(hex))
                {
                    hex.OverrideHighlight(HexGridManager.Instance.selectionMode);
                }
            }
        }
    }

    private void SetColors(bool value)
    {
        GetHexGridObject().ShowColor(value);
        foreach (var item in _gridObjectsColorToShow)
        {
            //if (item == _selectedGridObject && !value) continue;
            //todo if it was visible previous and will be visible, dont disable/enable, looks weird
            if (item != null && !item.IsHidden())
            {
                item.ShowColor(value);
            }
        }
        //if (!value) gridObjectsColorToShow.Clear();
    }


    private void SetAllowedMovementGridObjects(PlayerAction action)
    {
        //hexGridManager.HexGrid.TriggerDeselectAll(hexGridManager.HexGrid.getPlayersObject);
        _allowedMovementGridObjects.Clear();
        switch (action)
        {
            case PlayerAction.Moving:
                _allowedMovementGridObjects = Movement.SetMoveTargets();
                break;
            case PlayerAction.Dashing:
                _allowedMovementGridObjects = Movement.SetDashTargets();
                break;
            case PlayerAction.Teleporting:
                _allowedMovementGridObjects = Movement.SetTeleportTargets();
                break;
            case PlayerAction.Jumping:
                _allowedMovementGridObjects = Movement.SetJumpTargets();
                break;
        }

        _allowedMovementGridObjectsSet = true;
    }

   

    public void ResetTarget()
    {
        _canAct = false;
        _selectedGridObject = null;
        _allowedMovementGridObjectsSet = false;
    }

    //Checks for null too.
    public bool IsHexAllowed(HexGridObject hexGridObject)
    {
        if (hexGridObject == null) return false;

        return _allowedMovementGridObjects.Contains(hexGridObject);
    }
    
}