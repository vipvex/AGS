using UnityEngine;
using System.Collections;

/// <summary>
/// The hex chunck is responsible for generationg and update it's part of the terrain
/// </summary>
public class HexChunk : MonoBehaviour 
{

    private TerrainManager2 terrainManager2;

    private int terrainDataX, terrainDataY, chunkSize, chunkResolution, collisionResolution;


    private Vector3[] vertices;
    private Vector3[] normals;


    public Texture2D texture;


    public void Initialize(int terrainDataX, int terrainDataY, int chunkSize, int chunkResolution, int collisionResolution, TerrainManager2 terrainManager2)
    {
        this.terrainDataX = terrainDataX;
        this.terrainDataY = terrainDataY;
        this.chunkSize = chunkSize;
        this.chunkResolution = chunkResolution;
        this.collisionResolution = collisionResolution;
        this.terrainManager2 = terrainManager2;

        GenerateChunk();
    }

    public void GenerateChunk ()
    {
        GenerateMesh();
        GenerateCollisionMesh();
        GenerateTexture();
    }


    private void GenerateMesh()
    {

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        // Pixels per vetex point
        float resStep = chunkSize / chunkResolution;       
        float uvStep = 1f / chunkSize;

        vertices = new Vector3[(chunkResolution + 1) * (chunkResolution + 1)];
        normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];


        for (int v = 0, z = 0; z <= chunkResolution; z++) 
        {
            for (int x = 0; x <= chunkResolution; x++, v++) 
            {

                vertices[v] = new Vector3(x * resStep / terrainManager2.pixelsPerUnit,
                                          terrainManager2.hexTerrainData[Mathf.RoundToInt(x * (chunkSize / chunkResolution) + terrainDataX), Mathf.RoundToInt(z * (chunkSize / chunkResolution) + terrainDataY)] * terrainManager2.resolutionHeight,
                                          z * resStep / terrainManager2.pixelsPerUnit
                                          );


                normals[v] = Vector3.up;
                uv[v] = new Vector2(x * resStep * uvStep, z * resStep * uvStep);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;

        int[] triangles = new int[chunkResolution * chunkResolution * 6];
        for (int t = 0, v = 0, y = 0; y < chunkResolution; y++, v++) 
        {
            for (int x = 0; x < chunkResolution; x++, v++, t += 6)
            {
                triangles[t] = v;
                triangles[t + 1] = v + chunkResolution + 1;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + chunkResolution + 1;
                triangles[t + 5] = v + chunkResolution + 2;
            }
        }
        mesh.triangles = triangles;

        CalculateNormals();
        mesh.RecalculateBounds();
        TangentSolver.Solve(mesh);

    }


    private void GenerateCollisionMesh()
    {

        Mesh meshCollider = new Mesh();
        Vector3[] colVertices = new Vector3[(terrainManager2.chunkCollisionResolution + 1) * (terrainManager2.chunkCollisionResolution + 1)];

        float quadSize = terrainManager2.chunkCollisionResolution / chunkSize;


        float terrainSizeInUnits = terrainManager2.terrainWidth / terrainManager2.pixelsPerUnit;
        float unitsPerChunk = terrainSizeInUnits / (terrainManager2.terrainWidth / terrainManager2.chunkSize);
        float unitsPerRes = unitsPerChunk / terrainManager2.chunkCollisionResolution; 

        float stepSize = 1f / terrainManager2.terrainWidth;
        float resScale = chunkSize / terrainManager2.chunkCollisionResolution;


        for (int v = 0, z = 0; z <= terrainManager2.chunkCollisionResolution; z++) 
        {
            for (int x = 0; x <= terrainManager2.chunkCollisionResolution; x++, v++)
            {
                colVertices[v] = new Vector3(x * unitsPerRes,
                                             terrainManager2.hexTerrainData[Mathf.RoundToInt(x * (chunkSize / terrainManager2.chunkCollisionResolution) + terrainDataX), Mathf.RoundToInt(z * (chunkSize / terrainManager2.chunkCollisionResolution) + terrainDataY)] * terrainManager2.resolutionHeight,
                                             z * unitsPerRes
                                             );
            }
        }

        meshCollider.vertices = colVertices;

        int[] triangles = new int[terrainManager2.chunkCollisionResolution * terrainManager2.chunkCollisionResolution * 6];
        for (int t = 0, v = 0, y = 0; y < terrainManager2.chunkCollisionResolution; y++, v++)
        {
            for (int x = 0; x < terrainManager2.chunkCollisionResolution; x++, v++, t += 6)
            {
                triangles[t] = v;
                triangles[t + 1] = v + terrainManager2.chunkCollisionResolution + 1;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + terrainManager2.chunkCollisionResolution + 1;
                triangles[t + 5] = v + terrainManager2.chunkCollisionResolution + 2;
            }
        }
        meshCollider.triangles = triangles;


        GetComponent<MeshCollider>().sharedMesh = meshCollider;


    }


    private float GetXDerivative(int x, int z)
    {
        int rowOffset = z * (chunkResolution + 1);
        float left, right, scale;
        if (x > 0)
        {
            left = vertices[rowOffset + x - 1].y;
            if (x < chunkResolution)
            {
                right = vertices[rowOffset + x + 1].y;
                scale = 0.5f * chunkResolution;
            }
            else
            {
                right = vertices[rowOffset + x].y;
                scale = chunkResolution;
            }
        }
        else
        {
            left = vertices[rowOffset + x].y;
            right = vertices[rowOffset + x + 1].y;
            scale = chunkResolution;
        }
        return (right - left) * scale;
    }

    private float GetZDerivative(int x, int z)
    {
        int rowLength = chunkResolution + 1;
        float back, forward, scale;
        if (z > 0)
        {
            back = vertices[(z - 1) * rowLength + x].y;
            if (z < chunkResolution)
            {
                forward = vertices[(z + 1) * rowLength + x].y;
                scale = 0.5f * chunkResolution;
            }
            else
            {
                forward = vertices[z * rowLength + x].y;
                scale = chunkResolution;
            }
        }
        else
        {
            back = vertices[z * rowLength + x].y;
            forward = vertices[(z + 1) * rowLength + x].y;
            scale = chunkResolution;
        }
        return (forward - back) * scale;
    }

    private void CalculateNormals()
    {
        for (int v = 0, z = 0; z <= chunkResolution; z++)
        {
            for (int x = 0; x <= chunkResolution; x++, v++)
            {
                normals[v] = new Vector3(-GetXDerivative(x, z), 1f, -GetZDerivative(x, z)).normalized;
            }
        }
    }

    /// <summary>
    /// Converts the terrain data to a texture for the chunk
    /// </summary>
    private void GenerateTexture()
    {
        texture = new Texture2D(chunkSize, chunkSize);
        texture.wrapMode = TextureWrapMode.Clamp;
        //texture.filterMode = FilterMode.Point;  
        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
                texture.SetPixel(x, y, terrainManager2.terrainAltitudeColors.Evaluate(terrainManager2.hexTerrainData[x + terrainDataX, y + terrainDataY]));

        texture.Apply();
        GetComponent<Renderer>().material.mainTexture = texture;
    }


}
