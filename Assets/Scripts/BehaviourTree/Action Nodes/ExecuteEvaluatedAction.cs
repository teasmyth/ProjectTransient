using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExecuteEvaluatedAction : ActionNode
{
    private Vector3 _targetPosition;

    protected override void OnStart()
    {
        agent.SetPlayerAction(blackboard.bestAction.action);
        _targetPosition = blackboard.bestAction.TargetHex.GetPos() + new Vector3(0, agent.LocalScale / 2, 0);

        if (blackboard.bestAction.action == GameState.PlayerAction.Teleporting &&
            blackboard.bestAction.TargetHex.IsHidden() &&
            blackboard.bestAction.TargetHex != Treasure.Instance.GetHexGridObject())
        {
            Debug.Log("Teleporting to hidden hex. Pausing game. Action by: " + agent.name);
            EditorApplication.isPaused = true;
        }
    }

    protected override void OnStop()
    {
        blackboard.bestAction = null;
    }

    protected override State OnUpdate()
    {
        if (agent.Movement.DoSelectedAction(blackboard.bestAction.action, _targetPosition, agent.LocalScale))
        {
            blackboard.potentialActions.Clear();
            //I added this line to clear the potential actions after the action is executed for debugging. When it gets stuck executing, i can see the list.
            blackboard.previousObservation = HexData.ConvertHexGridObjectListToHexData(
                HexGridManager.Instance.HexGrid.GetGridObjectsInRange(agent.transform.position, agent.behaviourValues.ObservingRange));
            return State.Success;
        }

        return State.Running;
    }
}