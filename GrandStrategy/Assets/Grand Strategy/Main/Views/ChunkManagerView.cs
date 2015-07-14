using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


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

public class ChunkManagerView : ChunkManagerViewBase
{

    public bool drawGizmo = true;


    #region Chunks

    public GameObject ChunkPrefab;


    // How many chunks in length can the player see at a time
    public int ChunkViewRange = 8;

    public Chunk[,] Chunks;

    public int[] ChunksLODs;

    public AnimationCurve CamHeightResCurve;


    public int ChunkCollisionResolution;



    //private GameObject[,] ChunkObjs;
    //private ChunkLOD[,]   ChunkLODs;
    //private Texture2D[,]  ChunkHeightmaps;

    
    //public AnimationCurve ChunkLODCurve;
    //public Color32[]      ChunkLODColors;

    //public class ChunkLOD
    //{
    //    public bool visible;
    //    public bool needsToUpdate;
    //    public int resolution;
    //}

    #endregion

    #region Trees

    public int HexTreeDensity;
    public GameObject TreeChunkPrefab;

    #endregion

    #region Privates

    // Camera
    private Transform  PlayerCamera;
    private Vector3    CameraPos;
    private Vector2Int CameraChunkIndex;

    // Terrain
    private bool  GeneratedTerrain = false;
    private int   ChunkSize;
    private float ChunkWidth;
    private int   ChunkCountX;
    private int   ChunkCountY;


    // Mesh data
    private int[]     triangles;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uv;


    // Used for rendering various terrain textures 
    private Material DrawMaterial;
    private RenderTexture RenderTexture;

    
 #endregion

    Thread threadLOD;

    public override void Start()
    {
        base.Start();

        SetupRenderSettings();
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
        ChunkSize =   Terrain.ChunkSize;
        ChunkWidth =  Terrain.ChunkSize / Terrain.PixelsPerUnit;


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

        GeneratedTerrain = true;
        GenerateTrees();

        //Timer.Print();

        yield return null;
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

        //if (threadLOD != null && !threadLOD.IsAlive)
        //    threadLOD.Start();  

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
    public int ThreadCount    = 0;
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


                //if (ThreadCount < ThreadCountMax && Resolution != Chunks[ChunkIndexX, ChunkIndexY].Resolution &&
                //    Chunks[ChunkIndexX, ChunkIndexY].NeedsToUpdate == false && Chunks[ChunkIndexX, ChunkIndexY].BuildingMesh == false)
                //{
                //    Chunks[ChunkIndexX, ChunkIndexY].NeedsToUpdate = true;
                //}
                //
                //if (Chunks[ChunkIndexX, ChunkIndexY].MeshReady == true)
                //{
                //    Debug.Log("building mesh");
                //    BuildChunkMesh(ChunkIndexX, ChunkIndexY);
                //    Chunks[ChunkIndexX, ChunkIndexY].NeedsToUpdate = false;
                //    Chunks[ChunkIndexX, ChunkIndexY].BuildingMesh = false;
                //    Chunks[ChunkIndexX, ChunkIndexY].MeshReady = false;
                //    Chunks[ChunkIndexX, ChunkIndexY].Resolution = Resolution;
                //}

            }  
        }

        //for (int x = 0; x < Chunks.GetLength(0); x++)
        //{
        //    for (int y = 0; y < Chunks.GetLength(1); y++)
        //    {
        //        if (ThreadCount < ThreadCountMax &&
        //            Chunks[x, y].NeedsToUpdate == true &&
        //            Chunks[x, y].BuildingMesh == false)
        //        {
        //            ThreadCount += 1;
        //
        //            new Thread(Chunks[x, y].CalcMesh).Start();
        //        }
        //    }
        //
        //}

        //Timer.Print();

        /*
        for (int x = -ChunkViewRange; x < ChunkViewRange; x++)
        {
            for (int y = -ChunkViewRange; y < ChunkViewRange; y++)
            {
                ChunkIndexX = x + CameraChunkIndex.x;
                ChunkIndexY = y + CameraChunkIndex.y;

                // if the chunk view is out of the bounds of the world
                if (ChunkIndexX > ChunkCountX || ChunkIndexY > ChunkCountY || ChunkIndexX < 0 || ChunkIndexY < 0)
                {
                    continue;
                }

                newRes = (int)ChunkLODCurve.Evaluate(Vector3.Distance(Terrain.ChunkCenterWorldPos(ChunkIndexX, ChunkIndexY), CameraPos));

                if (ChunkLODs[ChunkIndexX, ChunkIndexY].resolution != newRes)
                {

                    ChunkLODs[ChunkIndexX, ChunkIndexY].visible = true;
                    ChunkLODs[ChunkIndexX, ChunkIndexY].needsToUpdate = true;
                    ChunkLODs[ChunkIndexX, ChunkIndexY].resolution = newRes;

                    // Make sure to update the surrounding chunks to fix the seams
                    // Top
                    if (ChunkIndexY < ChunkCountY)
                    {
                        ChunkLODs[ChunkIndexX, ChunkIndexY + 1].needsToUpdate = true;
                    }

                    // Bottom
                    if (ChunkIndexY > 0)
                    {
                        ChunkLODs[ChunkIndexX, ChunkIndexY - 1].needsToUpdate = true;
                    }

                    // Right
                    if (ChunkIndexX < ChunkCountX)
                    {
                        ChunkLODs[ChunkIndexX + 1, ChunkIndexY].needsToUpdate = true;
                    }

                    // Left
                    if (ChunkIndexX > 0)
                    {
                        ChunkLODs[ChunkIndexX - 1, ChunkIndexY].needsToUpdate = true;
                    }
                }
            }
        }

        // Update all the chunks that need updating
        bool lowerResTop = false, lowerResRight = false, lowerResBottom = false;
        for (int x = 0; x <= ChunkCountX; x++)
        {
            for (int y = 0; y <= ChunkCountY; y++)
            {
                if (ChunkLODs[x, y].needsToUpdate && ChunkLODs[x, y].visible)
                {
                    // Check if the top chunk has a lower resolution
                    if (y + 1 < ChunkCountY)
                        lowerResTop = ChunkLODs[x, y].resolution < ChunkLODs[x, y + 1].resolution;

                    // Check if the top chunk has a lower resolution
                    if (y - 1 >= 0)
                        lowerResBottom = ChunkLODs[x, y].resolution > ChunkLODs[x, y - 1].resolution;

                    // Check if the right chunk has a lower resolution
                    if (x + 1 <= ChunkCountX)
                        lowerResRight = ChunkLODs[x, y].resolution < ChunkLODs[x + 1, y].resolution;

                    UpdateChunkMesh(x, y, ChunkLODs[x, y].resolution, lowerResTop, false, lowerResRight, false);
                    
                    ChunkLODs[x, y].needsToUpdate = false;
                }
            }
        }

        /*
        for (int i = 0; i < visibleChunks.Count; i++)
        {
            int x = visibleChunks[i].x;
            int y = visibleChunks[i].y;

            // if the chunk resolution needs to be updated
            if (CurrentChunkLODs[x, y] != visibleChunkResolutions[x, y])
            {
                bool top = false;
                if (y + 1 <= Terrain.Chunks.GetLength(1))
                    top = visibleChunkResolutions[x, y] < visibleChunkResolutions[x, y + 1];

                bool right = false;
                if (x + 1 <= Terrain.Chunks.GetLength(0))
                    right = visibleChunkResolutions[x, y] < visibleChunkResolutions[x + 1, y];

                UpdateChunkMesh(x, y, visibleChunkResolutions[x, y], top, right);

                // Update the chunks around this one if they have not 
                //if (CurrentChunkLODs[x + 1, y] != visibleChunkResolutions[x + 1, y])
                //{
                //    UpdateChunkMesh(x, y, visibleChunkResolutions[x, y], top, right);
                //}

                CurrentChunkLODs[x, y] = visibleChunkResolutions[x, y];
            }
        }*/
    }

    void DoStuff()
    {
        Debug.Log("Doing stuff");
    }

  

    private void UpdateChunkMeshData(int ChunkX, int ChunkY, int resIndex, bool lowerTop, bool lowerBottom, bool lowerRight, bool lowerLeft)
    {
        Debug.Log("Updating chunk " + ChunkX + ", " + ChunkY);


        lock (Chunks[ChunkX, ChunkY])
        {
            
            Debug.Log("Trying");

            // Pixels per vetex point
            int res = 0; //LODRings[resIndex];
            Debug.Log("Got ring");

            int lowerRes = 0; // LODRings[Mathf.Clamp(resIndex + 1, 0, LODRings.Length - 1)];

            float resStep = (float)ChunkSize / (float)res;
            float lowerResStep = (float)ChunkSize / (float)lowerRes;

            float lowVertPerHighVert = (float)res / (float)lowerRes; // 2 or 1 


            float heightmapStep = (float)ChunkSize / (float)res;
            float lowerHeightmapStep = (float)ChunkSize / (float)lowerRes;
            float uvStep = 1f / ChunkSize;


            vertices = new Vector3[(res + 1) * (res + 1)];
            normals = new Vector3[vertices.Length];
            uv = new Vector2[vertices.Length];

            float xPos, zPos = 0;
            float lowX, lowZ, leftXHeight, rightXHeight, topZHeight, botZHeight, lowXFloat, lowZFloat, difference, increment = 0;
            float height = 0;


            for (int z = 0, v = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++, v++)
                {
                    Debug.Log("Doing loopp");

                    xPos = Mathf.Clamp(x * resStep / Terrain.PixelsPerUnit, 0, res * resStep / Terrain.PixelsPerUnit);
                    zPos = Mathf.Clamp(z * resStep / Terrain.PixelsPerUnit, 0, res * resStep / Terrain.PixelsPerUnit);

                    Debug.Log(Chunks[ChunkX, ChunkY]);
                    Debug.Log("Break");

                    lowX = Mathf.Floor(x / lowVertPerHighVert);
                    lowZ = Mathf.Floor(z / lowVertPerHighVert);
                    height = Chunks[ChunkX, ChunkY].Heights[Mathf.Clamp((int)(x * heightmapStep), 0, ChunkSize - 1), Mathf.Clamp((int)(z * heightmapStep), 0, ChunkSize - 1)] * (float)Terrain.PixelsToHeight;


                    vertices[v] = new Vector3(xPos, height, zPos);

                    normals[v] = Vector3.zero;
                    uv[v] = new Vector2(x * resStep * uvStep, z * resStep * uvStep);
                }
            }

            Debug.Log("Finished verts   ");


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

        }

        //TangentSolver.Solve(mesh);
        //mesh.RecalculateBounds();
        //mesh.Optimize();
        //Timer.End();
    }


    private Color32[] pixels;
    int rows;

    private void UpdateChunkTextures(GameObject chunk, int x, int y)
    {
        // generate chunk heightmap and add to array
        // set DrawMaterial textures based on (generate biome map, heightmap)

        // Texture generation
        ProceduralMaterial substance = Chunks[x, y].Obj.GetComponent<Renderer>().material as ProceduralMaterial;
        substance.isReadable = true;

        // Give the substance DrawMaterial the appropriate textures and let it process them

        substance.SetProceduralTexture("biome_mask", DrawChunkBiomemap(x, y));

        substance.SetProceduralTexture("heightmap", DrawChunkHeightmap(x, y));

        Timer.Start("Generating substnace textures " + x + ", " + y);
        substance.RebuildTexturesImmediately();
        Timer.End();

        Timer.Start("Seetting pixel setting " + x + ", " + y);

        // Retrieve the processed heightmap procedural texture
        ProceduralTexture substanceTexture = substance.GetGeneratedTexture("terrain_heightmap");

        pixels = substanceTexture.GetPixels32(0, 0, substanceTexture.width, substanceTexture.height);

        // Convert it to a readable Texture2D format
        //Chunks[x, y].Heightmap = new Texture2D(substanceTexture.width, substanceTexture.height, TextureFormat.ARGB32, false);
        //Chunks[x, y].Heightmap.SetPixels32(pixels);
        //Chunks[x, y].Heightmap.wrapMode = TextureWrapMode.Clamp;

        Chunks[x, y].Heights = new float[substanceTexture.width, substanceTexture.height];


        rows = -1;
        for (int i = 0; i < pixels.Length ; i++)
        {
            //Debug.Log(rows);
            //if (rows >= substanceTexture.width) Debug.Log(rows);

            if (i % substanceTexture.width == 0) rows++;

            Chunks[x, y].Heights[i % substanceTexture.height, rows] = ((Color)pixels[i]).grayscale;
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



    /* top seam
    if (z == res && ChunkY + 1 < ChunkCountY)
    {
        if (lowerTop == false)
        {
            height = Chunks[ChunkX, Mathf.Clamp(ChunkY + 1, 0, ChunkCountY)].Heightmap.GetPixel((int)(x * heightmapStep), 0).grayscale * (float)Terrain.PixelsToHeight; // (height + 
        }
        else
        {
            leftXHeight = Chunks[ChunkX, ChunkY + 1].Heightmap.GetPixel((int)(lowX * lowerHeightmapStep), 0).grayscale;
            rightXHeight = Chunks[ChunkX, ChunkY + 1].Heightmap.GetPixel((int)((lowX + 1) * lowerHeightmapStep), 0).grayscale;

            lowXFloat = x / ((float)res / (float)lowerRes);
            difference = rightXHeight - leftXHeight;
            increment = ((lowXFloat - (float)lowX)) / 1f;

            height = (leftXHeight + (difference * increment)) * (float)Terrain.PixelsToHeight;
        }
    }

    // bottom seam
    //if (z == 0 && ChunkY <= ChunkCountY)
    //{
    //    if (lowerBottom == false)
    //    {
    //        height = ChunkHeightmaps[ChunkX, Mathf.Clamp(ChunkY + 1, 0, Terrain.Chunks.GetLength(1) - 1)].GetPixel((int)(x * heightmapStep), -1).grayscale * (float)Terrain.PixelsToHeight; // (height + 
    //    }
    //    else
    //    {
    //        leftXHeight = ChunkHeightmaps[ChunkX, ChunkY - 1].GetPixel((int)(lowX * lowerHeightmapStep), -1).grayscale;
    //        rightXHeight = ChunkHeightmaps[ChunkX, ChunkY - 1].GetPixel((int)((lowX + 1) * lowerHeightmapStep), -1).grayscale;
    //
    //        lowXFloat = x / ((float)res / (float)lowerRes);
    //        difference = rightXHeight - leftXHeight;
    //        increment = ((lowXFloat - (float)lowX)) / 1f;
    //
    //        height = (leftXHeight + (difference * increment)) * (float)Terrain.PixelsToHeight;
    //    }
    //}

    // right seam
    if (x == res && ChunkX + 1 < ChunkCountX)
    {
        if (lowerRight == false)
        {
            height = Chunks[Mathf.Clamp(ChunkX + 1, 0, ChunkCountX), ChunkY].Heightmap.GetPixel(0, (int)(z * heightmapStep)).grayscale * (float)Terrain.PixelsToHeight;
        }
        else
        {
            topZHeight = Chunks[ChunkX + 1, ChunkY].Heightmap.GetPixel(0, (int)(lowZ * lowerHeightmapStep)).grayscale;
            botZHeight = Chunks[ChunkX + 1, ChunkY].Heightmap.GetPixel(0, (int)((lowZ + 1) * lowerHeightmapStep)).grayscale;

            lowZFloat = z / ((float)res / (float)lowerRes);
            difference = botZHeight - topZHeight;
            increment = ((lowZFloat - (float)lowZ)) / 1f;

            height = (topZHeight + (difference * increment)) * (float)Terrain.PixelsToHeight;
        }
    }*/

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

                if (Terrain.Hexes[x, y].WaterHex() || rand >= 8) continue;

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

        for (int i = 0; i < ChunkTrees.trees.Count; i++) // fill arrays
        {

            int ii = i * 4;

            float scale = TreeSize - UnityEngine.Random.value * TreeRandomizeSize;

            // sprite corners
            uvs2[ii] = qv0 * scale;
            uvs2[ii + 1] = qv1 * scale;
            uvs2[ii + 2] = qv2 * scale;
            uvs2[ii + 3] = qv3 * scale;

            uvs[ii] = uv0;
            uvs[ii + 1] = uv1;
            uvs[ii + 2] = uv2;
            uvs[ii + 3] = uv3;

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
