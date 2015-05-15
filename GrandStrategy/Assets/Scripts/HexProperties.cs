using UnityEngine;
using System.Collections;

public static class HexProperties
{
    public static float side;
    public static float height;
    public static float width;

    public static float tileR;
    public static float tileH;

    public static Vector3[] vertPos;


    public static float unityWidth;
    public static float unityHeight;


    public static void SetProperties (float hexSide, float pixelsPerUnit)
    {
        side = hexSide;
        tileH = Mathf.Sin((30f * Mathf.PI) / 180f) * side;
        tileR = Mathf.Cos((30f * Mathf.PI) / 180f) * side;

        width = Mathf.RoundToInt(2f * tileR);
        height = Mathf.RoundToInt(side + 2f * tileH);

        vertPos = new Vector3[6];
        for (int i = 0; i < vertPos.Length; i++)
        {
            vertPos[i] = GetVert(i);
        }

        unityWidth = width / pixelsPerUnit;
        unityHeight = height / pixelsPerUnit;

    }

    /*  Gets the vert position of a hexagon
     *     0  
     *  5     1
     *  4     2
     *     3
     */
    public static Vector3 GetVert(int angle)
    {
        return new Vector3(side * Mathf.Cos(2 * Mathf.PI / 6 * (angle + 0.5f)),
                           side * Mathf.Sin(2 * Mathf.PI / 6 * (angle + 0.5f)),
                           0);
    }
}

public static class FOWHexProperties
{
    public static float side;
    public static float height;
    public static float width;

    public static float tileR;
    public static float tileH;

    public static Vector3[] vertPos;


    public static void SetProperties(float hexSide)
    {
        side = hexSide;
        tileH = Mathf.Sin((30f * Mathf.PI) / 180f) * side;
        tileR = Mathf.Cos((30f * Mathf.PI) / 180f) * side;

        width = Mathf.RoundToInt(2f * tileR);
        height = Mathf.RoundToInt(side + 2f * tileH);

        vertPos = new Vector3[6];
        for (int i = 0; i < vertPos.Length; i++)
        {
            vertPos[i] = GetVert(i);
        }
    }

    /*  Gets the vert position of a hexagon
     *     0  
     *  5     1
     *  4     2
     *     3
     */
    public static Vector3 GetVert(int angle)
    {
        return new Vector3(side * Mathf.Cos(2 * Mathf.PI / 6 * (angle + 0.5f)),
                           side * Mathf.Sin(2 * Mathf.PI / 6 * (angle + 0.5f)),
                           0);
    }
}