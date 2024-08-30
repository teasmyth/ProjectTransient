using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckDash : ActionNode
{
    protected override void OnStart()
    {
        var potentialDashes = agent.Movement.SetDashTargets(false);
        
        if (potentialDashes.Count == 0)
        {
            return;
        }
        
        var basePotential = agent.behaviourValues.DashBasePotential;
        var weight = agent.behaviourValues.DashWeight;
        var exploration = agent.behaviourValues.Exploration;
        var attackWeight = agent.behaviourValues.AttackWeight;
        var aggression = agent.behaviourValues.Aggression;
        var riskTaking = agent.behaviourValues.RiskTaking;
        
        var canDashFail = !GameManager.Instance.GameRuleset.GetDashPreventsFall();
        
        
        foreach (var dash in potentialDashes)
        {
            if (!GameManager.Instance.SinglePlayerMode)
            {
                if (dash == PlayerManager.Instance.InactivePlayer.GetHexGridObject())
                {
                    blackboard.potentialActions.Add(new ActionEvaluation(dash, GameState.PlayerAction.Dashing,
                        ActionEvaluation.InfinitePotential, ActionEvaluation.EvaluationOrigin.PreEvaluation));
                    return;
                }
            }

            if (ActionEvaluation.IsRiskyAction(dash, blackboard.potentialActions))
            {
                continue;
            }
            
            var dashIsKnown = HexData.ContainsHex(blackboard.HexDataBase, dash);
            
            if (canDashFail && (!dashIsKnown || (dash.GetColorType() == agent.GetHexGridObject().GetColorType()) && !PlayerManager.Instance.Multiplayer))
            {
                continue;
            }
            
            //Is this distance multiplier necessary?
            int distanceMultiplier = Mathf.RoundToInt(HexGridManager.Instance.HexGrid.GetDistance(agent.transform.position, dash.GetPos()));
            int potential = basePotential + distanceMultiplier;
            
            agent.ActionPotentialEvaluation(dash, canDashFail, ref potential, weight, exploration, attackWeight,
                aggression, riskTaking);
            
            
            blackboard.potentialActions.Add(new ActionEvaluation(dash, GameState.PlayerAction.Dashing, potential, ActionEvaluation.EvaluationOrigin.Evaluation));
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
