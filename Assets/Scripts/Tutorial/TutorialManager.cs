using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


namespace Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }


        [SerializeField] private GameObject jumpAbility;
        [SerializeField] private GameObject dashAbility;
        [SerializeField] private GameObject teleportAbility;
        [SerializeField] private TextMeshProUGUI tutorialText;

        [SerializeField] private GameObject contButton;
        [SerializeField] private GameObject backButton;

        private int _currentTextIndex;
        private List<string> _previousTexts = new();

        private bool _playerCanMove = false;
        private bool _playerClick = false;

        public bool CanPlayerMove() => _playerCanMove;

        private int _waitForTurns = 0;
        private int _turnCounter = 0;
        private bool _wait = false;
        private bool _skipWaiting = false;

        private bool _playerCanJump = false;
        private bool _playerCanDash = false;
        private bool _playerCanTp = false;
        private bool _tutorialOver = false;


        private void Awake()
        {
            if (Instance != null & Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                Player.OnPlayerTurnStart += Execute;
                Player.OnPlayerClick += PlayerClick;
            }
        }

        public enum TutorialState
        {
            Start,
            Grid,
            Jump,
            Dash,
            Treasure,
            Teleport,
            End
        }

        [SerializeField] private TutorialState tutorialState = TutorialState.Start;
        private int _stateIndex;

        private void Execute()
        {
            if (_tutorialOver)
            {
                return;
            }

            if (_playerClick)
            {
                return;
            }

            if (!CheckState())
            {
                return;
            }

            _playerCanMove = false;
            Player.Instance.SetPlayerAction(Player.PlayerAction.Waiting);


            _currentTextIndex = 0;
            var firstText = GetNextText(_currentTextIndex);
            tutorialText.text = firstText;
            _previousTexts.Add(firstText);
        }

        //returns true if the tutorial panel should popup again.
        private bool CheckState()
        {
            switch (tutorialState)
            {
                case TutorialState.Start:
                    break;
                case TutorialState.Grid:
                {
                    if (!_wait)
                    {
                        _wait = true;
                        _playerCanMove = true;
                        return false;
                    }

                    break;
                }
                case TutorialState.Jump:
                {
                    if (!_wait)
                    {
                        _wait = true;
                        _playerCanMove = true;
                        return false;
                    }

                    if (Player.Instance.Movement.SetJumpTargets(false).Count == 0)
                    {
                        _skipWaiting = true;
                        return false;
                    }

                    jumpAbility.SetActive(true);
                    _playerCanJump = true;
                    break;
                }
                case TutorialState.Dash:
                {
                    if (!_wait)
                    {
                        _wait = true;
                        _playerCanMove = true;
                        return false;
                    }

                    if (Player.Instance.Movement.SetDashTargets(false).Count == 0)
                    {
                        return false;
                    }

                    dashAbility.SetActive(true);
                    _playerCanDash = true;
                    break;
                }

                case TutorialState.Treasure:
                {
                    if (!_wait && !_skipWaiting)
                    {
                        _waitForTurns = 1;
                        _wait = true;
                    }
                    
                    if (_wait && _turnCounter <= _waitForTurns)
                    {
                        _wait = false;
                        _turnCounter++;
                        _playerCanMove = true;
                        return false;
                    }
                }
                    break;
                
                case TutorialState.Teleport:
                {
                    if (!_wait && !_skipWaiting)
                    {
                        _waitForTurns = 2;
                        _wait = true;
                    }
                    
                    if (_wait && _turnCounter <= _waitForTurns)
                    {
                        _wait = false;
                        _turnCounter++;
                        _playerCanMove = true;
                        return false;
                    }
                    
                    teleportAbility.SetActive(true);
                    _playerCanTp = true;
                }
                    break;
                
                case TutorialState.End:
                {
                    if (!_wait)
                    {
                        _wait = true;
                        _playerCanMove = true;
                        return false;
                    }

                    _tutorialOver = true;
                }
                    break;
            }

            gameObject.SetActive(true);
            _skipWaiting = false;
            _turnCounter = 0;
            _wait = false;

            return true;
        }

        private string GetNextText(int textIndex)
        {
            return tutorialState switch
            {
                TutorialState.Start => StartTutorial(textIndex),
                TutorialState.Grid => GridTutorial(textIndex),
                TutorialState.Jump => JumpTutorial(textIndex),
                TutorialState.Dash => DashTutorial(textIndex),
                TutorialState.Teleport => TeleportTutorial(textIndex),
                TutorialState.Treasure => TreasureTutorial(textIndex),
                TutorialState.End => EndTutorial(textIndex),
                _ => ""
            };
        }


        private string StartTutorial(int textIndex)
        {
            string txt = "";
            switch (textIndex)
            {
                case 0:
                {
                    txt = "Welcome to the Transient's tutorial! \n";
                    txt += "This tutorial will show you how to play the game. \n";
                    txt += "The goal of the game is to find the hidden treasure on the map. \n";
                }
                    break;
                case 1:
                {
                    txt +=
                        "The grid is made up of hexagons. In the Tutorial ruleset, the map is made up of 6 x 6 hexagons. " +
                        "The map's width/height also indicates the types of colors in the map. In this case it is 6. ";
                    txt +=
                        "The colors are assigned and evenly distributed to the hexagons, meaning there are 6 of each color. ";
                    txt +=
                        $"The colors are: {(HexGridManager.ColorType)1}, {(HexGridManager.ColorType)2}, {(HexGridManager.ColorType)3}," +
                        $" {(HexGridManager.ColorType)4}, {(HexGridManager.ColorType)5} and {(HexGridManager.ColorType)6}. \n";
                    // txt += "You can see the colors of hexagons adjacent to you and the one you are standing on. \n";
                    break;
                }
                case 2:
                {
                    txt +=
                        "You can see the colors of hexagons adjacent to you and the one you are standing on and move to any adjacent hexagon. You cannot move to hidden hexagons. \n";
                    txt +=
                        "Every time you make an action, the grid will react to it and move certain hexagons down. \n";
                }
                    break;
                case 3:
                {
                    txt +=
                        $"In the current ruleset, the grid moves down {GameManager.Instance.GameRuleset.GetColorsToHide()} color types. One is RANDOMIZED, " +
                        "the other one is always guaranteed to be the PREVIOUS COLOR you were on. \n";
                    txt += "Regardless of ruleset, your PREVIOUS COLOR always moves down and the rest are RANDOMIZED.";
                }
                    break;
                case 4:
                {
                    txt +=
                        $"You are on a {Player.Instance.GetHexGridObject().GetColorType()} hexagon. Once you move, " +
                        $"all the {Player.Instance.GetHexGridObject().GetColorType()} type hexagons will go down, alongside the other RANDOMIZED color.";
                    txt +=
                        "The hexagons that are valid moves will be highlighted with green while the invalid ones will be red. \n";
                    txt += "Try moving around for a bit!";
                }
                    break;
            }

            return txt;
        }

        private string GridTutorial(int textIndex)
        {
            string txt = "";
            switch (textIndex)
            {
                case 0:
                {
                    txt += "New colors are picked again every turn with the rules in mind.\n";
                    txt += "While the grid can randomly re-pick the same colors, it will always move down at least one new color. This can be your PREVIOUS COLOR only as well.";
                }
                    break;
                case 1:
                {
                    txt += "Be mindful of where you are stepping and plan your moves accordingly. \n";
                    txt += "Remembering the colors of the hexes is key to finding the treasure.";
                }
                    break;
            }

            return txt;
        }

        private string JumpTutorial(int textIndex)
        {
            string txt = "";
            switch (textIndex)
            {
                case 0:
                {
                    txt +=
                        "In Transient you have 3 abilities. Jumping, Dashing and Teleporting. Let's introduce Jumping. \n";
                    txt +=
                        "Jumping allows you to jump over a hexagon, if there's a gap in-between. The hex's color you land on will NOT count towards PREVIOUS COLOR.";
                    txt += " So, even without knowing the color of the hex you are jumping to, it is safe.";
                }
                    break;
                case 1:
                {
                    txt += "So if you are in a tight situation, Jumping can be a big help. \n";
                    txt += "You can find your abilities on top left corner. Try Jumping!";
                }
                    break;
            }


            return txt;
        }

        private string DashTutorial(int textIndex)
        {
            string txt = "";
            switch (textIndex)
            {
                case 0:
                {
                    txt += "Dashing allows you to move to any hexagon in a straight line. \n";
                    txt += "In the Tutorial ruleset, your final destination's color will not count towards PREVIOUS COLOR. \n";
                    txt += "However, this can vary depending on the ruleset, indicated in the Main Menu.";
                }
                    break;
                case 1:
                {
                    txt += "You cannot dash to the hexagons adjacent to you. \n";
                    txt += "Dashing can be useful to move to a specific color, to avoid a color, or explore faster. \n";
                    txt += "You can find your abilities on top left corner. Try Dashing!";
                }
                    break;
            }

            return txt;
        }

        private string TreasureTutorial(int textIndex)
        {
            string txt = "";
            switch (textIndex)
            {
                case 0:
                {
                    txt +=
                        "Before the last ability is introduced, let's go back to the goal of the game: finding the hidden Treasure.";
                    txt +=
                        " In the beginning of the game, the Treasure picks a color, for example, Blue, and MOVES once between Blue hexes each turn. \n";
                    txt +=
                        "The Treasure will never change its color. The Treasure can also decide to stay in the same hexagon.";
                }
                    break;
                case 1:
                {
                    txt +=
                        "What's special about the Treasure's color, is that the hexes of this color be a RANDOMIZED color. \n";
                    txt +=
                        "As state before, in the current game, the map picks 2 colors to move down. One is your PREVIOUS COLOR the other is RANDOMIZED.";
                    txt += " The Treasure's color can never be one of the RANDOMIZED color.";
                    
                   
                }
                    break;
                case 2:
                {
                    txt +=
                        "However, there's a catch. If you step on the Treasure's color, it WILL count towards the PREVIOUS COLOR. \n";
                    txt +=
                        "This means that the Treasure's color will also move down next turn for one turn, then comes back up.";
                    txt += " So, the Treasure-colored hexes will go down if you step one of them PREVIOUSLY but cannot go down RANDOMLY.";

                }
                    break;
                case 3:
                    
                    txt += " For clarity, if you use an ability that does not count towards the PREVIOUS COLOR (such as Jump) which happens to be the Treasure's color, the Treasure's color will not move down.";
                    break;
                case 4:
                {
                    Treasure.Instance.GetComponent<MeshRenderer>().enabled = true;
                    txt +=
                        "How do you find the Treasure? The only exception to the Treasure's colored hexes is the actual hexagon the Treasure is on. \n";
                    txt += "Here's a sneak peek of the Treasure; it's the Green Ball on the map. \n";
                    txt +=
                        "So, assuming you did not step on that color previously, if the Treasure is on a Blue hexagon, all the Blue hexagons will be up, except the one that the Treasure is on.";
                }
                    break;
                case 5:
                {
                    Treasure.Instance.GetComponent<MeshRenderer>().enabled = false;
                    txt +=
                        "Another way to think about this, is if you remember two hexagons with the same color, and both of them are down, it cannot be the Treasure's color. That is, if this was not the color you previously stepped on.";
                    txt +=
                        " This way, if you already figured out which color belongs to the Treasure, and if you have good memory and observation skills, you can guess where the Treasure is hiding.";
                }
                    break;
                case 6:
                    txt += "Play for a bit, and see if you can figure out the color of the Treasure.";
                    break;
            }

            return txt;
        }

        private string TeleportTutorial(int textIndex)
        {
            string txt = "";
            switch (textIndex)
            {
                case 0:
                {
                    txt +=
                        "The last ability is: Teleport. Teleporting allows you to move to any hexagon on the map, even if it is invisible.";
                    txt += " The only exception is adjacent hexagons, you cannot Teleport to adjacent hexagons.";
                    txt +=
                        " Depending on the ruleset, Teleporting to a color other than the one you are stepping on may end the game.\n";
                    txt += "For tutorial ruleset, this is not the case; you can freely Teleport to any tile.";
                }
                    break;
                case 1:
                {
                    txt +=
                        "Teleporting can be useful to move to a specific color, to avoid a color, or explore faster. \n";
                    txt += "But more importantly, Teleporting is key to winning the game. \n";
                    txt += "As mentioned previously, you can Teleport to a hex, even if it is empty. \n";
                    txt += "Moreover, we also know that the Treasure is on a tile that is empty.";
                }
                    break;
                case 2:
                {
                    txt +=
                        "To win the game, you must figure out which color belongs to the Treasure, and then Teleport to the empty tile of that color. \n";
                    txt += "If you know the color and the map well, you will know where the Treasure is. \n";
                    txt +=
                        "You must be sure of the location however, because Teleporting to an empty hex also makes you lose the game. \n";
                }
                    break;
                case 3:
                {
                    txt += "You can find your abilities on top left corner. Try Teleporting!";
                }
                    break;
            }

            return txt;
        }


        private string EndTutorial(int textIndex)
        {
            string txt = "";
            switch (textIndex)
            {
                case 0:
                {
                    txt += "Congratulations! You have completed the tutorial. \n";
                    txt += "You are now ready to play the game. \n";
                    txt += "Misstepping safeguards will be disabled, so watch your step! \n";
                }
                    break;

                case 1:
                {
                    txt +=
                        "If you want to replay the Tutorial ruleset without the text, select it in the Main Menu. \n";
                    txt += "Good luck!";
                }
                    break;
            }

            return txt;
        }

        public void OnClickContinue()
        {
            if (_playerClick)
            {
                gameObject.SetActive(false);
                _playerClick = false;
                return;
            }

            if (GetNextText(_currentTextIndex + 1).Equals(""))
            {
                gameObject.SetActive(false);
                backButton.SetActive(false);
                _playerCanMove = true;
                _stateIndex++;
                tutorialState = (TutorialState)_stateIndex;
                _previousTexts.Clear();
                EventManager.Trigger_PlayerSwitchAction(Player.PlayerAction.Moving);
                return;
            }


            if (_currentTextIndex == _previousTexts.Count - 1)
            {
                _previousTexts.Add(GetNextText(_currentTextIndex + 1));
            }

            _currentTextIndex++;
            tutorialText.text = _previousTexts[_currentTextIndex];

            if (!backButton.activeInHierarchy)
            {
                backButton.SetActive(true);
            }
        }

        public void OnClickBack()
        {
            if (_currentTextIndex - 1 < 0 && _previousTexts.Count <= 0) return;

            _currentTextIndex--;
            tutorialText.text = _previousTexts[_currentTextIndex];

            if (!contButton.activeInHierarchy)
            {
                contButton.SetActive(true);
            }
        }


        private bool PlayerClick(HexGridObject selectedHex)
        {
            if (_tutorialOver) return true;
            
            //Checking for dash state as it automatically moves to next after jumping text.
            if (tutorialState == TutorialState.Dash && _playerCanJump)
            {
                if (Player.Instance.PlayerActionType != Player.PlayerAction.Jumping)
                {
                    _playerClick = true;
                    gameObject.SetActive(true);
                    tutorialText.text = "Try Jumping!";
                    return false;
                }
                
                _playerCanJump = false;
                return true;
            }

            if (tutorialState == TutorialState.Treasure && _playerCanDash)
            {
                if (Player.Instance.PlayerActionType != Player.PlayerAction.Dashing)
                {
                    _playerClick = true;
                    gameObject.SetActive(true);
                    tutorialText.text = "Try Dashing!";
                    return false;
                }
                
                _playerCanDash = false;
                return true;
            }

            if (tutorialState == TutorialState.End && _playerCanTp)
            {
                if (Player.Instance.PlayerActionType != Player.PlayerAction.Teleporting)
                {
                    _playerClick = true;
                    gameObject.SetActive(true);
                    tutorialText.text = "Try Teleporting!";
                    return false;
                }

                _playerCanTp = false;
                return true;
            }

            if (Player.Instance.PrevPos != null && Player.Instance.PlayerActionType == Player.PlayerAction.Moving
                                                && Player.Instance.PrevPos.GetColorType() == selectedHex.GetColorType())
            {
                _playerClick = true;
                gameObject.SetActive(true);
                tutorialText.text = "You tried to step on a tile that will move down, try again.";
                return false;
            }

            return true;
        }
    }
}