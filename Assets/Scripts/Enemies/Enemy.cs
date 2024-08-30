using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public sealed class HexData
{
    public HexGridObject HexGridObject { get; private set; }

    public HexGridManager.ColorType ColorType { get; private set; }

    public bool HiddenWhenAdded { get; private set; }

    //How many turns ago this hex was accessed
    public int LastAccessed { get; private set; }

    public HexData(HexGridObject hexGridObject, int lastAccessed)
    {
        HexGridObject = hexGridObject;
        ColorType = hexGridObject.GetColorType();
        LastAccessed = lastAccessed;
        HiddenWhenAdded = hexGridObject.IsHidden();
    }

    public bool IsHidden() => HexGridObject.IsHidden();
    public void IncreaseLastAccessed() => LastAccessed++;
    public void ResetLastAccessed() => LastAccessed = 0;

    public static bool ContainsHex(HexData[,] array, HexGridObject item)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (array[i, j] == null) continue;

                if (array[i, j].HexGridObject == item)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static List<HexData> ConvertHexGridObjectListToHexData(List<HexGridObject> list)
    {
        return list.Select(t => new HexData(t, 0)).ToList();
    }
}

public class Enemy : GameState
{
    public float LocalScale { get; private set; }

    public bool stopTurn;
    public bool continueTurn = true;

    public PlayerMovement Movement { get; private set; }
    public EnemyBehaviourValues behaviourValues;

    [SerializeField] private float movementAnimSpeed;
    [SerializeField] private bool turnDone;

    /*
    public HexData[,] HexDataBase;
    [SerializeField] private List<HexGridManager.ColorType> _possibleTreasureColors = new();
    public List<HexGridManager.ColorType> PossibleTreasureColors => _possibleTreasureColors;
    [SerializeField] private HexGridManager.ColorType _treasureColor = HexGridManager.ColorType.Null;
    public HexGridManager.ColorType TreasureColor => _treasureColor;

    List<HexData> _previousObservation = new();
    */

   // public HexGridManager.ColorType PreviousColorType { get; private set; }

    private List<HexGridObject> _gridObjectsColorToShow = new();
    [SerializeField] private BehaviourTreeRunner BTrunner;
    private bool setupDone;

    private TextMesh[,] _probabilityArray;
    private bool showProbabilities;

#region Deprecated
    /*
    private void AddOrUpdateHexData(HexGridObject hexGridObject)
    {
        if (HexDataBase[hexGridObject.GetX(), hexGridObject.GetZ()] != null)
        {
            HexDataBase[hexGridObject.GetX(), hexGridObject.GetZ()].ResetLastAccessed();
            return;
        }

        HexDataBase[hexGridObject.GetX(), hexGridObject.GetZ()] = new HexData(hexGridObject, 0);
    }
    */

    /*
    private void UpdateHexDataBase()
    {
        for (var x = 0; x < HexDataBase.GetLength(0); x++)
        {
            for (var z = 0; z < HexDataBase.GetLength(1); z++)
            {
                if (HexDataBase[x, z] == null) continue;

                HexDataBase[x, z].IncreaseLastAccessed();
                if (HexDataBase[x, z].LastAccessed > forgetHexDataAfterTurns)
                {
                    HexDataBase[x, z] = null;
                }
            }
        }
    }
    */

    /*
    //A color cannot be treasure color if there are at least two hexes with the same color that is down
    private HexGridManager.ColorType UpdatePossibleTreasureColors(List<HexData> prevObservation,
        bool usePlayersPrevColor)
    {
        List<HexGridManager.ColorType> hiddenColors = new();
        var prevColor = usePlayersPrevColor ? Player.Instance.PrevPos.GetColorType() : PreviousColorType;

        foreach (var hex in prevObservation)
        {
            if (!HexData.ContainsHex(HexDataBase, hex.HexGridObject)) continue;
            //Why separate? because I didnt like the long if statement.
            if (!_possibleTreasureColors.Contains(hex.ColorType) || hex.HiddenWhenAdded || hex.ColorType == prevColor ||
                !hex.IsHidden()) continue;

            if (!hiddenColors.Contains(hex.ColorType))
            {
                hiddenColors.Add(hex.ColorType);
            }
            else if (_possibleTreasureColors.Contains(hex.ColorType))
            {
                _possibleTreasureColors.Remove(hex.ColorType);
            }
        }

        if (_possibleTreasureColors.Count == 1)
        {
            Debug.Log(transform.name + " - " + "The treasure is " + _possibleTreasureColors[0] + " - Turn: " + GameManager.Instance.TurnCount);
        }

        //If there is one left, that is the treasure, otherwise return null, meaning the treasure color is unknown.
        return _possibleTreasureColors.Count == 1 ? _possibleTreasureColors[0] : HexGridManager.ColorType.Null;
    }
    */

    /*
    public void Learn(bool forgetHexes, bool usePlayersPrevColor)
    {
        var hexInRange = HexGridManager.HexGrid.GetNeighborGridObjects(transform.position);
        foreach (var hex in hexInRange)
        {
            AddOrUpdateHexData(hex);
        }

        if (forgetHexes)
        {
            UpdateHexDataBase();
        }

        if (_treasureColor == HexGridManager.ColorType.Null)
        {
            _treasureColor = UpdatePossibleTreasureColors(_previousObservation, usePlayersPrevColor);
        }

        if (_treasureColor != HexGridManager.ColorType.Null)
        {
            var data = Extensions.TwoDimArrayToList(HexDataBase);
            int treasureFound = 0;
            HexData treasure = null;
            foreach (var hex in data)
            {
                if (hex == null) continue;

                if (hex.ColorType == _treasureColor && hex.IsHidden())
                {
                    treasure = hex;
                    treasureFound++;
                }
            }


            if (treasureFound == 1 && treasure != null)
            {
                Extensions.CreateWorldText("Treasure", null, treasure.HexGridObject.GetPos(), 2, Color.red,
                    TextAnchor.MiddleCenter, TextAlignment.Center, 0);
                Treasure.Instance.DebugEnableMesh();
                //EditorApplication.isPaused = true;
            }

        }

        if (!showProbabilities) return;

        if (_treasureColor == HexGridManager.ColorType.Null)
        {
            for (int x = 0; x < HexGridManager.Instance.GridArray.GetLength(0); x++)
            {
                for (int z = 0; z < HexGridManager.Instance.GridArray.GetLength(1); z++)
                {
                    if (!HexData.ContainsHex(HexDataBase, HexGridManager.Instance.GridArray[x, z]))
                    {
                        _probabilityArray[x, z].text = "?%";
                        continue;
                    }

                    if (_possibleTreasureColors.Contains(HexDataBase[x, z].ColorType))
                    {
                        _probabilityArray[x, z].text =
                            ((1.0f - _possibleTreasureColors.Count / 64.0f) * 100.0f).ToString("F1") + "%";
                    }
                    else
                    {
                        _probabilityArray[x, z].text = "0%";
                    }
                }
            }
        }
        else
        {
            bool treasureFound = false;
            foreach (var hexData in HexDataBase)
            {
                if (hexData == null) continue;

                if (hexData.ColorType == _treasureColor && hexData.IsHidden())
                {
                    treasureFound = true;
                    break;
                }
            }

            for (int x = 0; x < HexGridManager.Instance.GridArray.GetLength(0); x++)
            {
                for (int z = 0; z < HexGridManager.Instance.GridArray.GetLength(1); z++)
                {
                    if (HexDataBase[x, z] == null || HexDataBase[x, z].ColorType != _treasureColor)
                    {
                        _probabilityArray[x, z].text = "0%";
                        continue;
                    }

                    if (treasureFound)
                    {
                        if (HexDataBase[x, z].IsHidden())
                        {
                            _probabilityArray[x, z].text = "100%";
                        }
                        else
                        {
                            _probabilityArray[x, z].text = "0%";
                        }

                        continue;
                    }

                    _probabilityArray[x, z].text =
                        ((1.0f - _possibleTreasureColors.Count / 8.0f) * 100.0f).ToString("F1") + "%";
                }
            }

            if (treasureFound)
            {
                //EditorApplication.isPaused = true;
            }
        }
    }
    */

#endregion
    public override void Enter(StateMachine stateMachine)
    {
        foreach (var item in _gridObjectsColorToShow)
        {
            if (item == null) continue;

            if (PlayerManager.Instance.Multiplayer &&
                PlayerManager.Instance.InactivePlayer.GetHexGridObject().Neighbors.Contains(item))
            {
                continue;
            }

            item.ShowColor(false);
        }

        //Learn(true, false);

       // PreviousColorType = HexGridManager.HexGrid.GetGridObject(transform.position).GetColorType();
        PrevPos = GetHexGridObject();

        PlayerActionType = PlayerAction.Moving;

        if (GetHexGridObject().IsHidden() && Treasure.Instance.GetHexGridObject() != GetHexGridObject())
        {
            EditorApplication.isPaused = true;
        }
    }

    public override void Execute(StateMachine stateMachine)
    {
        if (!setupDone) return;
        
        if (stopTurn && !continueTurn)
        {
            return;
        }

        if (!TurnIsDone)
        {
            TurnIsDone = BTrunner.RunTree() != Node.State.Running;
        }
    }

    public override void Exit(StateMachine stateMachine)
    {
        SetColors();
        BTrunner.tree.ResetTree();
        TurnIsDone = false;

        if (GetHexGridObject() == Treasure.Instance.GetHexGridObject())
        {
            EditorApplication.isPaused = true;
            Treasure.Instance.DebugEnableMesh();
            Debug.Log(transform.name + " has won the game by finding the treasure! Other bot is stupid!");
            return;
        }

        if (PlayerManager.Instance.Multiplayer)
        {
            if (GetHexGridObject() == PlayerManager.Instance.InactivePlayer.GetHexGridObject())
            {
                EditorApplication.isPaused = true;
                EventManager.Trigger_WinGame(transform.name);
                Debug.Log(transform.name + " has won the game by killing other bot! Other bot is stupid!");
            }
        }

        if (stopTurn)
        {
            continueTurn = false;
        }
    }

    public override void SetupState(StateMachine stateMachine)
    {
        base.SetupState(stateMachine);
        //PreviousColorType = HexGridManager.ColorType.Null;
        Movement = GetComponent<PlayerMovement>();
        LocalScale = transform.localScale.x;
        //PrevPos = HexGridManager.HexGrid.GetGridObject(transform.position);
        transform.localScale *= HexGridManager.Instance.GetScale();

        BTrunner.tree.blackboard.HexDataBase = new HexData[HexGridManager.Instance.HexGrid.GetWidth(), HexGridManager.Instance.HexGrid.GetHeight()];
        BTrunner.tree.blackboard.possibleTreasureColors = HexGridManager.Instance.UsedColors.ToList();
        BTrunner.SetupTree();

        setupDone = true;

        if (!showProbabilities) return;
        
        _probabilityArray = new TextMesh[HexGridManager.Instance.HexGrid.GetWidth(),
            HexGridManager.Instance.HexGrid.GetHeight()];

        var parent = new GameObject("ProbabilityArray").transform;

        for (int x = 0; x < HexGridManager.Instance.HexGrid.GetWidth(); x++)
        {
            for (int z = 0; z < HexGridManager.Instance.HexGrid.GetHeight(); z++)
            {
                _probabilityArray[x, z] = Extensions.CreateWorldText("?%", parent,
                    HexGridManager.Instance.HexGrid.GetWorldPosition(x, z), 2, Color.red, TextAnchor.MiddleCenter,
                    TextAlignment.Center, 0);
            }
        }
    }

    public override string GetStateDetails()
    {
        return "";
    }

    private void SetColors()
    {
        _gridObjectsColorToShow = HexGridManager.HexGrid.GetNeighborGridObjects(transform.position);
        _gridObjectsColorToShow.Add(GetHexGridObject());

        foreach (var item in _gridObjectsColorToShow)
        {
            //todo if it was visible previous and will be visible, dont disable/enable, looks weird
            if (item != null && !item.IsHidden())
            {
                item.ShowColor(true);
            }
        }
    }

    public void ActionPotentialEvaluation(in HexGridObject actionTarget, in bool hexKnown, ref int potential,
        in int expWeight, in int explorationMultiplier, in int atkWeight,
        in int aggressionMultiplier,
        int risk)
    {
        if (!GameManager.Instance.SinglePlayerMode)
        {
            if (actionTarget == PlayerManager.Instance.InactivePlayer.GetHexGridObject())
            {
                potential = ActionEvaluation.InfinitePotential;
                return;
            }

            PotentialNeighborDashes(actionTarget, ref potential, atkWeight, aggressionMultiplier, risk);
        }

        //No need to check for any risk in any move, they are already prefiltered in previous steps at CheckForRiskyMoves node.


        if (!hexKnown)
        {
            potential += expWeight;
        }
        else if (BTrunner.tree.blackboard.possibleTreasureColors.Contains(actionTarget.GetColorType())) // && hexKnown
        {
            potential -= expWeight * explorationMultiplier;
        }
        else
        {
            potential -= expWeight;
        }

        foreach (var potentialNeighbor in actionTarget.Neighbors)
        {
            //if (potentialNeighbor.IsHidden() && !actionIsSafe) continue; //if its not safe, then this neighbor will go down next turn.

            if (!GameManager.Instance.SinglePlayerMode)
            {
                //I am checking for the neighbors of neighnors.
                //If i step onto opponents neighbor, they can step on me easily.
                //However if i step on the neighbor of their neighbor, then i deny that position for them.
                foreach (var neighbor in actionTarget.Neighbors)
                {
                    //Later make it more intelligent by paying attention to colors

                    if (neighbor.Neighbors.Contains(PlayerManager.Instance.InactivePlayer.GetHexGridObject()))
                    {
                        potential += atkWeight * aggressionMultiplier;
                        break;
                    }
                }
            }

            if (potentialNeighbor.IsHidden()) continue;

            if (!HexData.ContainsHex(BTrunner.tree.blackboard.HexDataBase, potentialNeighbor) ||
                HexData.ContainsHex(BTrunner.tree.blackboard.HexDataBase, potentialNeighbor) &&
                potentialNeighbor.GetColorType() != actionTarget.GetColorType())
            {
                potential += expWeight;
                if (BTrunner.tree.blackboard.possibleTreasureColors.Contains(potentialNeighbor.GetColorType()))
                {
                    potential += expWeight * explorationMultiplier;
                }

                continue;
            }

            potential -= expWeight;

            if (HexData.ContainsHex(BTrunner.tree.blackboard.HexDataBase, potentialNeighbor) &&
                BTrunner.tree.blackboard.possibleTreasureColors.Contains(potentialNeighbor.GetColorType()))
            {
                potential -= expWeight * explorationMultiplier;
            }
        }
    }

    public void PotentialNeighborDashes(in HexGridObject target, ref int potential, in int atkWeight, in int aggression,
        in int risk)
    {
        int maxDistance = HexGridManager.Instance.HexGrid.GetWidth() >= HexGridManager.Instance.HexGrid.GetHeight()
            ? HexGridManager.Instance.HexGrid.GetWidth()
            : HexGridManager.Instance.HexGrid.GetHeight();

        List<List<HexGridObject>> allDashableHexes =
            HexGridManager.Instance.HexGrid.GetLineAllDirections(target.GetPos(), maxDistance);

        var maxPotential = EnemyBehaviourValues.MaxPotential;

        for (int i = 0; i < allDashableHexes.Count; i++)
        {
            if (allDashableHexes[i].Count < 2) continue; // 1 is neighbor hex, dash not allowed to adjacent hexes.

            //Checking if the agent dashes, would it land on the opponent's neighbor, thereby getting advantage.
            if (!allDashableHexes[i].Any(hex =>
                    PlayerManager.Instance.InactivePlayer.GetHexGridObject().Neighbors.Contains(hex)))
            {
                continue;
            }

            for (int j = 1; j < allDashableHexes[i].Count; j++)
            {
                if (PlayerManager.Instance.InactivePlayer.GetHexGridObject().Neighbors.Contains(allDashableHexes[i][j]))
                {
                    potential += atkWeight * aggression;
                    break;
                }

                if (allDashableHexes[i][j].IsHidden() && risk != maxPotential)
                {
                    potential -= atkWeight * (maxPotential - risk);
                }
                else
                {
                    potential += atkWeight * aggression;
                }
            }
        }
    }
}