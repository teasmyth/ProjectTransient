using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTeleport : ActionNode
{
    protected override void OnStart()
    {
        var possibleTeleports = agent.Movement.SetTeleportTargets();

        if (possibleTeleports.Count == 0)
        {
            return;
        }

        var basePotential = agent.behaviourValues.TeleportBasePotential;
        var weight = agent.behaviourValues.TeleportWeight;
        var exploration = agent.behaviourValues.Exploration;
        var attackWeight = agent.behaviourValues.AttackWeight;
        var aggression = agent.behaviourValues.Aggression;
        var riskTaking = agent.behaviourValues.RiskTaking;

        var canTeleportFail = !GameManager.Instance.GameRuleset.GetTeleportPreventsFall();
        var sameColorTPNecessary = GameManager.Instance.GameRuleset.GetFailedTpFailsGame();

        foreach (var teleport in possibleTeleports)
        {
            if (ActionEvaluation.IsRiskyAction(teleport, blackboard.potentialActions))
            {
                continue;
            }
            
            if (PlayerManager.Instance.Multiplayer && teleport == PlayerManager.Instance.InactivePlayer.GetHexGridObject())
            {
                continue;
            }

            var teleportIsKnown = HexData.ContainsHex(blackboard.HexDataBase, teleport);

            if (blackboard.treasureColor != HexGridManager.ColorType.Null && teleportIsKnown && teleport.IsHidden())
            {
                var data = Extensions.TwoDimArrayToList(blackboard.HexDataBase);
                HexData treasure = null;
                foreach (var hex in data)
                {
                    if (hex == null) continue;

                    if (hex.HexGridObject == Treasure.Instance.GetHexGridObject() && hex.IsHidden() && teleport == hex.HexGridObject)
                    {
                        treasure = hex;
                        break;
                    }
                }

                if (treasure != null)
                {
                    blackboard.potentialActions.Add(new ActionEvaluation(teleport, GameState.PlayerAction.Teleporting,
                        ActionEvaluation.InfinitePotential, ActionEvaluation.EvaluationOrigin.PreEvaluation));
                    return;
                }
            }


            if (teleport.IsHidden() || (canTeleportFail && (!teleportIsKnown || teleport.GetColorType() == agent.GetHexGridObject().GetColorType())) ||
                sameColorTPNecessary && (!teleportIsKnown || teleport.GetColorType() != agent.GetHexGridObject().GetColorType()))
            {
                continue;
            }


            int distanceMultiplier =
                Mathf.RoundToInt(
                    HexGridManager.Instance.HexGrid.GetDistance(agent.transform.position, teleport.GetPos()));
            int potential = basePotential + distanceMultiplier;

            agent.ActionPotentialEvaluation(teleport, canTeleportFail, ref potential, weight, exploration,
                attackWeight,
                aggression, riskTaking);


            blackboard.potentialActions.Add(new ActionEvaluation(teleport, GameState.PlayerAction.Teleporting,
                potential, ActionEvaluation.EvaluationOrigin.Evaluation));
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