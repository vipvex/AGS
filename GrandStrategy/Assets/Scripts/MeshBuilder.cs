using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshBuilder : MonoBehaviour 
{

    //public GameObject areaObj;

    /*
    public static void DrawArea (GameObject areaObj, List<Hex> area)
    {
        areaObj.transform.position = Vector3.zero;
        areaObj.transform.rotation = Quaternion.identity;
        MeshFilter meshFilter = areaObj.GetComponent<MeshFilter>();
        meshFilter.mesh.Clear();




        Vector3[] vertices = new Vector3[area.Count * 6];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        int v = 0;
        foreach(Hex hex in area)
        {
            if (hex == null)
                continue;

            for (int i=5; i > -1; i--)
            {
                // Normalize the vert pos
                vertices[v] = hex.worldPos + new Vector3(HexProperties.vertPos[i].x, 0, HexProperties.vertPos[i].y) / HexProperties.side * 2;
                normals[v] = Vector3.up;
                uv[v] = Vector2.zero;
                
                v++;
            }

        }

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.normals = normals;
        meshFilter.mesh.uv = uv;

       
       v = 0;
       int[] triangles = new int[area.Count * 12];
       for (int i = 0; i < area.Count * 6; i += 6)
       {
           triangles[v]     = i;
           triangles[v + 1] = i + 1;
           triangles[v + 2] = i + 2;


           triangles[v + 3] = i;
           triangles[v + 4] = i + 2;
           triangles[v + 5] = i + 3;

           triangles[v + 6] = i;
           triangles[v + 7] = i + 3;
           triangles[v + 8] = i + 4;

           triangles[v + 9] = i;
           triangles[v + 10] = i + 4;
           triangles[v + 11] = i + 5;
           v += 12;
       }


        meshFilter.mesh.triangles = triangles;
    }
    */

}
