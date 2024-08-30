using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckJumping : ActionNode
{
    protected override void OnStart()
    {
        var potentialJumps = agent.Movement.SetJumpTargets(false);
        
        if (potentialJumps.Count == 0)
        {
            return;
        }
        
        var basePotential = agent.behaviourValues.JumpBasePotential;
        var weight = agent.behaviourValues.JumpWeight;
        var exploration = agent.behaviourValues.Exploration;
        var attackWeight = agent.behaviourValues.AttackWeight;
        var aggression = agent.behaviourValues.Aggression;
        var riskTaking = agent.behaviourValues.RiskTaking;
        var canJumpFail = !GameManager.Instance.GameRuleset.GetJumpPreventsFall();
        
        foreach (var jump in potentialJumps)
        {
            if (!GameManager.Instance.SinglePlayerMode)
            {
                if (jump == PlayerManager.Instance.InactivePlayer.GetHexGridObject())
                {
                    blackboard.potentialActions.Add(new ActionEvaluation(jump, GameState.PlayerAction.Dashing,
                        ActionEvaluation.InfinitePotential, ActionEvaluation.EvaluationOrigin.PreEvaluation));
                    return;
                }
            }
            
            
            if (ActionEvaluation.IsRiskyAction(jump, blackboard.potentialActions))
            {
                continue;
            }
            
            var jumpIsKnown = HexData.ContainsHex(blackboard.HexDataBase, jump);
            
            if (canJumpFail && (!jumpIsKnown || jump.GetColorType() == agent.GetHexGridObject().GetColorType()))
            {
                continue;
            }
            
            int potential = basePotential;
            
           agent.ActionPotentialEvaluation(jump, canJumpFail, ref potential, weight, exploration, attackWeight,
                aggression, riskTaking);
            
            blackboard.potentialActions.Add(new ActionEvaluation(jump, GameState.PlayerAction.Jumping, potential, ActionEvaluation.EvaluationOrigin.Evaluation));
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
