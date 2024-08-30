using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance { get; private set; }

    public GameObject endScreen;
    public TextMeshProUGUI endScreenText;
    public TextMeshProUGUI selectedActionText;
    public TextMeshProUGUI debugText;
    public GameObject debugPanel;
    public GameObject notesPanel;
    public GameObject notePrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else Instance = this;
    }

    private bool twoD_Cam;
    private Vector3 camPos;

    [SerializeField] TextMeshProUGUI turnCount;

    // Start is called before the first frame update
   

    // Update is called once per frame
    void Update()
    {
        turnCount.text = GameManager.Instance != null ? "Turn: " + GameManager.Instance.TurnCount : "Turn: 0";
        selectedActionText.text = Player.Instance != null ? " " + Player.Instance.PlayerActionType : "";
    }

    public void SetCameraTo3D(float width)
    {
        width = width % 2 == 1 ? width : width - 1;
        camPos = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(width / 2, width * 0.8f, camPos.z);
        camPos = Camera.main.transform.position;
        twoD_Cam = false;
    }

    public void OnChangeCamera()
    {
        if (!twoD_Cam)
        {
            Camera.main.transform.position = new Vector3(camPos.x, camPos.x * 2, camPos.x * HexGrid<HexGridObject>.Hex_Vertical_Offset_Multiplier);
            Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
            twoD_Cam = true;
        }
        else
        {
            Camera.main.transform.position = camPos;
            Camera.main.transform.rotation = Quaternion.Euler(60, 0, 0);
            twoD_Cam = false;
        }
    }

    public void OnClickMove()
    {
        if (Tutorial.TutorialManager.Instance != null && !Tutorial.TutorialManager.Instance.CanPlayerMove()) return;
        EventManager.Trigger_PlayerSwitchAction(Player.PlayerAction.Moving);
    }

    public void OnClickDash()
    {
        if (Tutorial.TutorialManager.Instance != null && !Tutorial.TutorialManager.Instance.CanPlayerMove()) return;
        EventManager.Trigger_PlayerSwitchAction(Player.PlayerAction.Dashing);
    }

    public void OnClickJump()
    {
        if (Tutorial.TutorialManager.Instance != null && !Tutorial.TutorialManager.Instance.CanPlayerMove()) return;
        EventManager.Trigger_PlayerSwitchAction(Player.PlayerAction.Jumping);
    }

    public void OnClickTeleport()
    {
        if (Tutorial.TutorialManager.Instance != null && !Tutorial.TutorialManager.Instance.CanPlayerMove()) return;
        EventManager.Trigger_PlayerSwitchAction(Player.PlayerAction.Teleporting);
    }

    public void OnClickPlayAgain() => SceneManager.LoadScene("MainMenu");
    
    public void OnGameEndResultText(string text)
    {
        endScreen.SetActive(true);
        endScreenText.text = text;
    }

    public void OnClickDebug()
    {
        Time.timeScale = 0;
        string txt = "Please a send me a screenshot of this screen if you encounter a bug. \n \n";
        txt += "Current State : " + GameManager.Instance.GetCurrentState() + "\n";
        txt += "Player Action : " + Player.Instance.PlayerActionType + "\n";
        txt += "Player Prev Color (doesn't work if it's player turn) : " + Player.Instance.PrevPos.GetColorType() + "\n";
        txt += "Player Current Color : " + Player.Instance.GetHexGridObject().GetColorType() + "\n";
        txt += "HexGrid: Everything is correctly moved: " + HexGridManager.Instance.DebugIfEveryHexArrivedCorrectly() + "\n";
        txt += "HexGrid: Shown Hexes: " + HexGridManager.Instance.GetShownColorCount() + "\n";
        txt += "HexGrid: Hidden Hexes: " + HexGridManager.Instance.GetHiddenColorCount() + "\n";

        txt += "\n Current State Details: \n";
        txt += GameManager.Instance.GetCurrentState().GetStateDetails();
        
        
        
        debugText.text = txt;
        
        debugPanel.SetActive(true);
    }

    public void OnCloseDebug()
    {
        Time.timeScale = 1;
        debugPanel.SetActive(false);
    }

    public void SetupNotes()
    {
        foreach (var color in HexGridManager.Instance.UsedColors)
        {
            var note = Instantiate(notePrefab, notesPanel.transform);
            note.GetComponentInChildren<TextMeshProUGUI>().text = color.ToString();
            note.GetComponentInChildren<TextMeshProUGUI>().color = HexGridManager.AssignColor(color);
        }
    }
}
