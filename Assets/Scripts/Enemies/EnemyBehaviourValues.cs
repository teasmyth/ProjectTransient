using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyBehaviour_", menuName = "Game/Enemy Behaviour Settings", order = 0)]
public class EnemyBehaviourValues : ScriptableObject
{
   [SerializeField] private int memory = 30;
   [SerializeField] private int observingRange = 4;
   
   [Space(10)]
   [SerializeField, Range(0, MaxPotential)] private int movementBasePotential;
   [SerializeField, Range(0, MaxPotential)] private int movementWeight;
   [Space(10)]
   [SerializeField, Range(0, MaxPotential)] private int jumpBasePotential;
   [SerializeField, Range(0, MaxPotential)] private int jumpWeight;
   [Space(10)]

   [SerializeField, Range(0, MaxPotential)] private int dashBasePotential;
   [SerializeField, Range(0, MaxPotential)] private int dashWeight;
   [Space(10)]

   [SerializeField, Range(0, MaxPotential)] private int teleportBasePotential;
   [SerializeField, Range(0, MaxPotential)] private int teleportWeight;
   [Space(10)]
   
   [SerializeField, Range(0, MaxPotential)] private int attackWeight;
   [SerializeField, Range(1, MaxPotential)] private int aggression; //Multiplier, cannot be 0.
   [SerializeField, Range(0, MaxPotential)] private int riskTaking; 
   [SerializeField, Range(1, MaxPotential)] private int exploration; //Multiplier, cannot be 0.
   [SerializeField, Range (1, 100)] private int dumbness; //A percentage where the agent doesnt do the winning move.
   
   public const int MaxPotential = 10;
   
   public int Memory => memory;
   public int ObservingRange => observingRange;
   
   public int MovementBasePotential => movementBasePotential;
   public int MovementWeight => movementWeight;
   public int JumpBasePotential => jumpBasePotential;
   public int JumpWeight => jumpWeight;
   public int DashBasePotential => dashBasePotential;
   public int DashWeight => dashWeight;
   public int TeleportBasePotential => teleportBasePotential;
   public int TeleportWeight => teleportWeight;
   public int AttackWeight => attackWeight;
   public int Aggression => aggression;
   public int RiskTaking => riskTaking;
   public int Exploration => exploration;
   public int Dumbness => dumbness;
}
