using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


public class ChunkManagerView : ChunkManagerViewBase 
{
   

    public GameObject ChunkPrefab;
    public float ChunkMeshResolution;

    private GameObject[,] Chunks;
    private Texture2D[,] ChunkHeightmaps;
    private ChunkLOD[,] ChunkLODs;


    [Space(10)]
    public int            ChunkViewRange = 8;
    public AnimationCurve ChunkLODCurve;
    public int[]          ChunkLODRes;
    public Color32[]      ChunkLODColors;


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


    public override void Start()
    {
        base.Start();
        SetupRenderingSettings();
        SetupChunkSettings();

        Debug.Log(Mathf.CeilToInt(64f / 32f));
        Debug.Log(Mathf.CeilToInt(64f / 64f));
        Debug.Log(8f % 1f);
        Debug.Log(8f % 1.5f);
    }

    private void SetupChunkSettings()
    {
        ChunkLODs = new ChunkLOD[ChunkViewRange * 2, ChunkViewRange * 2];
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



    public class ChunkLOD
    {
        public int resolution;
        public Vector2Int index;
        public bool needsToUpdate;
        public bool visible;
    }

    // Run a loop through all the chunks to set their mesh resolutions
    private void UpdateChunkResolutions()
    {
        // Get the index of the chunk that the camera is hovering over
        CameraPos =        Camera.main.transform.position;
        CameraChunkIndex = new Vector2Int((int)((int)(CameraPos.x / ChunkSize) * ChunkSize / ChunkSize), 
                                          (int)((int)(CameraPos.z / ChunkSize) * ChunkSize / ChunkSize));

        //List<Vector2Int> visibleChunks = new List<Vector2Int>();
        //int[,] visibleChunkResolutions = new int[ChunkViewRange * ChunkViewRange, ChunkViewRange * ChunkViewRange];

        //visibleChunks.Add(cameraChunkIndex);
        /*        int newRes = 0;
        for (int x = 0; x < ChunkLODs.GetLength(0); x++)
        {
            for (int y = 0; y < ChunkLODs.GetLength(1); y++)
            {
                if (x + CameraChunkIndex.x > ChunkLODs.GetLength(0) - 1 || y + CameraChunkIndex.y > ChunkLODs.GetLength(1) - 1 || || x + CameraChunkIndex.x < 0)
*/


        // Loop through the surrounding visible chunks
        //int maxX = Mathf.Clamp(CameraChunkIndex.x + ChunkViewRange, 0, Terrain.Chunks.GetLength(0) - 1);
        //int maxY = Mathf.Clamp(CameraChunkIndex.y + ChunkViewRange, 0, Terrain.Chunks.GetLength(1) - 1);
        int newRes = 0;
        //for (int x = Mathf.Clamp(CameraChunkIndex.x - ChunkViewRange, 0, Terrain.Chunks.GetLength(0) - 1); x < maxX; x++)
        //{
        //    for (int y = Mathf.Clamp(CameraChunkIndex.y - ChunkViewRange, 0, Terrain.Chunks.GetLength(1) - 1); y < maxY; y++)
        //    {
                //visibleChunks.Add(new Vector2Int(x, y));
                //visibleChunkResolutions[x, y] = (int)LevelOfDetailDistance.Evaluate(Vector3.Distance(Terrain.ChunkCenterWorldPos(x, y), cameraPos));

        int ChunkIndexX = 0, ChunkIndexY = 0;

        for (int x = -ChunkViewRange; x < ChunkViewRange; x++)
        {
            for (int y = -ChunkViewRange; y < ChunkViewRange; y++)
            {
                // if the chunk view is out of the bounds of the world
                if (x + CameraChunkIndex.x > Chunks.GetLength(0) - 1 || y + CameraChunkIndex.y > Chunks.GetLength(1) - 1 || x + CameraChunkIndex.x < 0 || y + CameraChunkIndex.y < 0)
                {
                    ChunkLODs[x + ChunkViewRange, y + ChunkViewRange].visible = false;
                    continue;
                }

                ChunkIndexX = x + CameraChunkIndex.x;
                ChunkIndexY = y + CameraChunkIndex.y;

                newRes = (int)ChunkLODCurve.Evaluate(Vector3.Distance(Terrain.ChunkCenterWorldPos(ChunkIndexX, ChunkIndexY), CameraPos));
                if (ChunkLODs[x + ChunkViewRange, y + ChunkViewRange].resolution != newRes)
                {
                    ChunkLODs[x + ChunkViewRange, y + ChunkViewRange].index = new Vector2Int(ChunkIndexX, ChunkIndexY);
                    ChunkLODs[x + ChunkViewRange, y + ChunkViewRange].resolution = newRes;
                    ChunkLODs[x + ChunkViewRange, y + ChunkViewRange].needsToUpdate = true;
                    ChunkLODs[x + ChunkViewRange, y + ChunkViewRange].visible = true;

                    //Debug.Log("Chunk X: " + (ChunkIndexX) + "Y: " + (ChunkIndexY));

                    // Make sure to update the surrounding chunks to fix the seams
                    // Top
                    if (y + ChunkViewRange + 1 < Chunks.GetLength(1))
                    {
                        //Debug.Log("Updating neighbor X: " + (ChunkIndexX) + "Y: " + (ChunkIndexY + 1));
                        ChunkLODs[x + ChunkViewRange, y + ChunkViewRange + 1].needsToUpdate = true;
                    }

                    // Bottom
                    if (y + ChunkViewRange - 1 >= 0)
                    {
                        //Debug.Log("Updating neighbor X: " + (ChunkIndexX) + "Y: " + (ChunkIndexY - 1));
                        ChunkLODs[x + ChunkViewRange, y + ChunkViewRange - 1].needsToUpdate = true;
                    }

                    // Right
                    if (x + ChunkViewRange + 1 < Chunks.GetLength(0))
                    {
                        //Debug.Log("Updating neighbor X: " + (ChunkIndexX + 1) + "Y: " + (ChunkIndexY));
                        ChunkLODs[x + ChunkViewRange + 1, y + ChunkViewRange].needsToUpdate = true;
                    }

                    // Left
                    if (x + ChunkViewRange - 1 >= 0)
                    {
                        //Debug.Log("Updating neighbor X: " + (ChunkIndexX - 1) + "Y: " + (ChunkIndexY));
                        ChunkLODs[x + ChunkViewRange - 1, y + ChunkViewRange].needsToUpdate = true;
                    }
                }
            }
        }

        // Update all the chunks that need updating
        bool lowerResTop = false, lowerResRight = false;
        for (int x = 0; x < ChunkLODs.GetLength(0); x++)
        {
            for (int y = 0; y < ChunkLODs.GetLength(1); y++)
            {
                if (ChunkLODs[x, y].needsToUpdate && ChunkLODs[x, y].visible)
                {
                    // Check if the top chunk has a lower resolution
                    if (y + 1 <= ChunkLODs.GetLength(1))
                        lowerResTop = ChunkLODs[x, y].resolution < ChunkLODs[x, y + 1].resolution;

                    // Check if the right chunk has a lower resolution
                    if (x + 1 <= ChunkLODs.GetLength(0))
                        lowerResRight = ChunkLODs[x, y].resolution < ChunkLODs[x + 1, y].resolution;

                    //Debug.Log(ChunkLODs[x, y].index.x + " " + ChunkLODs[x, y].index.y);
                    UpdateChunkMesh(ChunkLODs[x, y].index.x, ChunkLODs[x, y].index.y, ChunkLODs[x, y].resolution, lowerResTop, false, lowerResRight, false);
                    
                    ChunkLODs[x, y].needsToUpdate = false;
                }
                
                // do some hiding logic here
                //if(ChunkLODs[x, y].visible == false)
                //{
                //}
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

        float ChunkSize = Terrain.ChunkSize / Terrain.PixelsPerUnit;

        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 cameraChunkPos = new Vector3((int)(cameraPos.x / ChunkSize) * ChunkSize, 0, (int)(cameraPos.z / ChunkSize) * ChunkSize);
        Vector2Int cameraIndex = new Vector2Int((int)(cameraChunkPos.x / ChunkSize), (int)(cameraChunkPos.z / ChunkSize));

        List<Vector2Int> visibleChunks = new List<Vector2Int>();
        visibleChunks.Add(cameraIndex);
        for (int x = -ChunkViewRange; x < ChunkViewRange + 1; x++)
        {
            for (int y = -ChunkViewRange; y < ChunkViewRange + 1; y++)
            {        
                visibleChunks.Add(new Vector2Int(cameraIndex.x + x, cameraIndex.y + y));
            }
        }


        Gizmos.color = Color.blue;
        for (int x = 0; x < Terrain.Chunks.GetLength(0); x++)
        {
            for (int y = 0; y < Terrain.Chunks.GetLength(1); y++)
            {
                if (visibleChunks.Contains(new Vector2Int(x, y)))
                {
                    Gizmos.color = (Color)ChunkLODColors[(int)ChunkLODCurve.Evaluate(Vector3.Distance(Terrain.ChunkCenterWorldPos(x, y), cameraPos))];
                    
                }else{
                    Gizmos.color = Color.blue;
                }
                
                Gizmos.DrawWireCube(Terrain.ChunkCenterWorldPos(x, y) + ((Vector3.up * Terrain.PixelsToHeight) / 2), Terrain.ChunkWorldSize() - Vector3.one * 0.05f);
            }
        }
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

                yield return null;
            }

        }

        GeneratedTerrain = true;
        ChunkCountX = Terrain.Chunks.GetLength(0) - 1;
        ChunkCountY = Terrain.Chunks.GetLength(1) - 1;

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
        ProceduralTexture substanceTexture = substance.GetGeneratedTexture("Terrain_heightmap");

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

    private void UpdateChunkMesh(int ChunkX, int ChunkY, int resIndex, bool lowerTop, bool lowerBottom, bool lowerRight, bool lowerLeft)
    {
        Mesh mesh = Chunks[ChunkX, ChunkY].GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        // Pixels per vetex point
        int res = ChunkLODRes[resIndex];
        int lowerRes = ChunkLODRes[Mathf.Clamp(resIndex + 1, 0, (int)ChunkLODRes.Length)];

        float resStep = (float)Terrain.ChunkSize / (float)res;
        float lowerResStep = (float)Terrain.ChunkSize / (float)lowerRes;

        // How many high resolution vertecies are in between 2 lower ones
        // Low:      |--------|
        // Height:   |--|--|--| lowVertPerHeighVert would be 4
        // 48 / 24 = 2 
        // 32 / 24 = 1.33
        float lowVertPerHighVert = (float)res / (float)lowerRes; // 2 or 1    // M;  // 


        float heightmapStep = (float)ChunkHeightmaps[ChunkX, ChunkY].width / res;
        float lowerHeightmapStep = (float)ChunkHeightmaps[ChunkX, ChunkY].width / (float)lowerRes;
        float uvStep = 1f / Terrain.ChunkSize;


        Vector3[] vertices = new Vector3[(res + 1) * (res + 1)];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

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
                if (z == res && ChunkY <= ChunkCountY)
                {
                    if (lowerTop == false)
                    {
                        height = ChunkHeightmaps[ChunkX, Mathf.Clamp(ChunkY + 1, 0, Terrain.Chunks.GetLength(1) - 1)].GetPixel((int)(x * heightmapStep), 0).grayscale * (float)Terrain.PixelsToHeight; // (height + 
                    }
                    else
                    {
                        leftXHeight = ChunkHeightmaps[ChunkX, ChunkY + 1].GetPixel((int)(lowX * lowerHeightmapStep), 0).grayscale;
                        rightXHeight = ChunkHeightmaps[ChunkX, ChunkY + 1].GetPixel((int)((lowX + 1) * lowerHeightmapStep), 0).grayscale;

                        lowXFloat = x / ((float)res / (float)lowerRes);
                        difference = rightXHeight - leftXHeight;
                        increment = ((lowXFloat - (float)lowX)) / 1f;

                        height = (leftXHeight + (difference * increment)) * (float)Terrain.PixelsToHeight;
                                                
                        //Debug.Log("X float" + lowXFloat);
                        //Debug.Log("Difference " + difference);
                        //Debug.Log("Increment " + increment);
                    }
                }

                // right seam
                if (x == res)
                {
                    if (lowerRight == false)
                    {
                        height = ChunkHeightmaps[Mathf.Clamp(ChunkX + 1, 0, Terrain.Chunks.GetLength(0) - 1), ChunkY].GetPixel(0, (int)(z * heightmapStep)).grayscale * (float)Terrain.PixelsToHeight;
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

                normals[v] = Vector3.up;
                uv[v] = new Vector2(x * resStep * uvStep, z * resStep * uvStep);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;

        int[] triangles = new int[res * res * 6];
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
        mesh.triangles = triangles;

        //CalculateNormals(res);
        TangentSolver.Solve(mesh);
        mesh.Optimize();
    }


    //Debug.Log(leftOver);
    //Debug.Log(((float)leftOver / (float)lowerVertsToHighSteps));
    //float bonusUVStep = uvStep * ((float)leftOver / (float)lowerVertsToHighSteps);
    ////Debug.Log(bonusUVStep);
    ////Debug.Log(uvStep);
    //
    //vertices[v] = new Vector3(Mathf.Clamp(newX * lowerResStep / Terrain.PixelsPerUnit, 0, res * resStep / Terrain.PixelsPerUnit),
    //                          ChunkHeightmaps[ChunkX, Mathf.Clamp(ChunkY + 1, 0, Terrain.Chunks.GetLength(1) - 1)].GetPixel((int)(newX * lowerHeightmapStep), 0).grayscale * (float)Terrain.PixelsToHeight,
    //                          Mathf.Clamp(z * resStep / Terrain.PixelsPerUnit, 0, res * resStep / Terrain.PixelsPerUnit)
    //                          ); 
    //
    //normals[v] = Vector3.up;
    //Debug.Log(new Vector2(x * resStep * uvStep + bonusUVStep, z * resStep * uvStep));
    //uv[v] = new Vector2(x * resStep * uvStep + bonusUVStep, z * resStep * uvStep);
    //
    //Debug.Log(uv[v]);

}
