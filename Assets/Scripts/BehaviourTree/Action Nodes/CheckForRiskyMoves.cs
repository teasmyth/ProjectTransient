using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckForRiskyMoves : ActionNode
{
    protected override void OnStart()
    {
        if (GameManager.Instance.SinglePlayerMode)
        {
            return;
        }

        var opponentDashList = PlayerMovement.GetAllPotentialDashTargets(PlayerManager.Instance.InactivePlayer
            .GetHexGridObject()
            .GetPos());
        var opponentNeighbors = PlayerManager.Instance.InactivePlayer.GetHexGridObject().Neighbors;
        var opponentJumpList = PlayerMovement.GetAllPotentialJumpTargets(PlayerManager.Instance.InactivePlayer
            .GetHexGridObject()
            .GetPos());


        foreach (var neighbor in opponentNeighbors)
        {
            //Manual dumbness because it is just super dumb to move next to the opponent.
            var dumbness = Random.Range(0, 1000);
            if (dumbness <= agent.behaviourValues.Dumbness)
            {
                continue;
            }

            blackboard.potentialActions.Add(new ActionEvaluation(neighbor, true));
        }

        foreach (var direction in opponentDashList)
        {
            if (direction.Count == 0)
            {
                continue;
            }
            
            var dumbness = Random.Range(0, 101);
            if (dumbness <= agent.behaviourValues.Dumbness)
            {
                continue;
            }

            int risk = 1;
            foreach (var dash in direction)
            {
                if (dash.IsHidden()) risk--;
                else risk++;

                if (risk <= agent.behaviourValues.RiskTaking)
                {
                    blackboard.potentialActions.Add(new ActionEvaluation(dash, true));
                }
            }
        }

        foreach (var jump in opponentJumpList)
        {
            //Manual dumbness because it is just super dumb to move next to the opponent.
            var dumbness = Random.Range(0, 1000);
            if (dumbness <= agent.behaviourValues.Dumbness)
            {
                continue;
            }

            blackboard.potentialActions.Add(new ActionEvaluation(jump, true));
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