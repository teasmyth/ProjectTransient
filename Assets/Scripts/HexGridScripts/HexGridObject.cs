using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridObject
{
    private int x;
    private int z;
    public GameObject gameObject;

    private HexGridManager.ColorType _colorType;
    private bool _hidden = false;
    private bool _occupied = false;
    private bool _colorPermaShown;
    private HexGridManager.SelectionMode _selectionMode;


    public GameObject defaultRenderer;
    private SpriteRenderer _highlightRenderer;
    private SpriteRenderer _hexColor;
    public List<HexGridObject> Neighbors { get; private set; }

    public HexGridObject(HexGrid<HexGridObject> grid, GameObject go, int x, int z)
    {
        this.x = x;
        this.z = z;
        gameObject = go;

        GridObjectSettings settings = go.GetComponent<GridObjectSettings>();
        _selectionMode = HexGridManager.SelectionMode.None;
        defaultRenderer = settings.defaultRenderer;
        _highlightRenderer = settings.highlightRenderer;
        _hexColor = settings.hexColor;

        grid.OnDeselectAll += DeselectAll;
    }

    public void SetNeighbors(List<HexGridObject> neighbors)
    {
        Neighbors = neighbors;
    }


    public bool IsHidden() => _hidden;
    public bool IsOccupied() => _occupied;


    public int GetX() => x;
    public int GetZ() => z;

    public HexGridManager.ColorType GetColorType() => _colorType;
    public Vector3 GetPos() => gameObject.transform.position;

    public Cube GetCube()
    {
        CubeTranslations.Oddr_To_Axial(x, z, out int q, out int r);
        return new Cube(q, r);
    }


    public void Highlight()
    {
        _highlightRenderer.color = HighlightColor(_selectionMode);
    }

    public void OverrideHighlight(HexGridManager.SelectionMode mode)
    {
        _highlightRenderer.color = HighlightColor(mode);
    }


    public void Deselect()
    {
        _highlightRenderer.color = HighlightColor(_selectionMode);
    }

    public void DeselectAll(HexGridObject hexGridObjectException = null)
    {
        if (hexGridObjectException != this) SetSelectionMode(HexGridManager.SelectionMode.None);
    }


    public void SetSelectionMode(HexGridManager.SelectionMode mode)
    {
        _selectionMode = mode;
        _highlightRenderer.color = HighlightColor(mode);
    }


    private Color HighlightColor(HexGridManager.SelectionMode mode)
    {
        return HexGridManager.Instance.HighlightColor(mode);
    }


    public void SetOccupied(bool isOccupied) => _occupied = isOccupied;
    public void SetHidden(bool isHidden) => _hidden = isHidden;
    public void SetColorType(HexGridManager.ColorType type) => _colorType = type;

    public void SetColor(HexGridManager.ColorType type, Color color)
    {
        _colorType = type;
        _hexColor.color = color;
    }

    public void ShowColor(bool showColor)
    {
        if (_colorPermaShown)
        {
            _hexColor.enabled = true;
        }
        else _hexColor.enabled = showColor;
    }

    public void SetColorPermaShown(bool showColor) => _colorPermaShown = showColor;

    public override string ToString()
    {
        return GetCoord();
    }

    public string GetCoord()
    {
        return x + ", " + z;
    }

    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}