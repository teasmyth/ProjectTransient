using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class Treasure : GameState
{
    [SerializeField] private float shrinkingAnimSpeed;
    public HexGridManager.ColorType ColorType { get; private set; }

    public HexGridManager.ColorType DebugColor;
    private bool _growing;

    private float _localScale;
    private bool _shrinking;
    private List<HexGridObject> _spawnLocations = new();
    private Vector3 _targetPos;

    public static Treasure Instance { get; private set; }

    public HexGridObject PreviousHexGridObject { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else Instance = this;
    }
    
    public override void Enter(StateMachine stateMachine)
    {
        HexGridManager.HexGrid.GetGridObject(transform.position).SetOccupied(false);
        TurnIsDone = false;
        PreviousHexGridObject = GetHexGridObject();
        SelectPos();
    }

    public override void Execute(StateMachine stateMachine)
    {
        if (Move(_targetPos))
        {
            TurnIsDone = true;
            TurnDone();
        }
    }

    public override void Exit(StateMachine stateMachine)
    {
        _shrinking = false;
        _growing = false;
        TurnIsDone = false;
        GetHexGridObject().SetOccupied(true);
    }

    public override void SetupState(StateMachine stateMachine)
    {
        base.SetupState(stateMachine);
        SetUpTreasure();
    }

    public override string GetStateDetails()
    {
        string txt = "Treasure State";
        txt += "\n Color: " + ColorType + "\n";
        txt += "Shrinking: " + _shrinking + "\n";
        txt += "Growing: " + _growing + "\n";




        return txt;
    }

    private void SetUpTreasure()
    {
        _localScale = transform.localScale.x;
        transform.position = HexGridManager.HexGrid.GetGridObject(0, 0).GetPos() + new Vector3(0, _localScale / 2, 0);
        ColorType = HexGridManager.UsedColors[Random.Range(0, HexGridManager.UsedColors.Count)];
        for (int i = 0; i < HexGridManager.GridArray.GetLength(0); i++)
        {
            for (int j = 0; j < HexGridManager.GridArray.GetLength(1); j++)
            {
                if (HexGridManager.GridArray[i, j].GetColorType() != ColorType) continue;

                _spawnLocations.Add(HexGridManager.GridArray[i, j]);
            }
        }
        
        DebugColor = ColorType;
    }

    private void SelectPos()
    {
        List<HexGridObject> possibleLocations = new List<HexGridObject>();

        foreach (var t in _spawnLocations)
        {
            if (t == Player.Instance?.GetHexGridObject()) continue;

            possibleLocations.Add(t);
        }

        if (possibleLocations.Count == 0)
        {
            TurnIsDone = true;
            return;
        }

        HexGridObject toMove = possibleLocations[Random.Range(0, possibleLocations.Count)];
        _targetPos = toMove.gameObject.transform.position + new Vector3(0, transform.localScale.x / 2, 0);
        _shrinking = true;
    }

    private bool Move(Vector3 targetPos)
    {
        if (_shrinking && !_growing)
        {
            transform.localScale -=
                new Vector3(shrinkingAnimSpeed, shrinkingAnimSpeed, shrinkingAnimSpeed) * Time.deltaTime;
            if (transform.localScale.x <= 0.01f)
            {
                transform.localScale = Vector3.zero;
                _growing = true;
            }
        }

        else if (_shrinking && _growing)
        {
            transform.position = targetPos;
            new WaitForSeconds(.2f);
            _shrinking = false;
        }

        else if (!_shrinking && _growing)
        {
            transform.localScale +=
                new Vector3(shrinkingAnimSpeed, shrinkingAnimSpeed, shrinkingAnimSpeed) * Time.deltaTime;
            if (transform.localScale.x >= _localScale)
            {
                transform.localScale = new Vector3(_localScale, _localScale, _localScale);
                return true;
            }
        }

        return false;
    }

    public void DebugEnableMesh()
    {
        GetComponent<MeshRenderer>().enabled = true;
    }
}