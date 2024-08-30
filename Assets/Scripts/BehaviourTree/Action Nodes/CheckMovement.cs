using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckMovement : ActionNode
{
    protected override void OnStart()
    {
        var hexInRange = agent.GetHexGridObject().Neighbors;
        var possibleHexes = new List<HexGridObject>();
        foreach (var hex in hexInRange)
        {
            if (PlayerManager.Instance.Multiplayer && hex == PlayerManager.Instance.InactivePlayer.GetHexGridObject())
            {
                blackboard.potentialActions.Add(new ActionEvaluation(hex, GameState.PlayerAction.Moving,
                    ActionEvaluation.InfinitePotential, ActionEvaluation.EvaluationOrigin.PreEvaluation));
                return;
            }
            
            if (hex.IsHidden() || (hex.GetColorType() == agent.GetHexGridObject().GetColorType() && !PlayerManager.Instance.Multiplayer)) continue;
            possibleHexes.Add(hex);
        }

        if (possibleHexes.Count == 0)
        {
            return;
        }

        //TODO: Make it initialized in the beginning so it doenst have to be done every time and read memory
        var basePotential = agent.behaviourValues.MovementBasePotential;
        var weight = agent.behaviourValues.MovementWeight;
        var exploration = agent.behaviourValues.Exploration;
        var attackWeight = agent.behaviourValues.AttackWeight;
        var aggression = agent.behaviourValues.Aggression;
        var riskTaking = agent.behaviourValues.RiskTaking;

        foreach (var possibleMove in possibleHexes)
        {
            if (ActionEvaluation.IsRiskyAction(possibleMove, blackboard.potentialActions))
            {
                continue;
            }
            
            int potential = basePotential;
            
            agent.ActionPotentialEvaluation(possibleMove, false, ref potential, weight, exploration, attackWeight,
                aggression, riskTaking);
            
            blackboard.potentialActions.Add(new ActionEvaluation(possibleMove, GameState.PlayerAction.Moving, potential, ActionEvaluation.EvaluationOrigin.Evaluation));
        }
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        return State.Success;
    }
}