using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Huge thanks to redblobgames.com. Amazing website!
public class CubeTranslations
{
    public static void Axial_To_Oddr(int q, int r, out int x, out int z)
    {
        x = q + (r - (r % 1)) / 2;
        z = r;
    }

    public static void Oddr_To_Axial(int x, int z, out int q, out int r)
    {
        q = x - (z - (z % 1)) / 2;
        r = z;
    }

    /// <summary>
    /// Element 0: Right.
    /// <br>Element 1: Top Right.</br>
    /// <br>Element 2: Top Left.</br>
    /// <br>Element 3: Left.</br>
    /// <br>Element 4: Bottom Left.</br>
    /// <br>Element 5: Bottom Right</br>
    /// </summary>
    public static List<Cube> HexCube_Directions = new()
    {
        new Cube(+1, 0, -1), //Right
        new Cube(+1, -1, 0),
        new Cube(0, -1, +1),
        new Cube(-1, 0, +1),
        new Cube(-1, +1, 0),
        new Cube(0, +1, -1),
    };

    public static Cube HexCube_Direction(int direction)
    {
        return HexCube_Directions[direction];
    }

    public static Cube HexCube_Add(Cube hex, Cube toAdd)
    {
        return new Cube(hex.q + toAdd.q, hex.r + toAdd.r, hex.s + toAdd.s);
    }

    public static Cube HexCube_Substract(Cube hex, Cube subs)
    {
        return new Cube(hex.q - subs.q, hex.r - subs.r, hex.s - subs.s);
    }

    public static Cube GetHexCubeNeighbor(Cube hexCube, int direction)
    {
        return HexCube_Add(hexCube, HexCube_Direction(direction));
    }

    public static Cube HexCube_Scale(Cube hex, int factor)
    {
        return new Cube(hex.q * factor, hex.r * factor, hex.s * factor);
    }

    public static List<Cube> HexCubeRing(Cube center, int radius)
    {
        List<Cube> results = new List<Cube>();
        Cube hex = HexCube_Add(center, HexCube_Scale(HexCube_Direction(4), radius));
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                results.Add(hex);
                hex = GetHexCubeNeighbor(hex, i);
            }
        }
        return results;
    }

    public static float Distance(Cube a, Cube b)
    {
        var vec = HexCube_Substract(a, b);
        return (Mathf.Abs(vec.q) + Mathf.Abs(vec.r) + Mathf.Abs(vec.s)) / 2;
    }

    public static List<Cube> DrawLine(Cube center, int direction ,int length)
    {
        List<Cube> lineElements = new();
        lineElements.Add(center);
        for (int i = 0; i < length; i++)
        {
            lineElements.Add(GetHexCubeNeighbor(lineElements[i], direction));
        }
        return lineElements;
    }

   
}

public struct Cube
{
    public int q;
    public int r;
    public int s;
    public Vector3Int vec;

    public Cube(int q, int r)
    {
        this.q = q;
        this.r = r;
        this.s = -q - r;
        this.vec = new Vector3Int(q, r, this.s);
    }

    public Cube(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        vec = new Vector3Int(q, r, s);

    }

    public Cube(Vector3Int vec)
    {
        q = vec.x;
        r = vec.y;
        s = vec.z;
        this.vec = vec;
    }

}
