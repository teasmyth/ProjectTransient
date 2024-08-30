using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Blackboard
{
    public Enemy agent;
    public HexData[,] HexDataBase;
    public List<HexGridManager.ColorType> possibleTreasureColors = new();
    public HexGridManager.ColorType treasureColor = HexGridManager.ColorType.Null;
    public List<HexData> previousObservation = new();
    public HexGridManager.ColorType previousAgentColor = HexGridManager.ColorType.Null;
    
    public List<ActionEvaluation> potentialActions = new List<ActionEvaluation>();
    public ActionEvaluation bestAction;
}

[System.Serializable]
public class ActionEvaluation
{
    public bool riskyAction;
    public HexGridObject TargetHex;
    public GameState.PlayerAction action;
    public int actionPotential;
    
    [SerializeField] GameObject targetObject; //this is for debug purpose to see the target hex in the editor.
    
    //super high potential value to make sure it is chosen
    public const int InfinitePotential = 1000000000;

    public enum EvaluationOrigin
    {
        Evaluation,
        PreEvaluation,
        RiskyAction
    }
    
    public EvaluationOrigin origin;
   

    public ActionEvaluation(HexGridObject targetHex, GameState.PlayerAction action, int potential, EvaluationOrigin origin)
    {
        TargetHex = targetHex;
        this.action = action;
        actionPotential = potential;
        this.origin = origin;
        targetObject = targetHex.gameObject;
    }

    public ActionEvaluation(HexGridObject targetHex, bool riskyAction)
    {
        this.TargetHex = targetHex;
        this.riskyAction = riskyAction;
        targetObject = targetHex.gameObject;
        origin = EvaluationOrigin.RiskyAction;
    }

    public static bool IsRiskyAction(HexGridObject targetHex, List<ActionEvaluation> potentialActions)
    {
        return potentialActions.Any(n => n.TargetHex == targetHex && n.riskyAction );
    }
}