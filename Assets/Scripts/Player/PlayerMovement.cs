using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// The methods will return true if they are done.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementAnimationSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float parabolicSpeed = .5f;
    [SerializeField] private float jumpHeight;

    private HexGridManager hexGridManager;
    //private List<HexGridObject> _allowedMovementGridObjects = new();

    private void Start()
    {
        hexGridManager = HexGridManager.Instance;
    }

    public List<HexGridObject> SetMoveTargets()
    {
        List<HexGridObject> possibleHexes = HexGridManager.Instance.HexGrid.GetNeighborGridObjects(transform.position);
        List<HexGridObject> allowedHexes = new();
        foreach (var hex in possibleHexes)
        {
            if (hex.IsHidden()) continue;

            allowedHexes.Add(hex);
        }

        return allowedHexes;
    }

    public List<HexGridObject> SetDashTargets(bool highlight = true)
    {
        return GetDashTargets(transform.position, highlight);
    }

    public static List<HexGridObject> GetDashTargets(Vector3 pos, bool highlight = true)
    {
        var maxDistance = HexGridManager.Instance.HexGrid.GetWidth() >= HexGridManager.Instance.HexGrid.GetHeight()
            ? HexGridManager.Instance.HexGrid.GetWidth()
            : HexGridManager.Instance.HexGrid.GetHeight();

        List<List<HexGridObject>> allDashableHexes =
            HexGridManager.Instance.HexGrid.GetLineAllDirections(pos, maxDistance);
        List<HexGridObject> allowedHexes = new();

        for (int i = 0; i < allDashableHexes.Count; i++)
        {
            if (allDashableHexes[i].Count < 2) continue;
            for (int j = 0; j < allDashableHexes[i].Count; j++)
            {
                if (!allDashableHexes[i][j].IsHidden())
                {
                    if (j == 0)
                    {
                        if (highlight) allDashableHexes[i][j].SetSelectionMode(HexGridManager.SelectionMode.NotAllowed);
                        continue;
                    }

                    allowedHexes.Add(allDashableHexes[i][j]);
                    if (highlight) allowedHexes[^1].SetSelectionMode(HexGridManager.SelectionMode.Dash);
                }
                else break;
            }
        }


        return allowedHexes;
    }

    public static List<List<HexGridObject>> GetAllPotentialDashTargets(Vector3 pos)
    {
        var maxDistance = HexGridManager.Instance.HexGrid.GetWidth() >= HexGridManager.Instance.HexGrid.GetHeight()
            ? HexGridManager.Instance.HexGrid.GetWidth()
            : HexGridManager.Instance.HexGrid.GetHeight();

        List<List<HexGridObject>> allDashableHexes =
            HexGridManager.Instance.HexGrid.GetLineAllDirections(pos, maxDistance);
        List<List<HexGridObject>> allowedHexes = new();

        for (int i = 0; i < allDashableHexes.Count; i++)
        {
            allowedHexes.Add(new List<HexGridObject>());
            if (allDashableHexes[i].Count < 2) continue;

            for (int j = 1; j < allDashableHexes[i].Count; j++) //starting from 1 to skip the adjacent neighbors
            {
                try
                {
                    allowedHexes[i].Add(allDashableHexes[i][j]);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }
        }


        return allowedHexes;
    }

    public List<HexGridObject> SetJumpTargets(bool highlight = true)
    {
        List<List<HexGridObject>> allJumpableHexes = hexGridManager.HexGrid.GetLineAllDirections(transform.position, 2);
        List<HexGridObject> allowedHexes = new();

        for (int i = 0; i < allJumpableHexes.Count; i++)
        {
            if (allJumpableHexes[i].Count < 2) continue;

            if (allJumpableHexes[i][0].IsHidden() && !allJumpableHexes[i][1].IsHidden())
            {
                allowedHexes.Add(allJumpableHexes[i][1]);
                if (highlight) allowedHexes[^1].SetSelectionMode(HexGridManager.SelectionMode.Jump);
            }
        }

        return allowedHexes;
    }

    public static List<HexGridObject> GetAllPotentialJumpTargets(Vector3 pos)
    {
        List<List<HexGridObject>> allJumpableHexes = HexGridManager.Instance.HexGrid.GetLineAllDirections(pos, 2);
        List<HexGridObject> allowedHexes = new();

        foreach (var t in allJumpableHexes)
        {
            if (t.Count < 2) continue;

            allowedHexes.Add(t[1]);
        }

        return allowedHexes;
    }

    public List<HexGridObject> SetTeleportTargets()
    {
        List<HexGridObject> allowedHexes = Extensions.TwoDimArrayToList(hexGridManager.GridArray);
        var neighbors = HexGridManager.Instance.HexGrid.GetNeighborGridObjects(transform.position);
        foreach (var neighbor in neighbors)
        {
            allowedHexes.Remove(neighbor);
        }

        allowedHexes.Remove(PlayerManager.Instance.ActivePlayer.GetHexGridObject());

        return allowedHexes;
    }


    public bool Move(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, movementAnimationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;
            return true;
        }

        return false;
    }

    private float dashTimer;

    public bool Dash(Vector3 target)
    {
        transform.position = Vector3.Lerp(transform.position, target, dashTimer / dashDuration);
        dashTimer += Time.deltaTime;

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;
            dashTimer = 0;
            hexGridManager.HexGrid.TriggerDeselectAll();
            return true;
        }

        return false;
    }


    private bool shrinking = true;
    private bool growing = false;

    public bool Teleport(Vector3 target, float localScale)
    {
        if (shrinking && !growing)
        {
            transform.localScale -=
                new Vector3(movementAnimationSpeed / 2, movementAnimationSpeed / 2, movementAnimationSpeed / 2) *
                Time.deltaTime;
            if (transform.localScale.x <= 0.01f)
            {
                transform.localScale = Vector3.zero;
                growing = true;
            }
            else return false; //if it is shrinking, the code doesn't need to continue.
        }

        if (shrinking && growing)
        {
            transform.position = target;
            new WaitForSeconds(.2f);
            shrinking = false;
        }

        if (growing && !shrinking)
        {
            transform.localScale +=
                new Vector3(movementAnimationSpeed / 2, movementAnimationSpeed / 2, movementAnimationSpeed / 2) *
                Time.deltaTime;
            if (transform.localScale.x >= localScale)
            {
                transform.localScale = new Vector3(localScale, localScale, localScale);
                shrinking = true;
                growing = false;
                return true;
            }
        }

        return false;
    }


    private float parabolicTimer = 0;
    Vector3 origin;

    public bool Jump(Vector3 target, Vector3 originalPos)
    {
        if (parabolicTimer <= 0) origin = originalPos;
        if (parabolicTimer >= parabolicSpeed)
        {
            parabolicTimer = 0;
            transform.position = target;
            hexGridManager.HexGrid.TriggerDeselectAll();
            return true;
        }

        transform.position = Extensions.CalculateParabola(origin, target, jumpHeight, parabolicTimer / parabolicSpeed);
        parabolicTimer += Time.deltaTime;
        return false;
    }

    public bool DoSelectedAction(GameState.PlayerAction playerActionType, Vector3 targetPos, float localScale)
    {
        return playerActionType switch
        {
            GameState.PlayerAction.Moving => Move(targetPos),
            GameState.PlayerAction.Dashing => Dash(targetPos),
            GameState.PlayerAction.Teleporting => Teleport(targetPos, localScale),
            GameState.PlayerAction.Jumping => Jump(targetPos, transform.position),
            _ => false
        };
    }
}