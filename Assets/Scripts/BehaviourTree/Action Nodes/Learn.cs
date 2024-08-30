using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Learn : ActionNode
{
    protected override void OnStart()
    {
        var hexInRange = HexGridManager.Instance.HexGrid.GetNeighborGridObjects(agent.transform.position);
        foreach (var hex in hexInRange)
        {
            AddOrUpdateHexData(hex);
        }


        UpdateHexDataBase();


        if (blackboard.treasureColor == HexGridManager.ColorType.Null)
        {
            blackboard.treasureColor = UpdatePossibleTreasureColors(blackboard.previousObservation);
        }
        
        blackboard.previousAgentColor = HexGridManager.Instance.HexGrid.GetGridObject(agent.transform.position).GetColorType();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        return State.Success;
    }

    private void AddOrUpdateHexData(HexGridObject hexGridObject)
    {
        if (blackboard.HexDataBase[hexGridObject.GetX(), hexGridObject.GetZ()] != null)
        {
            blackboard.HexDataBase[hexGridObject.GetX(), hexGridObject.GetZ()].ResetLastAccessed();
            return;
        }

        blackboard.HexDataBase[hexGridObject.GetX(), hexGridObject.GetZ()] = new HexData(hexGridObject, 0);
    }

    private void UpdateHexDataBase()
    {
        for (var x = 0; x < blackboard.HexDataBase.GetLength(0); x++)
        {
            for (var z = 0; z < blackboard.HexDataBase.GetLength(1); z++)
            {
                if (blackboard.HexDataBase[x, z] == null) continue;

                blackboard.HexDataBase[x, z].IncreaseLastAccessed();
                if (blackboard.HexDataBase[x, z].LastAccessed > agent.behaviourValues.Memory)
                {
                    blackboard.HexDataBase[x, z] = null;
                }
            }
        }
    }
    
    private HexGridManager.ColorType UpdatePossibleTreasureColors(List<HexData> prevObservation,
        bool usePlayersPrevColor = false)
    {
        List<HexGridManager.ColorType> hiddenColors = new();
        var prevColor = usePlayersPrevColor ? Player.Instance.PrevPos.GetColorType() : blackboard.previousAgentColor;

        foreach (var hex in prevObservation)
        {
            if (!HexData.ContainsHex(blackboard.HexDataBase, hex.HexGridObject)) continue;
            //Why separate? because I didnt like the long if statement.
            if (!blackboard.possibleTreasureColors.Contains(hex.ColorType) || hex.HiddenWhenAdded || hex.ColorType == prevColor ||
                !hex.IsHidden()) continue;

            if (!hiddenColors.Contains(hex.ColorType))
            {
                hiddenColors.Add(hex.ColorType);
            }
            else if (blackboard.possibleTreasureColors.Contains(hex.ColorType))
            {
                blackboard.possibleTreasureColors.Remove(hex.ColorType);
            }
        }

        if (blackboard.possibleTreasureColors.Count == 1)
        {
            Debug.Log(agent.transform.name + " - " + "The treasure is " + blackboard.possibleTreasureColors[0] + " - Turn: " + GameManager.Instance.TurnCount);
        }

        //If there is one left, that is the treasure, otherwise return null, meaning the treasure color is unknown.
        return blackboard.possibleTreasureColors.Count == 1 ? blackboard.possibleTreasureColors[0] : HexGridManager.ColorType.Null;
    }
}