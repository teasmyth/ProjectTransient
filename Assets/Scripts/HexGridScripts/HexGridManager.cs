using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class HexGridManager : GameState
{
    public static HexGridManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("A new instance of HexGridManager is being created, destroying the old one");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    public HexGrid<HexGridObject> HexGrid { get; private set; }
    public float GetScale() => cellSize;

    [SerializeField] private int width;
    [SerializeField] private int height;

    [SerializeField] private int negativeYpos;
    [SerializeField] private int colorsToHide;
    [SerializeField] private float cellSize;
    [SerializeField] private float animationDuration;
    [SerializeField] private Vector3 originPos;
    [SerializeField] private GameObject tile;

    [SerializeField] private Color defaultSelectionColor;
    [SerializeField] private Color jumpSelectionColor;
    [SerializeField] private Color dashSelectionColor;
    [SerializeField] private Color teleportSelectionColor;
    [SerializeField] private Color notAllowedSelectionColor;

    [SerializeField] private GameObject backgroundBlockingPlane;


    public enum ColorType
    {
        Null = 0,
        Red = 1,
        Blue = 2,
        Yellow = 3,
        Cyan = 4,
        Black = 5,
        Magenta = 6,
        Green = 7,
        Grey = 8,
    }

    public enum SelectionMode
    {
        Movement = 0,
        Dash = 1,
        Teleport = 2,
        Jump = 3,
        None = 4,
        NotAllowed = 5
    };

    public SelectionMode selectionMode = SelectionMode.None;

    private List<ColorType> _usedColors = new();
    public ReadOnlyCollection<ColorType> UsedColors => _usedColors.AsReadOnly();
    [SerializeField] private List<ColorType> _prevShownColors = new List<ColorType>();
    public int GetShownColorCount() => _prevShownColors.Count;


    public HexGridObject[,] GridArray { get; private set; }

    private List<MoveHexData> _objectsToMove = new();
    private HexGridObject _previousSelectedHex;
    private float _animationTimer = 0;
    [SerializeField] private List<ColorType> _hiddenColors = new();
    public int GetHiddenColorCount() => _hiddenColors.Count;

    private class MoveHexData
    {
        public readonly HexGridObject Hex;
        public readonly bool MoveUp;

        public Vector3 Position
        {
            get => Hex.defaultRenderer.transform.localPosition;
            set => Hex.defaultRenderer.transform.localPosition = value;
        }

        public MoveHexData(HexGridObject hex, bool moveUp)
        {
            Hex = hex;
            MoveUp = moveUp;
        }

        public static List<MoveHexData> ConvertListToMoveHexData(List<HexGridObject> hexes, bool moveUp)
        {
            List<MoveHexData> moveHexData = new List<MoveHexData>();
            foreach (var hex in hexes)
            {
                moveHexData.Add(new MoveHexData(hex, moveUp));
            }

            return moveHexData;
        }
    }


    private void OnDestroy()
    {
        HexGrid = null;
    }

    public override void Enter(StateMachine stateMachine)
    {
        GameManager.Instance.IncrementTurnCount();
        TurnIsDone = false;

        _objectsToMove = CheckWhichObjectsToMove();
        HexGrid.TriggerDeselectAll();
    }

    private void Update()
    {
        if (TutorialManager.Instance != null && !TutorialManager.Instance.CanPlayerMove()) return;

        if (HexGrid != null && Player.Instance != null) HighlightHex();
    }


    public override void Execute(StateMachine stateMachine)
    {
        //_gridMovingDone = MoveGridYAxis(_objectsToMove);
        if (GameManager.Instance.GameRuleset.GetColorsToHide() == 0)
        {
            TurnIsDone = true;
            return;
        }

        TurnIsDone = MoveGridYAxis(_objectsToMove);

        if (TurnIsDone)
        {
            TurnDone();
        }
    }

    public override void Exit(StateMachine stateMachine)
    {
    }

    public override void SetupState(StateMachine stateMachine)
    {
        base.SetupState(stateMachine);
        width = GameManager.Instance.GameRuleset.GetGridWidth();
        height = GameManager.Instance.GameRuleset.GetGridHeight();
        colorsToHide = GameManager.Instance.GameRuleset.GetColorsToHide();

        HudManager.Instance.SetCameraTo3D(width);
       

        SetUpGrid();
        _objectsToMove = MoveHexData.ConvertListToMoveHexData(Extensions.TwoDimArrayToList(GridArray), true);
        HexGrid.OnSelectionModeSet += SetSelectionMode;
        HudManager.Instance.SetupNotes();
    }

    public override string GetStateDetails()
    {
        string txt = "HexGrid State";
        txt += "\n Width: " + width + "\n";
        txt += "Height: " + height + "\n";
        txt += "Colors to Hide: " + colorsToHide + "\n";
        txt += "Selection Mode: " + selectionMode + "\n";
        txt += "Used Colors: " + _usedColors + "\n";
        txt += "Previous Shown Colors: " + _prevShownColors + "\n";
        txt += "Hidden Colors: " + _hiddenColors + "\n";
        txt += "Grid Array: " + GridArray + "\n";
        txt += "Objects to Move: " + _objectsToMove + "\n";
        txt += "Previous Selected Hex: " + _previousSelectedHex + "\n";
        txt += "Animation Timer: " + _animationTimer + "\n";
        txt += "Hidden Colors Count: " + GetHiddenColorCount() + "\n";
        txt += "Shown Colors Count: " + GetShownColorCount() + "\n";

        return txt;
    }

    private void SetUpGrid()
    {
        GameObject[,] hexes = new GameObject[width, height];
        for (var x = 0; x < width; x++)
        {
            for (var z = 0; z < height; z++)
            {
                hexes[x, z] = Instantiate(tile, transform);
            }
        }

        AdjustGridForAspectRatio();

        HexGrid = new HexGrid<HexGridObject>(width, height, cellSize, originPos, hexes, ref _usedColors,
            (g, go, x, z) => new HexGridObject(g, go, x, z), out HexGridObject[,] gridArrayOut);

        GridArray = gridArrayOut;

        var hexList = Extensions.TwoDimArrayToList(GridArray);

        foreach (var hex in hexList)
        {
            var neighbors = HexGrid.GetNeighborGridObjects(hex.GetPos());
            hex.SetNeighbors(neighbors);
        }
    }

    private void AdjustGridForAspectRatio()
    {
        var blockingPlane = Instantiate(backgroundBlockingPlane);
        // Check if the screen has a 16:9 aspect ratio
        if (Math.Abs((float)Screen.width / Screen.height - 16f / 9f) > 0.5f)
        {
            // If the screen does not have a 16:9 aspect ratio, calculate the middle of the screen in world coordinates
            // Calculate the middle of the screen in screen coordinates
            Vector3 screenMiddle = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            // Convert the middle of the screen in screen coordinates to world coordinates
            Vector3 worldMiddle = Camera.main.ScreenToWorldPoint(screenMiddle);

            cellSize = cellSize / (16f / 9f) * ((float)Screen.width / Screen.height);

            Vector3 newMiddle = new Vector3(Mathf.Abs(worldMiddle.x / 2), cellSize, Mathf.Abs(worldMiddle.z / 2));

            if (Screen.width > Screen.height)
            {
                newMiddle.z = 0;
            }
            else
            {
                //No science behind this, just completely unrelated things that happened to make a nice adjustment.
                newMiddle.x += (Screen.height - Screen.height % Screen.width) / Screen.width * cellSize;
                blockingPlane.transform.position = new Vector3(0, -cellSize, 0);
            }

            transform.position = originPos = Screen.width > Screen.height ? -newMiddle : newMiddle;
        }

        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            transform.position = originPos += new Vector3(0, 0, 0.6f);
        }
    }

    public void HighlightHex()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            HexGridObject hex = HexGrid.GetGridObject(hit.point);
            if (hex == null || _previousSelectedHex == hex) return;

            if (_previousSelectedHex != null)
            {
                _previousSelectedHex.Deselect();
            }

            _previousSelectedHex = hex;
            if (Player.Instance.IsHexAllowed(hex))
            {
                hex.OverrideHighlight(selectionMode);
            }
            else
            {
                hex.OverrideHighlight(SelectionMode.NotAllowed);
            }
        }
    }


    private bool MoveGridYAxis(List<MoveHexData> hexes)
    {
        if (hexes.Count == 0)
        {
            return true;
        }

        _animationTimer += Time.deltaTime;

        for (var x = 0; x < hexes.Count; x++)
        {
            if (hexes[x].MoveUp && hexes[x].Hex.IsHidden())
            {
                hexes[x].Hex.SetHidden(false);
            }

            float target = hexes[x].MoveUp ? 0 : negativeYpos;

            var tilePos = hexes[x].Position;
            var targetPos = new Vector3(tilePos.x, tilePos.y, target);

            hexes[x].Position = Vector3.Lerp(tilePos, targetPos, _animationTimer / animationDuration);

            if (Math.Abs(target - hexes[x].Position.z) < 0.01)
            {
                hexes[x].Position = new Vector3(tilePos.x, tilePos.y, target);

                if (!hexes[x].MoveUp)
                {
                    hexes[x].Hex.SetHidden(true);
                }

                if (x != hexes.Count - 1) continue;

                _animationTimer = 0;
                return true;
            }
        }

        return false;
    }

    private List<ColorType> PickColorsToShow()
    {
        ColorType playerPrevColor = PlayerManager.Instance.ActivePlayer.PrevPos.GetColorType(); //Color 1
        ColorType playerCurrentColor = PlayerManager.Instance.ActivePlayer.GetHexGridObject().GetColorType(); //Color 2
        ColorType treasureColor = Treasure.Instance.ColorType; //Color 3

        List<ColorType> colorsToShow = new List<ColorType>(UsedColors);
        int colorsToRemove = colorsToHide;

        bool doesAbilityPreventFall = PlayerManager.Instance.ActivePlayer.PlayerActionType switch
        {
            PlayerAction.Dashing => GameManager.Instance.GameRuleset.GetDashPreventsFall(),
            PlayerAction.Jumping => GameManager.Instance.GameRuleset.GetJumpPreventsFall(),
            PlayerAction.Teleporting => GameManager.Instance.GameRuleset.GetTeleportPreventsFall(),
            PlayerAction.Moving => false,
            _ => false
        };

        bool playerNewTileStaysUp = true;
        bool treasureStaysUp = true;


        if (PlayerManager.Instance.Multiplayer)
        {
            ColorType opponentColor = PlayerManager.Instance.InactivePlayer.GetHexGridObject().GetColorType();

            if (playerCurrentColor == opponentColor)
            {
                goto RemoveColors;
                //We skip the whole thing as your opponent is on the same color as you.
                //You shold not be able to take 'ground' from your opponent.
            }
        }

        //I wrote redundant bool sets for code readability.

        if (playerPrevColor == playerCurrentColor && playerPrevColor == treasureColor)
        {
            if (doesAbilityPreventFall)
            {
                playerNewTileStaysUp = true;
                treasureStaysUp = false; //they are the same, only one needed;
                colorsToShow.Remove(playerPrevColor);
            }
            else
            {
                playerNewTileStaysUp = false;
                treasureStaysUp = false;
                colorsToShow.Remove(playerPrevColor);
            }
        }
        else if (playerPrevColor != playerCurrentColor && playerPrevColor == treasureColor)
        {
            playerNewTileStaysUp = true;
            treasureStaysUp = false;
            colorsToShow.Remove(playerPrevColor);
            colorsToShow.Remove(playerCurrentColor);
            colorsToRemove--;
        }
        else if (playerPrevColor == playerCurrentColor && playerPrevColor != treasureColor)
        {
            if (doesAbilityPreventFall)
            {
                playerNewTileStaysUp = true;
                colorsToShow.Remove(playerPrevColor);
            }
            else
            {
                playerNewTileStaysUp = false;
                colorsToShow.Remove(playerPrevColor);
            }

            treasureStaysUp = true;
            colorsToShow.Remove(treasureColor);
        }
        else if (playerCurrentColor == treasureColor && playerCurrentColor != playerPrevColor)
        {
            playerNewTileStaysUp = true;
            treasureStaysUp = false;
            colorsToShow.Remove(playerCurrentColor);
            colorsToShow.Remove(playerPrevColor);
            colorsToRemove--;
        }
        else
        {
            playerNewTileStaysUp = true;
            treasureStaysUp = true;
            colorsToShow.Remove(playerPrevColor);
            colorsToShow.Remove(playerCurrentColor);
            colorsToShow.Remove(treasureColor);
            colorsToRemove--;
        }

        RemoveColors:
        if (PlayerManager.Instance.Multiplayer)
        {
            colorsToShow.Remove(PlayerManager.Instance.InactivePlayer.GetHexGridObject().GetColorType());
        }

        for (int i = 0; i < colorsToRemove; i++)
        {
            colorsToShow.Remove(colorsToShow[UnityEngine.Random.Range(0, colorsToShow.Count)]);
        }

        if (playerNewTileStaysUp) colorsToShow.Add(playerCurrentColor);
        if (treasureStaysUp && !colorsToShow.Contains(treasureColor)) colorsToShow.Add(treasureColor);

        if (PlayerManager.Instance.Multiplayer)
        {
            colorsToShow.Add(PlayerManager.Instance.InactivePlayer.GetHexGridObject().GetColorType());
        }

        _hiddenColors.Clear();
        foreach (var c in UsedColors)
        {
            if (colorsToShow.Contains(c)) continue;

            _hiddenColors.Add(c);
        }

        return colorsToShow;
    }

    private List<MoveHexData> CheckWhichObjectsToMove()
    {
        var colorsToShow = PickColorsToShow();

        while (_prevShownColors != null && !ColorsAreUnique(colorsToShow, _prevShownColors))
        {
            colorsToShow = PickColorsToShow();
        }

        _prevShownColors = colorsToShow;

        List<MoveHexData> toMove = new();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (GridArray[x, z] == Treasure.Instance.GetHexGridObject())
                {
                    toMove.Insert(0, new MoveHexData(GridArray[x, z], false));
                    //gridArray[x, z].SetHidden(true);
                    continue;
                }

                if (!colorsToShow.Contains(GridArray[x, z].GetColorType()))
                {
                    GridArray[x, z].ShowColor(false); //sometimes bugs out, but it's not a big deal.
                    if (GridArray[x, z].IsHidden()) continue;

                    toMove.Add(new MoveHexData(GridArray[x, z], false));
                    //gridArray[x, z].SetHidden(true);
                }
                else
                {
                    if (!GridArray[x, z].IsHidden()) continue;

                    toMove.Add(new MoveHexData(GridArray[x, z], true));
                    //gridArray[x, z].SetHidden(false);
                }
            }
        }

        return toMove;
    }

    public static Color AssignColor(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Red => Color.red,
            ColorType.Blue => Color.blue,
            ColorType.Yellow => Color.yellow,
            ColorType.Cyan => Color.cyan,
            ColorType.Black => Color.black,
            ColorType.Magenta => Color.magenta,
            ColorType.Green => Color.green,
            ColorType.Grey => Color.grey,
            _ => Color.white,
        };
    }

    public Color HighlightColor(SelectionMode mode)
    {
        return mode switch
        {
            SelectionMode.Movement => defaultSelectionColor,
            SelectionMode.Dash => dashSelectionColor,
            SelectionMode.Teleport => teleportSelectionColor,
            SelectionMode.Jump => jumpSelectionColor,
            SelectionMode.NotAllowed => notAllowedSelectionColor,
            SelectionMode.None => Color.black,
            _ => Color.black,
        };
    }

    private void SetSelectionMode(SelectionMode mode)
    {
        selectionMode = mode;
    }

    public Vector3 GetMiddlePos()
    {
        return GridArray[width / 2 - 1, height / 2 - 1].GetPos();
    }

    public Vector3 GetBottomLeftPos()
    {
        return GridArray[0, 0].GetPos();
    }

    public Vector3 GetTopRightPos()
    {
        return GridArray[width - 1, height - 1].GetPos();
    }

    private bool ColorsAreUnique(IEnumerable<ColorType> colorsToCheck, ICollection<ColorType> prevColors)
    {
        return colorsToCheck.Any(color => !prevColors.Contains(color));
    }

    public bool DebugIfEveryHexArrivedCorrectly()
    {
        bool everythingCorrect = true;
        for (var x = 0; x < width; x++)
        {
            for (var z = 0; z < height; z++)
            {
                if (GridArray[x, z].defaultRenderer.transform.localPosition.z == 0 ||
                    Math.Abs(GridArray[x, z].defaultRenderer.transform.localPosition.z - negativeYpos) < 0.01f)
                {
                    continue;
                }

                everythingCorrect = false;
                break;
            }

            if (!everythingCorrect) break;
        }

        return everythingCorrect;
    }
}