using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    private Player _player;
    private void Start()
    {
        _player = GetComponent<Player>();
        
        EventManager.OnPlayerSwitchAction += DoAction;
    }

    private void DoAction(Player.PlayerAction newAction)
    {
        switch (newAction)
        {
            case Player.PlayerAction.Moving:
                OnClickMove();
                break;
            case Player.PlayerAction.Dashing:
                OnClickDashing();
                break;
            case Player.PlayerAction.Teleporting:
                OnClickTeleporting();
                break;
            case Player.PlayerAction.Cloning:
                OnClickClone();
                break;
            case Player.PlayerAction.Jumping:
                OnClickJump();
                break;
        }
    }


    public void OnClickDashing()
    {
        HexGridManager.Instance.HexGrid.TriggerDeselectAll();
        if (_player.PlayerActionType != Player.PlayerAction.Dashing)
        {
            _player.SetPlayerAction(Player.PlayerAction.Dashing);
            HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Movement);
        }
        else
        {
            _player.SetPlayerAction(Player.PlayerAction.Moving);
            HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Movement);

        }
        _player.ResetTarget();
    }

    public void OnClickTeleporting()
    {
        HexGridManager.Instance.HexGrid.TriggerDeselectAll();
        if (_player.PlayerActionType != Player.PlayerAction.Teleporting)
        {
            _player.SetPlayerAction(Player.PlayerAction.Teleporting); 
            HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Teleport);
        }
        else
        {
            _player.SetPlayerAction(Player.PlayerAction.Moving);
            HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Movement);
            
        }
        _player.ResetTarget();
    }

    public void OnClickJump()
    {
        HexGridManager.Instance.HexGrid.TriggerDeselectAll();
        if (_player.PlayerActionType != Player.PlayerAction.Jumping)
        {
            _player.SetPlayerAction(Player.PlayerAction.Jumping);
            HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Movement);
        }
        else
        {
            _player.SetPlayerAction(Player.PlayerAction.Moving);
            //HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.Instance.SelectionMode.Movement); 
        }
        _player.ResetTarget();

    }
    
    public void OnClickMove()
    {
        HexGridManager.Instance.HexGrid.TriggerDeselectAll();
        if (_player.PlayerActionType != Player.PlayerAction.Moving)
        {
            _player.SetPlayerAction(Player.PlayerAction.Moving);
            HexGridManager.Instance.HexGrid.TriggerSelectionModeSet(HexGridManager.SelectionMode.Movement);
        }
        _player.ResetTarget();
    }

    public void OnClickClone()
    {
        
    }
}
