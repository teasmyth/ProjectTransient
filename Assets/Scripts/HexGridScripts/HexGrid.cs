using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid<TGridObject>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int z;
    }

    public event SetSelectionMode OnSelectionModeSet;

    public delegate void SetSelectionMode(HexGridManager.SelectionMode newMode);

    public event DeselectAll OnDeselectAll;

    public delegate void DeselectAll(HexGridObject exceptionHex = null);


    public const float Hex_Vertical_Offset_Multiplier = .75f;

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public HexGrid(int width, int height, float cellSize, Vector3 originPosition, GameObject[,] hexes,
        ref List<HexGridManager.ColorType> usedColors, Func<HexGrid<TGridObject>, GameObject, int, int, TGridObject> createGridObject,
        out TGridObject[,] gridArrayOut)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        List<HexGridManager.ColorType> colors = new();
        for (int x = 0; x < width; x++)
        {
          
            //x == 0 is Null colot. So we skip it.
            usedColors.Add((HexGridManager.ColorType)x + 1);

            for (int y = 0; y < height; y++)
            {
                colors.Add((HexGridManager.ColorType)x + 1);
            }
        }

        for (var x = 0; x < gridArray.GetLength(0); x++)
        {
            for (var z = 0; z < gridArray.GetLength(1); z++)
            {
                gridArray[x, z] = createGridObject(this,hexes[x, z], x, z);
                hexes[x, z].transform.position = GetWorldPosition(x, z);
                hexes[x, z].transform.localScale *= cellSize;
                var colorToAssignIndex = UnityEngine.Random.Range(0, colors.Count);
                var color = colors[colorToAssignIndex];

                try
                {
                    HexGridObject hexGridObject = GetGridObject(x, z) as HexGridObject;
                    hexGridObject?.SetColor(color, HexGridManager.AssignColor(color));
                }
                catch (Exception e)
                {
                    Debug.Log("Could not cast to HexGridObject. Error: " + e.Message);
                }

                colors.RemoveAt(colorToAssignIndex);
            }
        }

        //To show grid coords.
        
        bool showDebug = false;
        if (showDebug)
        {
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    debugTextArray[x, z] = Extensions.CreateWorldText(gridArray[x, z]?.ToString(), null,
                        GetWorldPosition(x, z), 6, Color.black, TextAnchor.MiddleCenter, TextAlignment.Center, 0);
                }
            }

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z]?.ToString();
            };
        }
        

        gridArrayOut = gridArray;
    }

    #region Gets

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, 0) * cellSize +
               new Vector3(0, 0, z) * cellSize * Hex_Vertical_Offset_Multiplier +
               ((z % 2 == 1) ? new Vector3(1, 0, 0) * cellSize * .5f : Vector3.zero) + originPosition;
    }

    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        worldPosition.y = 0;
        int roughX = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize - .1f * cellSize);
        int roughZ = Mathf.RoundToInt((worldPosition - originPosition).z / cellSize / Hex_Vertical_Offset_Multiplier);

        Vector3Int roughXZ = new Vector3Int(roughX, 0, roughZ);
        bool oddRow = roughZ % 2 == 1;
        List<Vector3Int> neighbourXZList = new List<Vector3Int>()
        {
            roughXZ + new Vector3Int(-1, 0, 0),
            roughXZ + new Vector3Int(1, 0, 0),

            roughXZ + new Vector3Int(oddRow ? 1 : -1, 0, 1),
            roughXZ + new Vector3Int(0, 0, 1),

            roughXZ + new Vector3Int(oddRow ? 1 : -1, 0, -1),
            roughXZ + new Vector3Int(0, 0, -1),
        };

        Vector3Int closestXZ = roughXZ;

        foreach (Vector3Int neighborXZ in neighbourXZList)
        {
            if (Vector3.Distance(worldPosition, GetWorldPosition(neighborXZ.x, neighborXZ.z)) <
                Vector3.Distance(worldPosition, GetWorldPosition(closestXZ.x, closestXZ.z)))
            {
                closestXZ = neighborXZ;
            }
        }

        x = closestXZ.x;
        z = closestXZ.z;
    }

    public TGridObject GetGridObject(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            return gridArray[x, z];
        }
        else
        {
            return default;
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, z;
        worldPosition.y = 0;
        GetXZ(worldPosition, out x, out z);
        return GetGridObject(x, z);
    }

    public List<TGridObject> GetNeighborGridObjects(Vector3 worldPosition)
    {
        GetXZ(worldPosition, out int x, out int z);
        CubeTranslations.Oddr_To_Axial(x, z, out int q, out int r);
        Cube center = new Cube(q, r);
        List<Cube> neighbors = CubeTranslations.HexCubeRing(center, 1);
        return TranslateFromCubeToGridObjectList(neighbors);
    }

    //Return hexes at the range of the given hex.
    public List<TGridObject> GetGridObjectsOnRange(Vector3 worldPosition, int range)
    {
        List<TGridObject> gridObjectsOnRange = new();
        Cube centerHex = TranslateToCube(worldPosition);

        List<Cube> hexCubesAtRange = CubeTranslations.HexCubeRing(centerHex, range);

        foreach (Cube hexCube in hexCubesAtRange)
        {
            CubeTranslations.Axial_To_Oddr(hexCube.q, hexCube.r, out int x, out int z);
            var gridObjectToAdd = GetGridObject(x, z);
            if (gridObjectToAdd != null) gridObjectsOnRange.Add(gridObjectToAdd);
        }

        return gridObjectsOnRange;
    }

    //Returns hexes within the range of the given hex.
    public List<TGridObject> GetGridObjectsInRange(Vector3 worldPosition, int range)
    {
        List<TGridObject> gridObjectsInRange = new();
        for (int i = 1; i <= range; i++)
        {
            List<TGridObject> gridObjectsOnRange = GetGridObjectsOnRange(worldPosition, i);
            for (int j = 0; j < gridObjectsOnRange.Count; j++)
            {
                gridObjectsInRange.Add(gridObjectsOnRange[j]);
            }
        }

        return gridObjectsInRange;
    }

    /// <summary>
    /// Element 0: Right.
    /// <br>Element 1: Top Right.</br>
    /// <br>Element 2: Top Left.</br>
    /// <br>Element 3: Left.</br>
    /// <br>Element 4: Bottom Left.</br>
    /// <br>Element 5: Bottom Right</br>
    /// </summary>
    public List<TGridObject> GetLine(Vector3 worldPosition, int direction, int length, bool includeCenter = false)
    {
        Cube centerHex = TranslateToCube(worldPosition);
        List<Cube> hexCubesLine = CubeTranslations.DrawLine(centerHex, direction, length);
        if (!includeCenter) hexCubesLine.Remove(centerHex);
        return new List<TGridObject>(TranslateFromCubeToGridObjectList(hexCubesLine));
    }

    public List<List<TGridObject>> GetLineAllDirections(Vector3 worldPosition, int length, bool includeCenter = false)
    {
        List<List<TGridObject>> lines = new();
        for (int i = 0; i < 6; i++)
        {
            List<TGridObject> line = GetLine(worldPosition, i, length, includeCenter);
            lines.Add(line);
        }

        return lines;
    }

    #endregion

    #region Sets

    public void SetGridObject(int x, int z, TGridObject value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            gridArray[x, z] = value;
            TriggerGridObjectChanged(x, z);
        }
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXZ(worldPosition, out int x, out int z);
        SetGridObject(x, z, value);
    }

    #endregion
    
    public float GetDistance(Vector3 worldPositionA, Vector3 worldPositionB)
    {
        Cube a = TranslateToCube(worldPositionA);
        Cube b = TranslateToCube(worldPositionB);
        return CubeTranslations.Distance(a, b);
    }

    public Cube TranslateToCube(Vector3 worldPosition)
    {
        worldPosition.y = 0;
        GetXZ(worldPosition, out int centerX, out int centerZ);
        CubeTranslations.Oddr_To_Axial(centerX, centerZ, out int q, out int r);
        return new Cube(q, r);
    }

    public List<TGridObject> TranslateFromCubeToGridObjectList(List<Cube> cubeList)
    {
        List<TGridObject> list = new();
        for (int i = 0; i < cubeList.Count; i++)
        {
            CubeTranslations.Axial_To_Oddr(cubeList[i].q, cubeList[i].r, out int x, out int z);
            if (GetGridObject(x, z) != null) list.Add(GetGridObject(x, z));
        }

        return list;
    }

    public void TriggerGridObjectChanged(int x, int z)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
    }

    public void TriggerSelectionModeSet(HexGridManager.SelectionMode newMode) => OnSelectionModeSet?.Invoke(newMode);


    public void TriggerDeselectAll(HexGridObject hexGridObject = null)
    {
        if (OnDeselectAll != null)
        {
            OnDeselectAll(hexGridObject);
        }
    }
}