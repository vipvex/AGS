using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Hex : HexBase, IHeapItem<Hex>
{
    public int XIndex, YIndex, Elevation;
    public Vector3 WorldPos, CubeIndex;


    public List<Hex> neighbors = new List<Hex>();

    public static Vector3[] neighborDirs = new Vector3[]{ new Vector3(+1, -1, 0), new Vector3(+1, 0, -1), new Vector3(0, +1, -1), new Vector3(-1, +1, 0), new Vector3(-1, 0, +1), new Vector3(0, -1, +1) };

    // Pathfinding properties
    public int gCost;
    public int hCost;
    public int heapIndex;


    public Hex(int XIndex, int YIndex, int Elevation, Vector3 WorldPos)
    {
       this.XIndex = XIndex;
       this.XIndex = YIndex;
       this.Elevation = Elevation;
       this.WorldPos = WorldPos;
       this.CubeIndex = OffsetToCubeOddQ(new Vector2(XIndex, YIndex));
    }

    #region Pathfinding
    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }
    public int CompareTo(Hex hexToCompare)
    {
        int compare = fCost.CompareTo(hexToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(hexToCompare.hCost);
        }
        return -compare;
    }
    #endregion

    #region Hex coordinate conversions
    public static Vector2 ArrayCoordToOffset(Vector2 arrayCoord)
    {
        arrayCoord.y -= (int)(arrayCoord.x / 2);
        return arrayCoord;
    }


    public static Vector2 CubeToOffsetOddQ(Vector3 cube)
    {
        Vector2 offset = Vector2.zero;
        offset.x = cube.x + (cube.z - ((int)cube.z & 1)) / 2;
        offset.y = cube.z;

        return offset;
    }
    public static Vector3 OffsetToCubeOddQ(Vector2 array)
    {
        Vector3 cube = new Vector3();
        cube.x = array.x - (array.y - ((int)array.y & 1)) / 2;
        cube.z = array.y;
        cube.y = -cube.x - cube.z;

        return cube;
    }

    public static Vector3 RoundCubeCoord(Vector3 cube)
    {
        float rx = Mathf.Round(cube.x);
        float ry = Mathf.Round(cube.y);
        float rz = Mathf.Round(cube.z);

        float x_diff = Mathf.Abs(rx - cube.x);
        float y_diff = Mathf.Abs(ry - cube.y);
        float z_diff = Mathf.Abs(rz - cube.z);

        if (x_diff > y_diff && x_diff > z_diff)
            rx = -ry - rz;
        else if (y_diff > z_diff)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        return new Vector3(rx, ry, rz);
    }

    //public static Hex GetHexAtPos(TerrainManagerViewModel terrainManager, Vector3 pos)
    //{
    //    float pointX = pos.x;
    //    float pointZ = pos.z;
    //    pointX = pointX * terrainManager.PixelsPerUnit;
    //    pointZ = pointZ * terrainManager.PixelsPerUnit;
    //    pointZ += HexProperties.tileH;
    //    pointX -= HexProperties.width / 2;
    //    pointZ -= (HexProperties.height - HexProperties.tileH);
    //
    //
    //    float q = (1f / 3f * Mathf.Sqrt(3f) * pointX - 1f / 3f * pointZ) / HexProperties.side;
    //    float r = 2f / 3f * pointZ / HexProperties.side;
    //
    //
    //    Vector3 cube = new Vector3();
    //    cube.x = q;
    //    cube.z = r;
    //    cube.y = -cube.x - cube.z;
    //
    //    cube = Hexagon.RoundCubeCoord(cube);
    //
    //    Vector2 hoverHexAraray = Hexagon.CubeToOffsetOddQ(cube);
    //    Hex hex = null;
    //
    //    if (hoverHexAraray.x >= 0 && hoverHexAraray.y >= 0 && hoverHexAraray.x < terrainManager.hexGrid.GetLength(0) && hoverHexAraray.y < terrainManager.hexGrid.GetLength(1))
    //        hex = terrainManager.hexGrid[(int)hoverHexAraray.x, (int)hoverHexAraray.y];
    //
    //    return hex;
    //}
    #endregion

    #region Search Operations

    public Hex RandomNeighbor()
    {
        return neighbors[UnityEngine.Random.Range(0, neighbors.Count - 1)];
    }

    public static void SearchNeighbors(Hex hex, Func<Hex, bool> searchParams, List<Hex> result)
    {
        hex.neighbors.Where(searchParams).ToList().ForEach(t_hex =>
        {
            if (result.Contains(t_hex) == false)
            {
                result.Add(t_hex);
                Hex.SearchNeighbors(t_hex, searchParams, result);
            }
        });
    }

    public Hex LowestNeighbor()
    {
        int lowestElevation = Elevation;
        int lowestElevationIndex = -1;
        for (int i=0; i<neighbors.Count; i++)
        {
            if (neighbors[i].Elevation < lowestElevation)
            {
                lowestElevation = lowestElevationIndex;
                lowestElevationIndex = i;
            }
        }

        if (lowestElevationIndex > -1)
        {
            return neighbors[lowestElevationIndex];
        }

        return null;
    }

    public bool WaterHex()
    {
        return TerrainType == TerrainType.Lake || TerrainType == TerrainType.Ocean || TerrainType == TerrainType.Sea || TerrainType == TerrainType.River;
    }

    public List<Hex> SurroundingWaterHexes(int waterElevation)
    {

        List<Hex> openSet = new List<Hex>();
        HashSet<Hex> closedSet = new HashSet<Hex>();
        openSet.Add(this);

        while (openSet.Count > 0)
        {
            
            Hex currentHex = openSet[0];
            openSet.RemoveAt(0);

            closedSet.Add(currentHex);

            if (currentHex == null)
            {
                return openSet;
            }

            foreach (Hex neighbour in currentHex.neighbors)
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                if (neighbour.Elevation < waterElevation)        
                    openSet.Add(neighbour);

            }
        }

        return openSet;
    }

    #endregion
}
