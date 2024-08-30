using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EvaluateActions : ActionNode
{
    protected override void OnStart()
    {
        if (blackboard.potentialActions.Count == 0)
        {
            Debug.Log("No possible actions. Abort.");
            EditorApplication.isPlaying = false;
            return;
        }   
        
        ActionEvaluation tentativeBestAction = blackboard.potentialActions[0];
        List<ActionEvaluation> equalBestActions = new List<ActionEvaluation> ();

        foreach (var t in blackboard.potentialActions)
        {
            if (t.actionPotential > tentativeBestAction.actionPotential && !t.riskyAction)
            {
                tentativeBestAction = t;
            }
        }
        
        foreach (var t in blackboard.potentialActions)
        {
            if (t.actionPotential == tentativeBestAction.actionPotential && !t.riskyAction)
            {
                equalBestActions.Add(t);
            }
        }
        
        blackboard.bestAction = equalBestActions.Count > 1 ? equalBestActions[Random.Range(0, equalBestActions.Count)] : tentativeBestAction;
    }

    protected override void OnStop()
    {
        //blackboard.potentialActions.Clear();
    }

    protected override State OnUpdate()
    {
        return State.Success;
    }
}
