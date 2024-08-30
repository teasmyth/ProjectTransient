using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RS_", menuName = "Game/Game Ruleset Settings", order = 0)]
public class GameRulesetSettings : ScriptableObject
{
    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
        Lol = 3,
        Tutorial = 4
    }

    [SerializeField] private Difficulty difficulty;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private int colorsToHide;

    [SerializeField] private bool jumpPreventsFall;
    [SerializeField] private bool dashPreventsFall;
    [SerializeField] private bool teleportPreventsFall;
    [SerializeField] private bool successfulTpUnlocksColor;
    [SerializeField] private bool failedTpFailsGame;


    public Difficulty GetDifficulty() => difficulty;
    public int GetGridWidth() => gridWidth;
    public int GetGridHeight() => gridHeight;
    public int GetColorsToHide() => colorsToHide;
    public bool GetJumpPreventsFall() => jumpPreventsFall;
    public bool GetDashPreventsFall() => dashPreventsFall;
    public bool GetTeleportPreventsFall() => teleportPreventsFall;

    public bool GetFailedTpFailsGame() => failedTpFailsGame;

    public bool GetSuccessfulTpUnlocksColor() => successfulTpUnlocksColor;

    public GameRulesetSettings GetSettings() => this;
}