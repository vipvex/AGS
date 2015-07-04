using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


public class ChunkManagerView : ChunkManagerViewBase
{

    #region Chunks

    public GameObject ChunkPrefab;
    public float ChunkMeshResolution;
    public int ChunkCollisionResolution;

    private GameObject[,] Chunks;
    private Texture2D[,]  ChunkHeightmaps;
    private ChunkLOD[,]   ChunkLODs;

    [Space(10)]
    public int            ChunkViewRange = 8;
    public AnimationCurve ChunkLODCurve;
    public int[]          ChunkLODRes;
    public Color32[]      ChunkLODColors;

    #endregion

    #region Trees

    public GameObject TreePrefab;
    public int ChunkTreeDensity;

    #endregion

    #region Privates
    [Space(10)]
    public bool drawGizmo = true;

    private bool GeneratedTerrain = false;
    private int ChunkCountX;
    private int ChunkCountY;
    

    private RenderTexture renderTexture;
    private Material material;
    private Transform camera;


    private float ChunkSize;
    private Vector3 CameraPos;
    private Vector2Int CameraChunkIndex;



    private int[] triangles;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uv;
#endregion


    public class ChunkLOD
    {
        public bool visible;
        public bool needsToUpdate;
        public int resolution;
    }


    public override void Start()
    {
        base.Start();
        SetupRenderingSettings();
    }

    private void SetupChunkSettings()
    {
        ChunkLODs = new ChunkLOD[Chunks.GetLength(0), Chunks.GetLength(1)];

        for (int x = 0; x < ChunkLODs.GetLength(0); x++)
        {
            for (int y = 0; y < ChunkLODs.GetLength(1); y++)
            {
                ChunkLODs[x, y] = new ChunkLOD();
            }
        }
        ChunkSize = Terrain.ChunkSize / Terrain.PixelsPerUnit;
    }

    private void SetupRenderingSettings()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;

        material = new Material(Shader.Find("GUI/Text Shader"));
        material.hideFlags = HideFlags.HideAndDontSave;
        material.shader.hideFlags = HideFlags.HideAndDontSave;
        camera = Camera.main.transform;
    }

    public void FixedUpdate()
    {
        if (GeneratedTerrain)
            UpdateChunkResolutions();
    }

    public override void GenerateChunksExecuted(GenerateChunksCommand command)
    {
        StartCoroutine(GenerateChunkObjects());
    }

    // Run a loop through all the chunks to set their mesh resolutions
    private void UpdateChunkResolutions()
    {
        // Get the index of the chunk that the camera is hovering over
        CameraPos =        Camera.main.transform.position;
        CameraChunkIndex = new Vector2Int((int)((int)(CameraPos.x / ChunkSize) * ChunkSize / ChunkSize), 
                                          (int)((int)(CameraPos.z / ChunkSize) * ChunkSize / ChunkSize));

        int newRes = 0, ChunkIndexX = 0, ChunkIndexY = 0;


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

    void OnDrawGizmos()
    {
        if (!drawGizmo || (Terrain == null || Terrain.Chunks == null)) return;

        //Mesh mesh = null;
        //
        //for (int x = 0; x < 1; x++)
        //{
        //    for (int y = 0; y < 1; y++)
        //    {
        //        mesh = Chunks[x, y].GetComponent<MeshFilter>().mesh;
        //
        //        for (int i = 0; i < mesh.normals.Length; i++)
        //        {
        //            Gizmos.DrawRay(mesh.vertices[i], mesh.normals[i]);
        //        }
        //    }
        //}


        //float ChunkSize = Terrain.ChunkSize / Terrain.PixelsPerUnit;
        //
        //Vector3 cameraPos = Camera.main.transform.position;
        //Vector3 cameraChunkPos = new Vector3((int)(cameraPos.x / ChunkSize) * ChunkSize, 0, (int)(cameraPos.z / ChunkSize) * ChunkSize);
        //Vector2Int cameraIndex = new Vector2Int((int)(cameraChunkPos.x / ChunkSize), (int)(cameraChunkPos.z / ChunkSize));
        //
        //List<Vector2Int> visibleChunks = new List<Vector2Int>();
        //visibleChunks.Add(cameraIndex);
        //for (int x = -ChunkViewRange; x < ChunkViewRange + 1; x++)
        //{
        //    for (int y = -ChunkViewRange; y < ChunkViewRange + 1; y++)
        //    {        
        //        visibleChunks.Add(new Vector2Int(cameraIndex.x + x, cameraIndex.y + y));
        //    }
        //}
        //
        //
        //Gizmos.color = Color.blue;
        //for (int x = 0; x < Terrain.Chunks.GetLength(0); x++)
        //{
        //    for (int y = 0; y < Terrain.Chunks.GetLength(1); y++)
        //    {
        //        if (visibleChunks.Contains(new Vector2Int(x, y)))
        //        {
        //            Gizmos.color = (Color)ChunkLODColors[(int)ChunkLODCurve.Evaluate(Vector3.Distance(Terrain.ChunkCenterWorldPos(x, y), cameraPos))];
        //            
        //        }else{
        //            Gizmos.color = Color.blue;
        //        }
        //        
        //        Gizmos.DrawWireCube(Terrain.ChunkCenterWorldPos(x, y) + ((Vector3.up * Terrain.PixelsToHeight) / 2), Terrain.ChunkWorldSize() - Vector3.one * 0.05f);
        //    }
        //}
    }


    private IEnumerator GenerateChunkObjects()
    {

        Vector3 chunkPos;
        Chunks = new GameObject[Terrain.Chunks.GetLength(0), Terrain.Chunks.GetLength(1)];
        ChunkHeightmaps = new Texture2D[Terrain.Chunks.GetLength(0), Terrain.Chunks.GetLength(1)];

        for (int x = 0; x < Terrain.Chunks.GetLength(0); x++)
        {
            for (int y = 0; y < Terrain.Chunks.GetLength(1); y++)
            {

                chunkPos = Terrain.ChunkWorldPos(x, y);


                Chunks[x, y] = InstantiateView(ChunkPrefab, Terrain.Chunks[x, y], chunkPos, Quaternion.identity).gameObject;
                Chunks[x, y].name = "Chunk [" + x + ", " + y + "]";
                
                UpdateChunkTextures(Chunks[x, y], x, y);
                UpdateChunkCollisionMesh(x, y);
                

                yield return null;
            }

        }

        GeneratedTerrain = true;
        ChunkCountX = Terrain.Chunks.GetLength(0) - 1;
        ChunkCountY = Terrain.Chunks.GetLength(1) - 1;
        SetupChunkSettings();
        GenerateTrees();

        //StitchChunks();
        //GenerateVegetation();
        yield return null;
    }

    private void UpdateChunkTextures(GameObject chunk, int x, int y)
    {
        // generate chunk heightmap and add to array
        // set material textures based on (generate biome map, heightmap)

        // Texture generation
        ProceduralMaterial substance = Chunks[x, y].GetComponent<Renderer>().material as ProceduralMaterial;
        substance.isReadable = true;

        // Give the substance material the appropriate textures and let it process them
        substance.SetProceduralTexture("biome_mask", DrawChunkBiomemap(x, y));
        substance.SetProceduralTexture("heightmap", DrawChunkHeightmap(x, y));
        substance.RebuildTexturesImmediately();

        // Retrieve the processed heightmap procedural texture
        ProceduralTexture substanceTexture = substance.GetGeneratedTexture("terrain_heightmap");

        // Convert it to a readable Texture2D format
        ChunkHeightmaps[x, y] = new Texture2D(substanceTexture.width, substanceTexture.height, TextureFormat.ARGB32, false);
        ChunkHeightmaps[x, y].SetPixels32(substanceTexture.GetPixels32(0, 0, substanceTexture.width, substanceTexture.height));
        ChunkHeightmaps[x, y].wrapMode = TextureWrapMode.Clamp;
        //substanceTexture2D.Apply();

        // Make sure the generated textures are not tielable (causes texture issues)
        for (int i = 0; i < substance.GetGeneratedTextures().Length; i++)
        {
            substance.GetGeneratedTexture(substance.GetGeneratedTextures()[i].name).wrapMode = TextureWrapMode.Clamp;
        }
    }


    private Texture2D DrawChunkHeightmap(int ChunkX, int ChunkY)
    {
        int dataX = Mathf.Clamp(ChunkX * Terrain.ChunkHexCountX - 1, 0, 10000);
        int dataY = Mathf.Clamp(ChunkY * Terrain.ChunkHexCountY - 1, 0, 10000);

        int ChunkSize = Terrain.ChunkSize;

        int posX, posY;

        int chunkTextureOffsetX = ChunkX * ChunkSize;
        int chunkTextureOffsetY = ChunkY * ChunkSize;


        // get a temporary RenderTexture //
        renderTexture = RenderTexture.GetTemporary(ChunkSize, ChunkSize);

        // set the RenderTexture as global target (that means GL too)
        RenderTexture.active = renderTexture;

        // clear GL //
        GL.Clear(false, true, Color.black);

        // render GL immediately to the active render texture //
        material.SetPass(0);
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
        //RenderTexture.ReleaseTemporary(renderTexture);

        return newTexture;
    }

    private Texture2D DrawChunkBiomemap(int ChunkX, int ChunkY)
    {
        int dataX = Mathf.Clamp(ChunkX * Terrain.ChunkHexCountX - 1, 0, 10000);
        int dataY = Mathf.Clamp(ChunkY * Terrain.ChunkHexCountY - 1, 0, 10000);

        int ChunkSize = Terrain.ChunkSize;

        int posX, posY;

        int chunkTextureOffsetX = ChunkX * ChunkSize;
        int chunkTextureOffsetY = ChunkY * ChunkSize;


        // get a temporary RenderTexture //
        renderTexture = RenderTexture.GetTemporary(ChunkSize, ChunkSize);

        // set the RenderTexture as global target (that means GL too)
        RenderTexture.active = renderTexture;

        // clear GL //
        GL.Clear(false, true, Color.black);

        // render GL immediately to the active render texture //
        material.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, ChunkSize, ChunkSize, 0);


        GL.Begin(GL.TRIANGLES);


        for (int x = dataX; x <= dataX + Terrain.ChunkHexCountX + ChunkX + 1 && x < Terrain.Width; x++)
        {
            for (int y = dataY; y <= dataY + Terrain.ChunkHexCountY + ChunkY + 1 && y < Terrain.Height; y++)
            {
                posX = Mathf.RoundToInt(x * 2 * HexProperties.tileR + (y % 2 == 0 ? 0 : 1) * HexProperties.tileR + HexProperties.tileR) - chunkTextureOffsetX;
                posY = ChunkSize - (Mathf.RoundToInt(y * (HexProperties.tileH + HexProperties.side) + HexProperties.side) - chunkTextureOffsetY);

                //GL.Color(new Color((float)Terrain.Hexes[x, y].Elevation / Terrain.Elevations, (float)Terrain.Hexes[x, y].Elevation / Terrain.Elevations, (float)Terrain.Hexes[x, y].Elevation / Terrain.Elevations));
                //GL.Color(Color.blue);
                //if (TerrainManager.hexGrid[x, y].height == TerrainManager.Altitudes)
                //{
                //    GL.Color(Color.white);
                //    TerrainManager.hexGrid[x, y].terrainType = TerrainType.Arctic;
                //    break;
                //}

                //for (int i = 0; i < biomes.Length; i++)
                //{
                //    if (TerrainManager.hexGrid[x, y].Temperature >= biomes[i].minTemp && TerrainManager.hexGrid[x, y].Humidity >= biomes[i].minHum)
                //    {
                //        TerrainManager.hexGrid[x, y].terrainType = (TerrainType)(i);
                //        GL.Color(new Color(biomes[i].color.r, biomes[i].color.g, biomes[i].color.b)); //  * (1 - 0.3f * (1 - TerrainManager.hexGrid[x, y].height / 6f))
                //        break;
                //    }
                //}

                //Debug.Log(this.Terrain.TerrainTypesList.TerrainTypes[(int)this.Terrain.Hexes[x, y].TerrainType].Color);
                GL.Color(new Color(this.Terrain.TerrainTypesList.TerrainTypes[(int)this.Terrain.Hexes[x, y].TerrainType].Color.r, this.Terrain.TerrainTypesList.TerrainTypes[(int)this.Terrain.Hexes[x, y].TerrainType].Color.g, this.Terrain.TerrainTypesList.TerrainTypes[(int)this.Terrain.Hexes[x, y].TerrainType].Color.b));
                //GL.Color(Color.blue);

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
        //RenderTexture.ReleaseTemporary(renderTexture);

        return newTexture;
    }

    private void UpdateChunkMesh(int ChunkX, int ChunkY, int resIndex, bool lowerTop, bool lowerBottom, bool lowerRight, bool lowerLeft)
    {
        Mesh mesh = Chunks[ChunkX, ChunkY].GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        // Pixels per vetex point
        int res = ChunkLODRes[resIndex];
        int lowerRes = ChunkLODRes[Mathf.Clamp(resIndex + 1, 0, (int)ChunkLODRes.Length)];

        float resStep = (float)Terrain.ChunkSize / (float)res;
        float lowerResStep = (float)Terrain.ChunkSize / (float)lowerRes;

        float lowVertPerHighVert = (float)res / (float)lowerRes; // 2 or 1 


        float heightmapStep = (float)ChunkHeightmaps[ChunkX, ChunkY].width / res;
        float lowerHeightmapStep = (float)ChunkHeightmaps[ChunkX, ChunkY].width / (float)lowerRes;
        float uvStep = 1f / Terrain.ChunkSize;


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
                xPos = Mathf.Clamp(x * resStep / Terrain.PixelsPerUnit, 0, res * resStep / Terrain.PixelsPerUnit);
                zPos = Mathf.Clamp(z * resStep / Terrain.PixelsPerUnit, 0, res * resStep / Terrain.PixelsPerUnit);

                lowX = Mathf.Floor(x / lowVertPerHighVert);
                lowZ = Mathf.Floor(z / lowVertPerHighVert);
                height = ChunkHeightmaps[ChunkX, ChunkY].GetPixel((int)(x * heightmapStep), (int)(z * heightmapStep)).grayscale * (float)Terrain.PixelsToHeight;

                // top seam
                if (z == res && ChunkY + 1 <= ChunkCountY)
                {
                    if (lowerTop == false)
                    {
                        height = ChunkHeightmaps[ChunkX, Mathf.Clamp(ChunkY + 1, 0, ChunkCountY)].GetPixel((int)(x * heightmapStep), 0).grayscale * (float)Terrain.PixelsToHeight; // (height + 
                    }
                    else
                    {
                        leftXHeight = ChunkHeightmaps[ChunkX, ChunkY + 1].GetPixel((int)(lowX * lowerHeightmapStep), 0).grayscale;
                        rightXHeight = ChunkHeightmaps[ChunkX, ChunkY + 1].GetPixel((int)((lowX + 1) * lowerHeightmapStep), 0).grayscale;

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
                if (x == res && ChunkX + 1 <= ChunkCountX)
                {
                    if (lowerRight == false)
                    {
                        height = ChunkHeightmaps[Mathf.Clamp(ChunkX + 1, 0, ChunkCountX), ChunkY].GetPixel(0, (int)(z * heightmapStep)).grayscale * (float)Terrain.PixelsToHeight;
                    }
                    else
                    {
                        topZHeight = ChunkHeightmaps[ChunkX + 1, ChunkY].GetPixel(0, (int)(lowZ * lowerHeightmapStep)).grayscale;
                        botZHeight = ChunkHeightmaps[ChunkX + 1, ChunkY].GetPixel(0, (int)((lowZ + 1) * lowerHeightmapStep)).grayscale;

                        lowZFloat = z / ((float)res / (float)lowerRes);
                        difference = botZHeight - topZHeight;
                        increment = ((lowZFloat - (float)lowZ)) / 1f;

                        height = (topZHeight + (difference * increment)) * (float)Terrain.PixelsToHeight;
                    }
                }

                vertices[v] = new Vector3(xPos,
                                          height,
                                          zPos);

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

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;


        //CalculateNormals(res);



        mesh.normals = normals;
       
        mesh.normals = RecalculateNormals2(); //
        //mesh.RecalculateNormals();
        //NormalSolver.RecalculateNormals(mesh, 20);
        //mesh.RecalculateBounds();

        //TangentSolver.Solve(mesh);


        //mesh.Optimize();


        //CalculateNormals(res);

        //TangentSolver.Solve(mesh);
        //mesh.normals = normals;     
        //
        //mesh.triangles = triangles;
        
        //TangentSolver.Solve(mesh);
        
        //mesh.Optimize();
    }


    private void UpdateChunkCollisionMesh(int ChunkX, int ChunkY)
    {

        //MeshCollider meshCollider = Chunks[ChunkX, ChunkY].GetComponent<MeshCollider>(); //Chunks[ChunkX, ChunkY].GetComponent<MeshCollider>().sharedMesh;
        Mesh meshCollider = new Mesh();

        //Debug.Log(Chunks[ChunkX, ChunkY].GetComponent<MeshCollider>());
        //meshCollider.Clear();

        vertices = new Vector3[(ChunkCollisionResolution + 1) * (ChunkCollisionResolution + 1)];

        // Pixels per vetex point
        //float resStep = ChunkSize / ChunkCollisionResolution;
        float resStep = (float)Terrain.ChunkSize / (float)ChunkCollisionResolution;
        float heightmapStep = (float)ChunkHeightmaps[ChunkX, ChunkY].width / (float)ChunkCollisionResolution;


        for (int v = 0, z = 0; z <= ChunkCollisionResolution; z++)
        {
            for (int x = 0; x <= ChunkCollisionResolution; x++, v++)
            {
                vertices[v] = new Vector3(x * resStep / Terrain.PixelsPerUnit,
                                          ChunkHeightmaps[ChunkX, ChunkY].GetPixel((int)(x * heightmapStep), (int)(z * heightmapStep)).grayscale * Terrain.PixelsToHeight,
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
        Chunks[ChunkX, ChunkY].GetComponent<MeshCollider>().sharedMesh = meshCollider;
    }

    public Vector3[] RecalculateNormals2()
    {
        //List<int> tris = this.data.triangles;
        //Vector3[] normals = new Vector3[this.data.vertices.Count];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = vertices[triangles[i]] - vertices[triangles[i + 1]];
            Vector3 b = vertices[triangles[i]] - vertices[triangles[i + 2]];

            Vector3 normal = Vector3.Cross(a, b);
            normals[triangles[i]] += normal;
            normals[triangles[i + 1]] += normal;
            normals[triangles[i + 2]] += normal;
        }

        List<Vector3> nList = new List<Vector3>();
        for (int i = 0; i < normals.Length; i++)
        {
            nList.Add(normals[i].normalized);
        }
        return nList.ToArray();
    }

    private void CalculateNormals (int resolution) {
		for (int v = 0, z = 0; z <= resolution; z++) {
			for (int x = 0; x <= resolution; x++, v++) {
                normals[v] = new Vector3(-GetXDerivative(x, z, resolution), 1f, -GetZDerivative(x, z, resolution)).normalized;
			}
		}
	}

    private float GetXDerivative(int x, int z, int resolution)
    {
        int rowOffset = z * (resolution + 1);
        float left, right, scale;
        if (x > 0)
        {
            left = vertices[rowOffset + x - 1].y;
            if (x < resolution)
            {
                right = vertices[rowOffset + x + 1].y;
                scale = 0.5f * resolution;
            }
            else
            {
                right = vertices[rowOffset + x].y;
                scale = resolution;
            }
        }
        else
        {
            left = vertices[rowOffset + x].y;
            right = vertices[rowOffset + x + 1].y;
            scale = resolution;
        }
        return (right - left) * scale;
    }

    private float GetZDerivative(int x, int z, int resolution)
    {
        int rowLength = resolution + 1;
        float back, forward, scale;
        if (z > 0)
        {
            back = vertices[(z - 1) * rowLength + x].y;
            if (z < resolution)
            {
                forward = vertices[(z + 1) * rowLength + x].y;
                scale = 0.5f * resolution;
            }
            else
            {
                forward = vertices[z * rowLength + x].y;
                scale = resolution;
            }
        }
        else
        {
            back = vertices[z * rowLength + x].y;
            forward = vertices[(z + 1) * rowLength + x].y;
            scale = resolution;
        }
        return (forward - back) * scale;
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
        float chunkSize = Terrain.ChunkSize / Terrain.PixelsPerUnit;
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


                    int chunkX = Mathf.Clamp(Mathf.FloorToInt(pos.x / chunkSize), 0, ChunkCountX - 1);
                    int chunkY = Mathf.Clamp(Mathf.FloorToInt(pos.z / chunkSize), 0, ChunkCountY - 1);

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
