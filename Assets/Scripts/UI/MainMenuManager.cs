using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [SerializeField] private TMP_Dropdown gameRulesDropdown;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TextMeshProUGUI rulesetText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [SerializeField] private GameRulesetSettings[] rulesetDropDownSettings;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (rulesetDropDownSettings.Length == 0)
        {
            Debug.Log("Ruleset settings missing");
            return;
        }
        
        GameManager.Instance.SetGameRuleset(rulesetDropDownSettings[1]);
        DisplayRuleset(rulesetDropDownSettings[1]);
    }
    

    public void OnGameRulesDropdown()
    {
        if (gameRulesDropdown.value > rulesetDropDownSettings.Length) return;

        GameManager.Instance.SetGameRuleset(rulesetDropDownSettings[gameRulesDropdown.value]);
        DisplayRuleset(rulesetDropDownSettings[gameRulesDropdown.value]);
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGame" || scene.name == "Tutorial")
        {
            GameManager.Instance.SetupGame();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void OnClickStart()
    {
        SceneManager.LoadScene("MainGame");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnClickTutorial()
    {
        GameManager.Instance.SetGameRuleset(rulesetDropDownSettings[0]);
        SceneManager.LoadScene("Tutorial");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void DisplayRuleset(GameRulesetSettings ruleset)
    {
        string rulesetString = "Ruleset: " + ruleset.GetDifficulty() + "\n";
        rulesetString += "- The size of the grid is " + ruleset.GetGridWidth() + "x" + ruleset.GetGridHeight() + "\n";
        rulesetString +=
            $"- {ruleset.GetGridWidth() - ruleset.GetColorsToHide()} colors will be shown and {ruleset.GetColorsToHide()} will be hidden. \n";
        string willJumpCount = ruleset.GetJumpPreventsFall() ? "will not count" : "counts";
        rulesetString += $"- Jumping {willJumpCount} towards previous color. \n";
        string willDashCount = ruleset.GetDashPreventsFall() ? "will not count" : "counts";
        rulesetString += $"- Dashing {willDashCount} towards previous color. \n";
        string willFailTp = ruleset.GetFailedTpFailsGame() ? "ends" : "will not end";
        rulesetString += $"- Teleporting to a non-matching colored hex {willFailTp} the game. \n";
        string willUnlockColor = ruleset.GetSuccessfulTpUnlocksColor() ? "unlocks" : "will not unlock";
        rulesetString += $"- Successfully teleporting {willUnlockColor} the color of the hex teleported to. \n";

        rulesetText.text = rulesetString;
    }
}