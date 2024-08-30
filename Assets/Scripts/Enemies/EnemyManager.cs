using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyManager : GameState
{
    [SerializeField] private bool spawnEnemies;
    
    private static EnemyManager _instance;
    public static EnemyManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else _instance = this;
    }

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyAutoSpawnTurnNumber;
    
  
    
    public override void Enter(StateMachine stateMachine)
    {
       TurnDone();

    }

    public override void Execute(StateMachine stateMachine)
    {
       
    }


    public override void Exit(StateMachine stateMachine)
    {
        TurnIsDone = false;
        //new WaitForSeconds(1);
    }

    public override void SetupState(StateMachine stateMachine)
    {
        
    }

    public override string GetStateDetails()
    {
        return "";
    }

    private void RandomXZ(out int x, out int z)
    {
        x = Random.Range(0, HexGridManager.HexGrid.GetWidth());
        z = Random.Range(0, HexGridManager.HexGrid.GetHeight());
    }

}
