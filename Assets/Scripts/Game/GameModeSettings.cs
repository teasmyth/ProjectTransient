using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GM_", menuName = "Game/Game Mode Settings", order = 0)]
public class GameModeSettings : ScriptableObject
{
    public enum GameMode
    {
        SinglePlayer = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Lol = 4,
        CustomOnlyBot = 5,
        CustomBotVBot = 6
    }
    
    public GameMode gameMode;
    
}
