using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The hexagon data used in the game.
/// Hexagons use a mix of the cube and axial coordinate systems.
/// </summary>
[System.Serializable]
public class Hexagon 
{

    public Vector2 arrayCoord;
    public Vector3 cubeCoord;


    public Vector2 dataPos;
    public Vector3 worldPos;


    public float height;
    
    public int tempeture;
    public int percepitation;



    public TerrainType terrainType;
    public enum TerrainType { Grassland, Plains, Desert, Marsh, Forest, Jungle, Tundra, Snow, Ice, River, Lake, Coast, Ocean, Mountain };



    public Hexagon[] neighbors = new Hexagon[6];

    public int pathParent;
    public int pathScore;


    public static Vector3[] neighborDirs = new Vector3[]{ new Vector3(+1, -1, 0), new Vector3(+1, 0, -1), new Vector3(0, +1, -1), 
                                                       new Vector3(-1, +1, 0), new Vector3(-1, 0, +1), new Vector3(0, -1, +1) };


    public static Vector3[] diagonalsDirs = new Vector3[]{ new Vector3(+2, -1, -1), new Vector3(+1, +1, -2), new Vector3(-1, +2, -1),
                                                       new Vector3(-2, +1, +1), new Vector3(-1, -1, +2), new Vector3(+1, -2, +1) };



    public Hexagon(Vector2 arrayCoord, Vector2 dataPos, float height, Vector3 worldPos)
    {
        this.arrayCoord = arrayCoord;
        this.cubeCoord = OffsetToCubeOddQ(arrayCoord);

        this.dataPos = dataPos;
        this.worldPos = worldPos;
        this.height   = height;
        this.neighbors = new Hexagon[6];
    }

    public void SetPathParent(Hexagon parentHex)
    {
        pathScore = 0;
        

        if (parentHex == null)
        {
            pathParent = -1;
        }
        else
        {
            //Debug.Log("Seting parent " + parentHex.arrayCoord);
            for (int i = 0; i < 6; i++)
            {
                //Debug.Log(neighbors[i].arrayCoord);
                if (neighbors[i] == parentHex) 
                    pathParent = i;
            }
            //Debug.Log(this + " pp: " + pathParent + " " + neighbors[pathParent]);
        }
        
    }

    public int GetPathScores(Hexagon goal)
    {
        // if there was no previous path to this hex 
        if (pathParent == -1)
        {
            pathScore = 0;
        }
        else
        {
            // Get the score of the parent hex
            int parentScore = neighbors[pathParent].pathScore;
            int score = (int)this.Distance(this.cubeCoord, goal.cubeCoord);

            // Do score logic here
            // score = calculate hight differences

            pathScore = parentScore + score;

        }

        return pathScore;
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
            rx = -ry-rz;
        else if (y_diff > z_diff)
            ry = -rx-rz;
        else
            rz = -rx-ry;

        return new Vector3(rx, ry, rz);
    }


    public static Vector2 ArrayCoordToOffset(Vector2 arrayCoord)
    {
        arrayCoord.y -= (int)(arrayCoord.x / 2);
        return arrayCoord;
    }
    

    public static Vector2 CubeToOffsetOddQ(Vector3 cube)
    {
        //# convert cube to odd-r offset
        //q = x + (z - (z&1)) / 2
        //r = z
        Vector2 offset = Vector2.zero;
        offset.x = cube.x + (cube.z - ((int)cube.z&1)) / 2;
        offset.y = cube.z;

        return offset;
    }
    public static Vector3 OffsetToCubeOddQ(Vector2 array)
    {
        //# convert odd-r offset to cube
        //x = q - (r - (r&1)) / 2
        //z = r
        //y = -x-z
        Vector3 cube = new Vector3();
        cube.x = array.x - (array.y - ((int)array.y&1)) / 2;
        cube.z = array.y;
        cube.y = -cube.x - cube.z;

        return cube;
    }




    public float Distance(Vector3 hex1, Vector3 hex2)
    {
        return (Mathf.Abs(hex1.x - hex2.x) + Mathf.Abs(hex1.y - hex2.y) + Mathf.Abs(hex1.z - hex2.z)) / 2;
    }

    public int DirectionTo(Hexagon toHex)
    {
        if (this == toHex)
        {
            Debug.LogWarning("Can't get direction to self");
            return -1;
        }

        Vector3 dir = toHex.worldPos - this.worldPos;
        float angle = Vector3.Angle(dir, Vector3.right);
        Vector3 rhs = Vector3.Cross(dir, Vector3.right);

        if (rhs.y < 0) 
            return 5 - (int)(angle / 60); 
        else 
            return (int)(angle / 60);
    }


    public List<Vector3> InRange(int distance)
    {

        List<Vector3> results = new List<Vector3>();
        
        for(int x = -distance; -distance <= x && x <= distance; x++)
            for(int y = -distance; -distance <= y && y <= distance; y++)
                for(int z = -distance; -distance <= z && z <= distance; z++)
                    if (x + y + z == 0)
                        results.Add(new Vector3(x, y, z));


        /*
        for(int x = -distance, z=0; -distance <= x && x <= distance; x++)
        {
            for (int y = -distance; Mathf.Max(-distance, -x - distance) <= y && y <= Mathf.Min(distance, -x + distance); y++)
            {
                z = -x - y;
                results.Add(new Vector3(x, y, z));
            }
        }
        */
        
        return results;
    }


    /*
     * function cube_ring(center, radius):
    var results = []
    var cube = cube_add(center, 
                        cube_scale(cube_direction(4), radius))
    for each 0 ≤ i < 6:
        for each 0 ≤ j < radius:
            results.append(cube)
            cube = cube_neighbor(cube, i)
    return results
     */

    public void Line (Vector3 hex1, Vector3 hex2)
    {
        float distance = Distance(hex1, hex2);
        //for each 0 ≤ i ≤ N:
        //    draw hex at hex_round(A * (1 - i/N) + B * i/N)
    }


    /*function hex_round(Cube(x, y, z)):
    rx = round(x)
    ry = round(y)
    rz = round(z)

    x_diff = abs(rx - x)
    y_diff = abs(ry - y)
    z_diff = abs(rz - z)

    if x_diff > y_diff and x_diff > z_diff:
        rx = -ry-rz
    else if y_diff > z_diff:
        ry = -rx-rz
    else:
        rz = -rx-ry

    return Cube(rx, ry, rz)*/


    /*
     
    # convert cube to axial
    q = x
    r = z

    # convert axial to cube
    x = q
    z = r
    y = -x-z  
     
    */


    /* Distance = Cube
     * Movement range = Cube
     * 
     * 
     * 
     */


}
