using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Kernel;
using uFrame.MVVM;
using uFrame.MVVM.Services;
using uFrame.MVVM.Bindings;
using uFrame.Serialization;
using UniRx;
using UnityEngine;
using System.Threading;


public class Chunk
{
    public GameObject Obj;
    public Texture2D Heightmap;
    public float[,] Heights;
    public int Resolution;

    public bool NeedsToUpdate;
    public bool Busy;
    public bool MeshReady;


    public static int PixelsPerUnit;
    public static int PixelsToHeight;


    // Mesh data
    public int[] triangles;
    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uv;


    public void GenMeshData()
    {
        Busy = true;

        // Pixels per vetex point
        int res = Resolution;

        int lowerRes = Resolution;

        float resStep = (float)Heights.GetLength(0) / (float)res;
        float lowerResStep = (float)Heights.GetLength(0) / (float)lowerRes;

        float lowVertPerHighVert = (float)res / (float)lowerRes; // 2 or 1 


        float heightmapStep = (float)Heights.GetLength(0) / (float)res;
        float lowerHeightmapStep = (float)Heights.GetLength(0) / (float)lowerRes;
        float uvStep = 1f / Heights.GetLength(0);


        vertices = new Vector3[(res + 1) * (res + 1)];
        normals = new Vector3[vertices.Length];
        uv = new Vector2[vertices.Length];

        float xPos, zPos = 0;
        float lowX, lowZ;
        float height = 0;


        for (int z = 0, v = 0; z <= res; z++)
        {
            for (int x = 0; x <= res; x++, v++)
            {
                xPos = Mathf.Clamp(x * resStep / PixelsPerUnit, 0, res * resStep / PixelsPerUnit);
                zPos = Mathf.Clamp(z * resStep / PixelsPerUnit, 0, res * resStep / PixelsPerUnit);

                lowX = Mathf.Floor(x / lowVertPerHighVert);
                lowZ = Mathf.Floor(z / lowVertPerHighVert);
                height = Heights[Mathf.Clamp((int)(x * heightmapStep), 0, Heights.GetLength(0) - 1), Mathf.Clamp((int)(z * heightmapStep), 0, Heights.GetLength(0) - 1)] * (float)PixelsToHeight;


                vertices[v] = new Vector3(xPos, height, zPos);

                normals[v] = Vector3.zero;
                uv[v] = new Vector2(x * resStep * uvStep, z * resStep * uvStep);
            }
        }

        triangles = new int[res * res * 6];
        for (int t = 0, v = 0, y = 0; y < res; y++, v++)
        {
            for (int x = 0; x < res; x++, v++, t += 6)
            {
                triangles[t] = v;
                triangles[t + 1] = v + res + 1;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + res + 1;
                triangles[t + 5] = v + res + 2;
            }
        }

        RecalculateNormals();

        Busy = false;
        MeshReady = true;
    }

    private Vector3 a, b, normal;

    public void RecalculateNormals()
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            a = vertices[triangles[i]] - vertices[triangles[i + 1]];
            b = vertices[triangles[i]] - vertices[triangles[i + 2]];

            normal = Vector3.Cross(a, b);
            normals[triangles[i]] += normal;
            normals[triangles[i + 1]] += normal;
            normals[triangles[i + 2]] += normal;

            normals[triangles[i]].Normalize();
            normals[triangles[i + 1]].Normalize();
            normals[triangles[i + 2]].Normalize();
        }
    }

}


public class ChunkManagerView : ChunkManagerViewBase {

    public bool drawGizmo = true;


    #region Chunks

    public GameObject ChunkPrefab;


    // How many chunks in length can the player see at a time
    public int ChunkViewRange = 8;

    public Chunk[,] Chunks;

    public int[] ChunksLODs;

    public AnimationCurve CamHeightResCurve;


    public int ChunkCollisionResolution;


    #endregion

    #region Trees

    public int HexTreeDensity;
    public GameObject TreeChunkPrefab;

    #endregion

    #region Privates

    // Camera
    private Transform PlayerCamera;
    private Vector3 CameraPos;
    private Vector2Int CameraChunkIndex;

    // Terrain
    private bool GeneratedTerrain = false;
    private int ChunkSize;
    private float ChunkWidth;
    private int ChunkCountX;
    private int ChunkCountY;


    // Mesh data
    private int[] triangles;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uv;


    // Used for rendering various terrain textures 
    private Material DrawMaterial;
    private RenderTexture RenderTexture;


    #endregion

    Thread threadLOD;


    protected override IEnumerator Start()
    {
 	 
        SetupRenderSettings();

        return base.Start();
    }

    private void SetupRenderSettings()
    {
        PlayerCamera = Camera.main.transform;


        // Make sure substance is using multithreading for optimal performance
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;


        DrawMaterial = new Material(Shader.Find("GUI/Text Shader"));
        DrawMaterial.hideFlags = HideFlags.HideAndDontSave;
        DrawMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    private void SetupChunkSettings()
    {
        Chunk.PixelsPerUnit = Terrain.PixelsPerUnit;
        Chunk.PixelsToHeight = Terrain.PixelsToHeight;
        ChunkCountX = Terrain.ChunkCountX();
        ChunkCountY = Terrain.ChunkCountY();
        ChunkSize = Terrain.ChunkSize;
        ChunkWidth = Terrain.ChunkSize / Terrain.PixelsPerUnit;


        Chunks = new Chunk[ChunkCountX, ChunkCountY];

        for (int x = 0; x < ChunkCountX; x++)
        {
            for (int y = 0; y < ChunkCountY; y++)
            {
                Chunks[x, y] = new Chunk();
            }
        }
    }


    public void FixedUpdate()
    {

        if (GeneratedTerrain)
        {
            // Update chunks at the phisics interval
            CalcLods();
        }
    }


    public override void GenerateChunksExecuted(GenerateChunksCommand command)
    {
        StartCoroutine(GenerateChunks());
    }

    private IEnumerator GenerateChunks()
    {
        SetupChunkSettings();

        Vector3 ChunkPos;

        for (int x = 0; x < ChunkCountX; x++)
        {
            for (int y = 0; y < ChunkCountY; y++)
            {
                ChunkPos = Terrain.ChunkWorldPos(x, y);


                Chunks[x, y].Obj = InstantiateView(ChunkPrefab, Terrain.Chunks[x, y], ChunkPos, Quaternion.identity).gameObject;
                Chunks[x, y].Obj.name = "Chunk (" + x + ", " + y + ")";


                UpdateChunkTextures(Chunks[x, y].Obj, x, y);
                UpdateChunkCollisionMesh(x, y);

                yield return null;
            }
        }

        StitchChunkHeights();
        GeneratedTerrain = true;
        GenerateTrees();

        Timer.Print();

        yield return null;
    }

    private void StitchChunkHeights()
    {
        float avarage = 0;

        for (int ChunkX = 0; ChunkX < ChunkCountX; ChunkX++)
        {
            for (int ChunkY = 0; ChunkY < ChunkCountY; ChunkY++)
            {
                // Stitch the top and bottom
                for (int x = 0; x < ChunkSize; x++)
                {
                    // top
                    if (ChunkY + 1 < ChunkCountY)
                    {
                        avarage = (Chunks[ChunkX, ChunkY].Heights[x, ChunkSize - 1] + Chunks[ChunkX, ChunkY + 1].Heights[x, 0]) / 2;

                        Chunks[ChunkX, ChunkY + 1].Heights[x, 0] = avarage;
                        Chunks[ChunkX, ChunkY].Heights[x, ChunkSize - 1] = avarage;
                    }

                    // bottom
                    if (ChunkY - 1 >= 0)
                    {
                        avarage = (Chunks[ChunkX, ChunkY].Heights[x, 0] + Chunks[ChunkX, ChunkY - 1].Heights[x, ChunkSize - 1]) / 2;

                        Chunks[ChunkX, ChunkY].Heights[x, 0] = avarage;
                        Chunks[ChunkX, ChunkY - 1].Heights[x, ChunkSize - 1] = avarage;
                    }
                }

                // Stitch right and left
                for (int y = 0; y < ChunkSize; y++)
                {
                    // right
                    if (ChunkX + 1 < ChunkCountX)
                    {
                        avarage = (Chunks[ChunkX, ChunkY].Heights[ChunkSize - 1, y] + Chunks[ChunkX + 1, ChunkY].Heights[0, y]) / 2;

                        Chunks[ChunkX + 1, ChunkY].Heights[0, y] = avarage;
                        Chunks[ChunkX, ChunkY].Heights[ChunkSize - 1, y] = avarage;
                    }

                    // bottom
                    if (ChunkX - 1 >= 0)
                    {
                        avarage = (Chunks[ChunkX, ChunkY].Heights[0, y] + Chunks[ChunkX - 1, ChunkY].Heights[ChunkSize - 1, y]) / 2;

                        Chunks[ChunkX - 1, ChunkY].Heights[ChunkSize - 1, y] = avarage;
                        Chunks[ChunkX, ChunkY].Heights[0, y] = avarage;
                    }
                }
            }
        }
    }

    List<Thread> threadList = new List<Thread>();

    public void CalcLods()
    {
        CameraChunkIndex = new Vector2Int((int)(Mathf.Round((PlayerCamera.transform.position.x - ChunkWidth / 2) / ChunkWidth) * ChunkWidth / ChunkWidth),
                                     (int)(Mathf.Round((PlayerCamera.transform.position.z - ChunkWidth / 2) / ChunkWidth) * ChunkWidth / ChunkWidth));


        for (int x = -ChunkViewRange; x < ChunkViewRange; x++)
        {
            for (int y = -ChunkViewRange; y < ChunkViewRange; y++)
            {
                ChunkIndexX = x + CameraChunkIndex.x;
                ChunkIndexY = y + CameraChunkIndex.y;

                // if the chunk view is out of the bounds of the world
                if (ChunkIndexX >= ChunkCountX || ChunkIndexY >= ChunkCountY || ChunkIndexX < 0 || ChunkIndexY < 0)
                {
                    continue;
                }

                // Logic starts

                // Build chunk if mesh is ready
                if (Chunks[ChunkIndexX, ChunkIndexY].MeshReady)
                {
                    // if the mesh is ready this means that the thread is finished 
                    ThreadCount -= 1;

                    BuildChunkMesh(ChunkIndexX, ChunkIndexY);

                    Chunks[ChunkIndexX, ChunkIndexY].MeshReady = false;
                }

                // Resolution based on distance
                Resolution = LODRings[(int)Mathf.Clamp(Vector2.Distance(new Vector2(ChunkIndexX, ChunkIndexY), new Vector2(CameraChunkIndex.x, CameraChunkIndex.y)), 0, LODRings.Length - 1)];

                // If the chunk resolution hasen't been updated
                if (Chunks[ChunkIndexX, ChunkIndexY].Resolution != Resolution && !Chunks[ChunkIndexX, ChunkIndexY].Busy && ThreadCount < ThreadCountMax)
                {
                    // Chunk mesh should build based on its heights
                    // Mesh should take into account it's neighbors

                    Chunks[ChunkIndexX, ChunkIndexY].Resolution = Resolution;

                    ThreadCount += 1;

                    threadList.Add(new Thread(Chunks[ChunkIndexX, ChunkIndexY].GenMeshData));
                }
            }
        }

        for (int i = 0; i < threadList.Count; i++)
        {
            threadList[i].Start();
        }

        threadList.Clear();
    }

    private void BuildChunkMesh(int X, int Y)
    {
        Mesh mesh = Chunks[X, Y].Obj.GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        mesh.vertices = Chunks[X, Y].vertices;
        mesh.uv = Chunks[X, Y].uv;
        mesh.triangles = Chunks[X, Y].triangles;
        mesh.normals = Chunks[X, Y].normals;
    }



    public int[] LODRings;
    public int ThreadCount = 0;
    public int ThreadCountMax = 5;
    int ChunkIndexX, ChunkIndexY, Resolution, UpdatedChunks;

    // Run a loop through all the chunks to set their mesh resolutions
    private void UpdateChunkLODs()
    {
        // Get the index of the chunk that the PlayerCamera is hovering over
        CameraChunkIndex = new Vector2Int((int)(Mathf.Round((PlayerCamera.transform.position.x - ChunkWidth / 2) / ChunkWidth) * ChunkWidth / ChunkWidth),
                                          (int)(Mathf.Round((PlayerCamera.transform.position.z - ChunkWidth / 2) / ChunkWidth) * ChunkWidth / ChunkWidth));


        UpdatedChunks = 0;

        // ring loop
        // chunk loop

        for (int x = -ChunkViewRange; x < ChunkViewRange; x++)
        {
            for (int y = -ChunkViewRange; y < ChunkViewRange; y++)
            {
                ChunkIndexX = x + CameraChunkIndex.x;
                ChunkIndexY = y + CameraChunkIndex.y;

                // if the chunk view is out of the bounds of the world
                if (ChunkIndexX >= ChunkCountX || ChunkIndexY >= ChunkCountY || ChunkIndexX < 0 || ChunkIndexY < 0)
                {
                    continue;
                }

                Resolution = (int)Vector2.Distance(new Vector2(ChunkIndexX, ChunkIndexY), new Vector2(CameraChunkIndex.x, CameraChunkIndex.y));

                //// the four chunks near the camera
                //if ((CameraChunkIndex.x <= ChunkIndexX && CameraChunkIndex.x >= ChunkIndexX - 1) ||
                //    (CameraChunkIndex.x <= ChunkIndexY && CameraChunkIndex.x >= ChunkIndexY - 1))
                //{
                //    Resolution -= Mathf.RoundToInt(CamHeightResCurve.Evaluate(PlayerCamera.transform.position.y));
                //}

                if (CameraChunkIndex.x == ChunkIndexX && CameraChunkIndex.y == ChunkIndexY)
                {
                    //  Resolution += 1;
                }

                Resolution = Mathf.Clamp(Resolution, 0, LODRings.Length - 1);
            }
        }
    }

    private Color32[] pixels;
    int rows;

    private void UpdateChunkTextures(GameObject Chunk, int ChunkX, int ChunkY)
    {
        // generate chunk heightmap and add to array
        // set DrawMaterial textures based on (generate biome map, heightmap)

        // Texture generation
        ProceduralMaterial substance = Chunks[ChunkX, ChunkY].Obj.GetComponent<Renderer>().material as ProceduralMaterial;
        substance.isReadable = true;

        // Give the substance DrawMaterial the appropriate textures and let it process them

        substance.SetProceduralTexture("biome_mask", DrawChunkBiomemap(ChunkX, ChunkY));
        substance.SetProceduralTexture("heightmap", DrawChunkHeightmap(ChunkX, ChunkY));


        Timer.Start("Generating Substance textures " + ChunkX + ", " + ChunkY);
        substance.RebuildTexturesImmediately();
        Timer.End();


        Timer.Start("Seetting pixel setting " + ChunkX + ", " + ChunkY);

        // Retrieve the processed heightmap procedural texture
        ProceduralTexture substanceTexture = substance.GetGeneratedTexture("terrain_heightmap");

        pixels = substanceTexture.GetPixels32(0, 0, substanceTexture.width, substanceTexture.height);

        // Convert it to a readable Texture2D format
        //Chunks[x, y].Heightmap = new Texture2D(substanceTexture.width, substanceTexture.height, TextureFormat.ARGB32, false);
        //Chunks[x, y].Heightmap.SetPixels32(pixels);
        //Chunks[x, y].Heightmap.wrapMode = TextureWrapMode.Clamp;

        Chunks[ChunkX, ChunkY].Heights = new float[substanceTexture.width, substanceTexture.height];


        rows = -1;
        for (int i = 0; i < pixels.Length; i++)
        {
            //Debug.Log(rows);
            //if (rows >= substanceTexture.width) Debug.Log(rows);

            if (i % substanceTexture.width == 0) rows++;

            Chunks[ChunkX, ChunkY].Heights[i % substanceTexture.height, rows] = ((Color)pixels[i]).grayscale;
        }

        //substanceTexture2D.Apply();

        // Make sure the generated textures are not tielable (causes texture issues)
        for (int i = 0; i < substance.GetGeneratedTextures().Length; i++)
        {
            substance.GetGeneratedTexture(substance.GetGeneratedTextures()[i].name).wrapMode = TextureWrapMode.Clamp;
        }

        Timer.End();
    }


    private Texture2D DrawChunkHeightmap(int ChunkX, int ChunkY)
    {
        Timer.Start("Generating heightmap " + ChunkX + ", " + ChunkY);

        int dataX = Mathf.Clamp(ChunkX * Terrain.ChunkHexCountX - 1, 0, 10000);
        int dataY = Mathf.Clamp(ChunkY * Terrain.ChunkHexCountY - 1, 0, 10000);

        int posX, posY;

        int chunkTextureOffsetX = ChunkX * ChunkSize;
        int chunkTextureOffsetY = ChunkY * ChunkSize;


        // get a temporary RenderTexture //
        RenderTexture = RenderTexture.GetTemporary(ChunkSize, ChunkSize);

        // set the RenderTexture as global target (that means GL too)
        RenderTexture.active = RenderTexture;

        // clear GL //
        GL.Clear(false, true, Color.black);

        // render GL immediately to the active render texture //
        DrawMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, ChunkSize, ChunkSize, 0);


        GL.Begin(GL.TRIANGLES);


        for (int x = dataX; x <= dataX + Terrain.ChunkHexCountX + ChunkX + 1 && x < Terrain.Width; x++)
        {
            for (int y = dataY; y <= dataY + Terrain.ChunkHexCountY + ChunkY + 1 && y < Terrain.Height; y++)
            {
                posX = Mathf.RoundToInt(x * 2 * HexProperties.tileR + (y % 2 == 0 ? 0 : 1) * HexProperties.tileR + HexProperties.tileR) - chunkTextureOffsetX;
                posY = ChunkSize - (Mathf.RoundToInt(y * (HexProperties.tileH + HexProperties.side) + HexProperties.side) - chunkTextureOffsetY);

                GL.Color(new Color((float)Terrain.Hexes[x, y].Elevation / Terrain.Elevations, (float)Terrain.Hexes[x, y].Elevation / Terrain.Elevations, (float)Terrain.Hexes[x, y].Elevation / Terrain.Elevations));

                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[1].x, posY + HexProperties.vertPos[1].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[2].x, posY + HexProperties.vertPos[2].y, 0);


                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[2].x, posY + HexProperties.vertPos[2].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[3].x, posY + HexProperties.vertPos[3].y, 0);

                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[3].x, posY + HexProperties.vertPos[3].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[4].x, posY + HexProperties.vertPos[4].y, 0);

                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[4].x, posY + HexProperties.vertPos[4].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[5].x, posY + HexProperties.vertPos[5].y, 0);
            }
        }

        GL.End();
        GL.PopMatrix();


        // read the active RenderTexture into a new Texture2D //
        Texture2D newTexture = new Texture2D(ChunkSize, ChunkSize);
        newTexture.ReadPixels(new Rect(0, 0, ChunkSize, ChunkSize), 0, 0);


        // apply pixels and compress //
        newTexture.Apply(false);
        newTexture.Compress(true); // might not want to compress! check later

        // clean up after the party //
        //RenderTexture.active = null;
        //RenderTexture.ReleaseTemporary(RenderTexture);

        Timer.End();

        return newTexture;
    }

    private Texture2D DrawChunkBiomemap(int ChunkX, int ChunkY)
    {
        Timer.Start("Generating biomemap " + ChunkX + ", " + ChunkY);


        int dataX = Mathf.Clamp(ChunkX * Terrain.ChunkHexCountX - 1, 0, 10000);
        int dataY = Mathf.Clamp(ChunkY * Terrain.ChunkHexCountY - 1, 0, 10000);

        int posX, posY;

        int chunkTextureOffsetX = ChunkX * ChunkSize;
        int chunkTextureOffsetY = ChunkY * ChunkSize;


        // get a temporary RenderTexture //
        RenderTexture = RenderTexture.GetTemporary(ChunkSize, ChunkSize);

        // set the RenderTexture as global target (that means GL too)
        RenderTexture.active = RenderTexture;

        // clear GL //
        GL.Clear(false, true, Color.black);

        // render GL immediately to the active render texture //
        DrawMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, ChunkSize, ChunkSize, 0);


        GL.Begin(GL.TRIANGLES);


        for (int x = dataX; x <= dataX + Terrain.ChunkHexCountX + ChunkX + 1 && x < Terrain.Width; x++)
        {
            for (int y = dataY; y <= dataY + Terrain.ChunkHexCountY + ChunkY + 1 && y < Terrain.Height; y++)
            {
                posX = Mathf.RoundToInt(x * 2 * HexProperties.tileR + (y % 2 == 0 ? 0 : 1) * HexProperties.tileR + HexProperties.tileR) - chunkTextureOffsetX;
                posY = ChunkSize - (Mathf.RoundToInt(y * (HexProperties.tileH + HexProperties.side) + HexProperties.side) - chunkTextureOffsetY);

                GL.Color(new Color(Terrain.TerrainTypesList.TerrainTypes[(int)Terrain.Hexes[x, y].TerrainType].Color.r,
                                   Terrain.TerrainTypesList.TerrainTypes[(int)Terrain.Hexes[x, y].TerrainType].Color.g,
                                   Terrain.TerrainTypesList.TerrainTypes[(int)Terrain.Hexes[x, y].TerrainType].Color.b));

                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[1].x, posY + HexProperties.vertPos[1].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[2].x, posY + HexProperties.vertPos[2].y, 0);


                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[2].x, posY + HexProperties.vertPos[2].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[3].x, posY + HexProperties.vertPos[3].y, 0);

                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[3].x, posY + HexProperties.vertPos[3].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[4].x, posY + HexProperties.vertPos[4].y, 0);

                GL.Vertex3(posX + HexProperties.vertPos[0].x, posY + HexProperties.vertPos[0].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[4].x, posY + HexProperties.vertPos[4].y, 0);
                GL.Vertex3(posX + HexProperties.vertPos[5].x, posY + HexProperties.vertPos[5].y, 0);
            }
        }

        GL.End();
        GL.PopMatrix();


        // read the active RenderTexture into a new Texture2D //
        Texture2D newTexture = new Texture2D(ChunkSize, ChunkSize);
        newTexture.ReadPixels(new Rect(0, 0, ChunkSize, ChunkSize), 0, 0);


        // apply pixels and compress //
        newTexture.Apply(false);
        newTexture.Compress(true); // might not want to compress! check later

        // clean up after the party //
        //RenderTexture.active = null;
        //RenderTexture.ReleaseTemporary(RenderTexture);

        Timer.End();

        return newTexture;
    }


    private void UpdateChunkCollisionMesh(int ChunkX, int ChunkY)
    {

        //MeshCollider meshCollider = Chunks[ChunkX, ChunkY].GetComponent<MeshCollider>(); //Chunks[ChunkX, ChunkY].GetComponent<MeshCollider>().sharedMesh;
        Mesh meshCollider = new Mesh();

        //Debug.Log(Chunks[ChunkX, ChunkY].GetComponent<MeshCollider>());
        //meshCollider.Clear();

        vertices = new Vector3[(ChunkCollisionResolution + 1) * (ChunkCollisionResolution + 1)];

        // Pixels per vetex point
        //float resStep = ChunkWidth / ChunkCollisionResolution;
        float resStep = (float)ChunkSize / (float)ChunkCollisionResolution;
        float heightmapStep = (float)Chunks[ChunkX, ChunkY].Heights.GetLength(0) / (float)ChunkCollisionResolution;


        for (int v = 0, z = 0; z <= ChunkCollisionResolution; z++)
        {
            for (int x = 0; x <= ChunkCollisionResolution; x++, v++)
            {
                vertices[v] = new Vector3(x * resStep / Terrain.PixelsPerUnit,
                                          Chunks[ChunkX, ChunkY].Heights[(int)(x * heightmapStep), (int)(z * heightmapStep)] * Terrain.PixelsToHeight,
                                          z * resStep / Terrain.PixelsPerUnit);

            }
        }

        meshCollider.vertices = vertices;

        int[] triangles = new int[ChunkCollisionResolution * ChunkCollisionResolution * 6];
        for (int t = 0, v = 0, y = 0; y < ChunkCollisionResolution; y++, v++)
        {
            for (int x = 0; x < ChunkCollisionResolution; x++, v++, t += 6)
            {
                triangles[t] = v;
                triangles[t + 1] = v + ChunkCollisionResolution + 1;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + ChunkCollisionResolution + 1;
                triangles[t + 5] = v + ChunkCollisionResolution + 2;
            }
        }

        meshCollider.triangles = triangles;
        meshCollider.RecalculateBounds();

        //meshCollider.sharedMesh = newMesh;
        Chunks[ChunkX, ChunkY].Obj.GetComponent<MeshCollider>().sharedMesh = meshCollider;
    }


    public ChunkTrees[,] ChunkTreesList;
    public Material[] TreeMaterials;
    public int TreeCount;
    public float HexTreeRadius = 0.4f;

    public class ChunkTrees
    {
        public List<Vector3> trees = new List<Vector3>();
    }

    private void GenerateTrees()
    {
        ChunkTreesList = new ChunkTrees[ChunkCountX, ChunkCountY];

        for (int x = 0; x < ChunkTreesList.GetLength(0); x++)
        {
            for (int y = 0; y < ChunkTreesList.GetLength(1); y++)
            {
                ChunkTreesList[x, y] = new ChunkTrees();
            }
        }

        int rand = 0;

        for (int x = 0; x < Terrain.Hexes.GetLength(0); x++)
        {
            for (int y = 0; y < Terrain.Hexes.GetLength(1); y++)
            {
                rand = UnityEngine.Random.Range(0, 10);

                if (Terrain.Hexes[x, y].WaterHex() || Terrain.Hexes[x, y].TerrainType == TerrainType.Arctic || Terrain.Hexes[x, y].TerrainType == TerrainType.Desert || Terrain.Hexes[x, y].TerrainType == TerrainType.ColdDesert || rand >= 6) continue;

                for (int i = 0; i < TreeCount; i++)
                {

                    Vector3 pos = Terrain.Hexes[x, y].WorldPos + new Vector3(UnityEngine.Random.Range(-HexProperties.unityWidth, HexProperties.unityWidth) * HexTreeRadius,
                                                                             0,
                                                                             UnityEngine.Random.Range(-HexProperties.unityWidth, HexProperties.unityWidth) * HexTreeRadius);

                    Vector3 castFrom = new Vector3(pos.x, 50, pos.z);
                    Vector3 castTo = new Vector3(pos.x, -50, pos.z);
                    RaycastHit info;

                    if (Physics.Linecast(castFrom, castTo, out info))
                        pos.y = info.point.y;
                    else
                        continue;

                    //Material mat = TreeMaterials[UnityEngine.Random.Range(0, TreeMaterials.Length)];


                    int chunkX = Mathf.Clamp(Mathf.FloorToInt(pos.x / ChunkWidth), 0, ChunkCountX - 1);
                    int chunkY = Mathf.Clamp(Mathf.FloorToInt(pos.z / ChunkWidth), 0, ChunkCountY - 1);

                    ChunkTreesList[chunkX, chunkY].trees.Add(pos);


                    if (ChunkTreesList[chunkX, chunkY].trees.Count == 10666) // max quads per mesh (42 664 indices)
                    {
                        BuildChunkTrees(ChunkTreesList[chunkX, chunkY]);
                        ChunkTreesList[chunkX, chunkY].trees.Clear(); // clear quads list for next mesh
                    }
                }
            }
        }

        for (int x = 0; x < ChunkTreesList.GetLength(0); x++)
        {
            for (int y = 0; y < ChunkTreesList.GetLength(1); y++)
            {
                BuildChunkTrees(ChunkTreesList[x, y]);
            }
        }
    }

    public float TreeSize = 0.5f;
    public float TreeRandomizeSize;

    // each tree vertices
    Vector2 qv0 = new Vector2(-1, -1);
    Vector2 qv1 = new Vector2(1, -1);
    Vector2 qv2 = new Vector2(1, 1);
    Vector2 qv3 = new Vector2(-1, 1);

    // uv frame shift (4 frames per 4 rows in tree texture)
    const float frameSize = 1.0f / 4.0f;

    public float Rows;

    // each tree uvs
    Vector2 uv0 = new Vector2(1, 0);
    Vector2 uv1 = new Vector2(0, 0);
    Vector2 uv2 = new Vector2(0, 1);
    Vector2 uv3 = new Vector2(1, 1);


    public void BuildChunkTrees(ChunkTrees ChunkTrees)
    {
        //if (ChunkTrees.trees.Count == 0) return;
        //Debug.Log(ChunkTrees.trees.Count);

        Vector3 tree;
        Vector3[] verts = new Vector3[ChunkTrees.trees.Count * 4]; // sprite center
        Vector2[] uvs = new Vector2[ChunkTrees.trees.Count * 4]; // trees uvs
        Vector2[] uvs2 = new Vector2[ChunkTrees.trees.Count * 4]; // sprite corner

        int[] indices = new int[ChunkTrees.trees.Count * 4]; // quads in mesh

        float rowX, rowY;
        float rowConst = 1.0f / Rows;

        for (int i = 0; i < ChunkTrees.trees.Count; i++) // fill arrays
        {

            int ii = i * 4;

            float scale = TreeSize - UnityEngine.Random.value * TreeRandomizeSize;

            rowX = (int)UnityEngine.Random.Range(-1, (int)Rows - 1);
            rowY = (int)UnityEngine.Random.Range(-1, (int)Rows - 1);


            // sprite corners
            uvs2[ii] = qv0 * scale;
            uvs2[ii + 1] = qv1 * scale;
            uvs2[ii + 2] = qv2 * scale;
            uvs2[ii + 3] = qv3 * scale;

            uvs[ii + 0] = new Vector2(rowConst * (rowX + 1), rowConst * rowY);
            uvs[ii + 1] = new Vector2(rowConst * rowX, rowConst * rowY);
            uvs[ii + 2] = new Vector2(rowConst * rowX, rowConst * (rowY + 1));
            uvs[ii + 3] = new Vector2(rowConst * (rowX + 1), rowConst * (rowY + 1));

            indices[ii] = ii;
            indices[ii + 1] = ii + 1;
            indices[ii + 2] = ii + 2;
            indices[ii + 3] = ii + 3;

            // push tree up from groung on it height
            tree = ChunkTrees.trees[i];
            tree.y += qv2.y * scale;

            // sprite center position is same for all 4 corners
            verts[ii] = tree;
            verts[ii + 1] = tree;
            verts[ii + 2] = tree;
            verts[ii + 3] = tree;
        }

        // creating mesh
        GameObject trees = new GameObject("Blah");

        MeshFilter mf = trees.AddComponent<MeshFilter>();

        mf.sharedMesh = new Mesh();
        mf.sharedMesh.vertices = verts;

        mf.sharedMesh.uv = uvs;
        mf.sharedMesh.uv2 = uvs2;

        mf.sharedMesh.SetIndices(indices, MeshTopology.Quads, 0);

        MeshRenderer mr = trees.AddComponent<MeshRenderer>();
        mr.sharedMaterial = TreeMaterials[UnityEngine.Random.Range(0, TreeMaterials.Length)];

        //if (castShadows)
        //    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        //else
        //    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        trees.AddComponent<TurboForestChunk>();
        trees.transform.parent = transform;
    }
}
