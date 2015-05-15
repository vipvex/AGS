using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainManager2 : MonoBehaviour 
{


    public float[,] terrainData;
    public float[,] hexTerrainData;


    public int terrainSeed = 0;
    public bool randomizeSeed;


    [UnityEngine.Space(10)]


    public int terrainWidth  = 1024;
    public int terrainHeight = 1024;


    public float pixelsPerUnit = 5;
    public float resolutionHeight = 3;


    [UnityEngine.Space(10)]
    
    [Range(0f, 1f)]
    public float altitudes = 5;
    [Range(0f, 1f)]
    public float altitudeVariation = 0.4f;
    [Range(0f, 1f)]
    public float grain = 0.5f;


    [UnityEngine.Space(10)]





    public int chunkSize = 512;
    public int chunkResolution = 64;
    public int chunkCollisionResolution = 16;

    public GameObject chunkPrefab;
    
    public Hexagon[,] hexGrid;
    public HexChunk[,] hexChunks;

    private Transform chunkContainer;


    [UnityEngine.Space(10)]


    public bool hexagonolize = true;
    public bool showPathfindingNodes = false;
    public bool showCurrentPathNodes = false;
    public bool generatePathObjects = false;

    public AnimationCurve hexagonSmoothness;
    public HexProperties hexProperties;
    [System.Serializable]
    public struct HexProperties
    {
        public float side;
        public float height;
        public float width;

        public float tileR;
        public float tileH;
    }


    [UnityEngine.Space(10)]

    public Gradient terrainAltitudeColors;


    public Texture2D heightMap;
    public Texture2D tempetureMap;
    public Texture2D moistureMap;
    public Texture2D biomMap;
    public Texture2D terrainTexture;

    [UnityEngine.Space(10)]

    private Hexagon hoverHex;
    public Vector2 hoverHexAraray;
    public Vector3 hoverHexCube;


    [UnityEngine.Space(10)]
    public GameObject node;

    public GameObject heightLightHex;


    List<Hexagon> path;

	void Awake () 
    {
        SetHexProperties();
        GenerateMap();
	}

    private void SetHexProperties()
    {
        hexProperties.tileH = Mathf.Sin((30f * Mathf.PI) / 180f) * hexProperties.side;
        hexProperties.tileR = Mathf.Cos((30f * Mathf.PI) / 180f) * hexProperties.side;

        hexProperties.width = Mathf.RoundToInt(2f * hexProperties.tileR);
        hexProperties.height = Mathf.RoundToInt(hexProperties.side + 2f * hexProperties.tileH);
    }


    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateMap();
        }
    }

    void FixedUpdate()
    {
        HexSelect();
    }
    
    void HexSelect ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200))
        {
            Debug.DrawLine(ray.origin, hit.point);

            float stepSize = 1f / chunkSize;
            float resScale = chunkSize / chunkResolution;


            float pointX = hit.point.x;
            float pointZ = hit.point.z;
            pointX = pointX * pixelsPerUnit;
            pointZ = pointZ * pixelsPerUnit;
            pointZ += hexProperties.tileH;
            pointX -= hexProperties.width / 2;
            pointZ -= (hexProperties.height - hexProperties.tileH);


            float q = (1f / 3f * Mathf.Sqrt(3f) * pointX - 1f / 3f * pointZ) / hexProperties.side;
            float r = 2f / 3f * pointZ / hexProperties.side;


            Vector3 cube = new Vector3();
            cube.x = q;
            cube.z = r;
            cube.y = -cube.x - cube.z;

            cube = Hexagon.RoundCubeCoord(cube);


            hoverHexCube = cube;
            hoverHexAraray = Hexagon.CubeToOffsetOddQ(cube);


            if (hoverHexAraray.x >= 0 && hoverHexAraray.y >= 0 && hoverHexAraray.x < hexGrid.GetLength(0) && hoverHexAraray.y < hexGrid.GetLength(1))
                hoverHex = hexGrid[(int)hoverHexAraray.x, (int)hoverHexAraray.y];
            else
                hoverHex = null;

            if (Input.GetButton("Fire1"))
            {
                int pathCost = 0;
                //path = Pathfinding.GetPath(hexGrid[2, 2], hoverHex, pathCost);
                Debug.Log(pathCost);
            }

        }

        if (hoverHex != null)
        {
            heightLightHex.transform.position = hoverHex.worldPos;
        }
        else
        {
            heightLightHex.transform.position = Vector3.up * -5;
        }


    }

    void OnGUI()
    {
        GUI.Box(new Rect(Input.mousePosition.x + 5, Screen.height - Input.mousePosition.y + 5, 150, 50), "Array: " + hoverHexAraray + "\n\n" + "Cube: " + hoverHexCube);
    }

    public void GenerateMap ()
    {

        int timeStart = System.Environment.TickCount;

        if (chunkContainer)
        {
            Destroy(chunkContainer.gameObject);
        }

        if (randomizeSeed)
        {
            terrainSeed = (int)(System.Environment.TickCount + System.DateTime.Now.Ticks);
        }
        Random.seed = terrainSeed;



        terrainData = new float[terrainWidth + 1, terrainHeight + 1];
        hexTerrainData = new float[terrainWidth + 1, terrainHeight + 1];


        terrainData[0, 0] = Random.Range(0.1995f, 0.8005f);
        terrainData[terrainWidth, 0] = Random.Range(0.2995f, 0.9005f);
        terrainData[0, terrainHeight] = Random.Range(0.2995f, 1.005f);
        terrainData[terrainWidth, terrainHeight] = Random.Range(0.1995f, 0.6005f);

        DiamondSquare(0, 0, terrainWidth, terrainHeight, altitudeVariation, grain);

        if (hexagonolize)
        {
            Hexagonize();
        }

        Debug.Log("Terrain size: " + terrainData.GetLength(0) + ", " + terrainData.GetLength(1));
        Debug.Log("Hexagon size: " + (hexProperties.width / (pixelsPerUnit)) + ", " + (hexProperties.height / (pixelsPerUnit)));


        heightMap = DataToHeightMap(terrainData);
        //tempetureMap = DataToTemperatureMap(hexTerrainData);
        //moistureMap = DataToMoistureMap(hexTerrainData);
        //biomMap = DataToBiomeMap(hexTerrainData);
        //terrainTexture = GenerateTexture();


        GenerateChunks();


        Debug.Log("Terrain generated: " + (System.Environment.TickCount - timeStart) + "ms");

    }
    
    public void GenerateChunkMeshes ()
    {
        //for (int x=0; x<he)
    }

    private void DiamondSquare (int xbegin, int ybegin, int xend, int yend, float randomRange, float randomDiminish)
    {

        float sum, randomNow = randomRange;
        int squareSize = xend - xbegin; // Length of x
        int x0, y0, x1, y1;

        int diamondTimer = System.Environment.TickCount;

        while (squareSize > 1)
        {
            // diamond step
            for (x0 = xbegin; x0 < xend; x0 += squareSize)
            {
                x1 = x0 + squareSize; // right

                for (y0 = ybegin; y0 < yend; y0 += squareSize)
                {
                    y1 = y0 + squareSize; // right

                    
                    int midx = x0 + (x1 - x0) / 2; // Middle of the points
                    int midy = y0 + (y1 - y0) / 2;

                    sum = (terrainData[x0, y0] + terrainData[x0, y1] + terrainData[x1, y0] + terrainData[x1, y1]) / 4.0f; // Get avarage of the 4 points
                    terrainData[midx, midy] = sum * (1 + Random.Range(-randomNow, randomNow)); // middle * (0.6 - 1.4) - essentially adds a bit of vaRIATION TO 
                }
            }

            // square step
            for (x0 = xbegin; x0 < xend; x0 += squareSize)
            {
                x1 = x0 + squareSize; // right

                for (y0 = ybegin; y0 < yend; y0 += squareSize)
                {
                    y1 = y0 + squareSize; // right

                    int midx = x0 + (x1 - x0) / 2; // Middle of the points
                    int midy = y0 + (y1 - y0) / 2;

                    
                    if (y0 == ybegin) // top
                    {
                        sum = (terrainData[x0, y0] + terrainData[x1, y0] + terrainData[midx, midy]) / 3.0f; // Avarage middle top edge
                        terrainData[midx, y0] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }
                    else
                    {
                        sum = (terrainData[x0, y0] + terrainData[x1, y0] + terrainData[midx, midy] + terrainData[midx, midy - squareSize]) / 4.0f; // Bottom
                        terrainData[midx, y0] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }

                    if (y1 == yend)
                    {
                        sum = (terrainData[x0, y1] + terrainData[x1, y1] + terrainData[midx, midy]) / 3.0f;
                        terrainData[midx, y1] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }
                    else
                    {
                        sum = (terrainData[x0, y1] + terrainData[x1, y1] + terrainData[midx, midy] + terrainData[midx, midy + squareSize]) / 4.0f;
                        terrainData[midx, y1] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }

                    if (x0 == xbegin)
                    {
                        sum = (terrainData[x0, y0] + terrainData[x0, y1] + terrainData[midx, midy]) / 3.0f;
                        terrainData[x0, midy] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }
                    else
                    {
                        sum = (terrainData[x0, y0] + terrainData[x0, y1] + terrainData[midx, midy] + terrainData[midx - squareSize, midy]) / 4.0f;
                        terrainData[x0, midy] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }

                    if (x1 == xend)
                    {
                        sum = (terrainData[x1, y0] + terrainData[x1, y1] + terrainData[midx, midy]) / 3.0f;
                        terrainData[x1, midy] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }
                    else
                    {
                        sum = (terrainData[x1, y0] + terrainData[x1, y1] + terrainData[midx, midy] + terrainData[midx + squareSize, midy]) / 4.0f;
                        terrainData[x1, midy] = sum * ((1.0f + Random.Range(-randomNow, randomNow)));
                    }
                }
            }

            squareSize /= 2; // Divide the map
            randomNow *= randomDiminish; // Decrease the randomization
        }

        Debug.Log("Diamond Square: " + (System.Environment.TickCount - diamondTimer) + "ms");
    }

    public void GenerateRivers ()
    {
        for (int x = 0; x < hexGrid.GetLength(0); x++)
        {
            for (int y = 0; y < hexGrid.GetLength(1); y++)
            {
                
            }
        }
    }

    public void CalculateMoisture ()
    {

    }

    public void CalculateTempetures()
    {

    }

    public void CalculateBiomes()
    {

    }

    public void GenerateFlora ()
    {

    }

    public void GenerateRecources ()
    { 
    
    }


    private void Hexagonize() {		
		int hexagonizeStart = System.Environment.TickCount;

        int dataX = 0;
        int dataY = 0;
        float avarageHeight = 0;

        int hexCountX = Mathf.FloorToInt(terrainWidth / hexProperties.width);
        int hexCountY = Mathf.FloorToInt(terrainHeight / (hexProperties.height - hexProperties.tileH));

        Debug.Log("X count " + hexCountX + " Y count " + hexCountY);
        Debug.Log("Size: " + terrainData.GetLength(0));

        hexGrid = new Hexagon[hexCountX, hexCountY];

        Vector3 arrayPos;

        for (int y = 0; y < hexGrid.GetLength(1); y++)
        {
            for (int x = 0; x < hexGrid.GetLength(0); x++)
            {

                dataX = Mathf.RoundToInt(x * 2 * hexProperties.tileR + (y % 2 == 0 ? 0 : 1) * hexProperties.tileR + hexProperties.tileR);
                dataY = Mathf.RoundToInt((y) * (hexProperties.tileH + hexProperties.side));


                avarageHeight = Mathf.Round(terrainData[dataX, dataY] / altitudes) * altitudes;


                Vector3 worldPos = new Vector3();
                worldPos.x = dataX / pixelsPerUnit;
                worldPos.y = avarageHeight * resolutionHeight;
                worldPos.z = (dataY + hexProperties.side) / pixelsPerUnit;

                //avarageHeight = (terrainData[dataX, dataY] + terrainData[dataX + 1, dataY] + terrainData[dataX + 1, dataY + 1] + terrainData[dataX - 1, dataY] + terrainData[dataX - 1, dataY  - 1]) / 5;

                ConvertToHex(dataX, dataY, avarageHeight);
                hexGrid[x, y] = new Hexagon(new Vector2(x, y), new Vector2(dataX, dataY), avarageHeight, worldPos);

                if (generatePathObjects)
                {

                    GameObject a = Instantiate(node, hexGrid[x, y].worldPos, Quaternion.identity) as GameObject;
                    a.name = "" + hexGrid[x, y].arrayCoord.x + ", " + hexGrid[x, y].arrayCoord.y;
                    //a.name = "" + hexGrid[x, y].cubeCoord.x + ", " + hexGrid[x, y].cubeCoord.y + ", " + hexGrid[x, y].cubeCoord.z;

                    a.transform.parent = chunkContainer;
                }

            }
        }

        for (int y = 0; y < hexGrid.GetLength(1); y++)
        {
            for (int x = 0; x < hexGrid.GetLength(0); x++)
            {
                // Setup hex neighbors !!! DOEN"T WORK PROERLY!
                for (int n = 0; n < 6; n++)
                {
                    arrayPos = Hexagon.CubeToOffsetOddQ(hexGrid[x, y].cubeCoord + Hexagon.neighborDirs[n]);

                    if (arrayPos.x >= 0 && arrayPos.x < hexGrid.GetLength(0) && arrayPos.y >= 0 && arrayPos.y < hexGrid.GetLength(1))
                    {
                        hexGrid[x, y].neighbors[n] = hexGrid[(int)arrayPos.x, (int)arrayPos.y];
                    }
                }
            }
        }

		Debug.Log ("Hexagonized: " + (System.Environment.TickCount - hexagonizeStart) + "ms");
	}




    void ConvertToHex(int centerX, int centerY, float height)
    {
        int startX = 0, endX = 0;


        for (int y = (int)hexProperties.height; y > 0; y--)
        {

            if (y < hexProperties.tileH) // TOP
            {
                startX = Mathf.FloorToInt(-(y / hexProperties.tileH * hexProperties.tileR));
                endX = Mathf.CeilToInt(y / hexProperties.tileH * hexProperties.tileR);
            }
            else if (y >= hexProperties.tileH && y <= hexProperties.side + hexProperties.tileH) // MIDDLE
            {
                startX = Mathf.FloorToInt(-hexProperties.tileR);
                endX = Mathf.CeilToInt(hexProperties.tileR);
            }
            else // BOTTOM
            {
                startX = Mathf.FloorToInt(-((hexProperties.height - y) / hexProperties.tileH * hexProperties.tileR));
                endX = Mathf.CeilToInt(((hexProperties.height - y) / hexProperties.tileH * hexProperties.tileR));
            }

            for (int x = startX; x < endX; x++)
            {
                if (x + centerX > 0 && x + centerX < terrainWidth - 1 && y + centerY > 0 && y + centerY < terrainHeight - 1)
                {
                    if (height <= altitudes)
                        hexTerrainData[x + centerX, y + centerY] = altitudes;          
                    else
                        hexTerrainData[x + centerX, y + centerY] = height + ((height - terrainData[x + centerX, y + centerY]) * -hexagonSmoothness.Evaluate((Vector2.Distance(new Vector2(hexProperties.width / 2, hexProperties.height / 2), new Vector2(hexProperties.tileR + x, y)) / hexProperties.tileR)));
                }
            }
        }
    }

    Texture2D DrawHex(Texture2D texture, int centerX, int centerY, Color color)
    {
        int startX = 0, endX = 0;


        for (int y = (int)hexProperties.height; y > 0; y--)
        {

            if (y < hexProperties.tileH) // TOP
            {
                startX = Mathf.FloorToInt(-(y / hexProperties.tileH * hexProperties.tileR));
                endX = Mathf.CeilToInt(y / hexProperties.tileH * hexProperties.tileR);
            }
            else if (y >= hexProperties.tileH && y <= hexProperties.side + hexProperties.tileH) // MIDDLE
            {
                startX = Mathf.FloorToInt(-hexProperties.tileR);
                endX = Mathf.CeilToInt(hexProperties.tileR);
            }
            else // BOTTOM
            {
                startX = Mathf.FloorToInt(-((hexProperties.height - y) / hexProperties.tileH * hexProperties.tileR));
                endX = Mathf.CeilToInt(((hexProperties.height - y) / hexProperties.tileH * hexProperties.tileR));
            }

            for (int x = startX; x < endX; x++)
            {
                if (x + centerX > 0 && x + centerX < terrainWidth - 1 && y + centerY > 0 && y + centerY < terrainHeight - 1)
                {
                    texture.SetPixel(x + centerX, y + centerY, color);
                }
            }
        }
        return texture;
    }

    /// <summary>
    /// Generates the chunks for the terrain
    /// </summary>
    void GenerateChunks()
    {
        int chunkXCount = Mathf.CeilToInt(terrainWidth / chunkSize);
        int chunkYCount = Mathf.CeilToInt(terrainHeight / chunkSize);

        chunkContainer = new GameObject("_ChunkContainer").GetComponent<Transform>();
        hexChunks = new HexChunk[chunkXCount, chunkYCount];

        for (int x=0; x<chunkXCount; x++)
        {
            for (int y=0; y<chunkYCount; y++)
            {
                CreateChunk(x, y);
            }
        }

    }

    /// <summary>
    /// Creates a chunk
    /// </summary>
    void CreateChunk(int x, int y)
    {
        GameObject chunk = (GameObject)Instantiate(chunkPrefab, new Vector3(chunkSize / pixelsPerUnit * x, 0, chunkSize / pixelsPerUnit * y), Quaternion.identity);
        
        chunk.name = "Chunk " + "[" + x + "," + y + "]";

        chunk.GetComponent<Transform>().parent = chunkContainer;

        
        HexChunk hexChunk = chunk.GetComponent<HexChunk>();
        hexChunk.Initialize(x * chunkSize, y * chunkSize, chunkSize, chunkResolution, chunkCollisionResolution, this);
        hexChunks[x, y] = hexChunk;
    }




    
    /// <summary>
    /// Converts the terrain data to a heightmap
    /// </summary>
    private Texture2D DataToHeightMap (float[,] data)
    {
        int timer = System.Environment.TickCount;

        Texture2D texture = new Texture2D(data.GetLength(0), data.GetLength(1));
        for (int x = 0; x < data.GetLength(0); x++)
            for (int y = 0; y < data.GetLength(1); y++)
                texture.SetPixel(x, y, new Color(data[x, y], data[x, y], data[x, y]));

        texture.Apply();

        Debug.Log("Data to heightmap: " + (System.Environment.TickCount - timer) + "ms");
        return texture;
    }

    /// <summary>
    /// Converts the terrain data to a heightmap
    /// </summary>
    private Texture2D DataToTemperatureMap(float[,] data)
    {
        int timer = System.Environment.TickCount;

        Texture2D texture = new Texture2D(data.GetLength(0), data.GetLength(1));
        for (int x = 0; x < data.GetLength(0); x++)
            for (int y = 0; y < data.GetLength(1); y++)
                texture.SetPixel(x, y, new Color(data[x, y], data[x, y], data[x, y]));

        texture.Apply();

        Debug.Log("Data to temperature: " + (System.Environment.TickCount - timer) + "ms");
        return texture;
    }

    /// <summary>
    /// Converts the terrain data to a heightmap
    /// </summary>
    private Texture2D DataToMoistureMap(float[,] data)
    {
        int timer = System.Environment.TickCount;

        Texture2D texture = new Texture2D(data.GetLength(0), data.GetLength(1));
        for (int x = 0; x < data.GetLength(0); x++)
            for (int y = 0; y < data.GetLength(1); y++)
                texture.SetPixel(x, y, new Color(data[x, y], data[x, y], data[x, y]));

        texture.Apply();

        Debug.Log("Data to moisture: " + (System.Environment.TickCount - timer) + "ms");
        return texture;
    }

    /// <summary>
    /// Converts the terrain data to a heightmap
    /// </summary>
    private Texture2D DataToBiomeMap(float[,] data)
    {
        int timer = System.Environment.TickCount;

        Texture2D texture = new Texture2D(data.GetLength(0), data.GetLength(1));
        for (int x = 0; x < data.GetLength(0); x++)
            for (int y = 0; y < data.GetLength(1); y++)
                texture.SetPixel(x, y, new Color(data[x, y], data[x, y], data[x, y]));

        texture.Apply();

        Debug.Log("Data to biome: " + (System.Environment.TickCount - timer) + "ms");
        return texture;
    }

    /// <summary>
    /// Converts the terrain data to a texture for the chunk
    /// </summary>
    private Texture2D GenerateTexture()
    {
        int timer = System.Environment.TickCount;

        Texture2D texture = new Texture2D(hexTerrainData.GetLength(0), hexTerrainData.GetLength(1));


        for (int x = 0; x < hexGrid.GetLength(0); x++)
        {
            for (int y = 0; y < hexGrid.GetLength(1); y++)
            {
                texture = DrawHex(texture, (int)hexGrid[x, y].dataPos.x, (int)hexGrid[x, y].dataPos.y, new Color(hexGrid[x, y].height, hexGrid[x, y].height, hexGrid[x, y].height));
            }
        }

        texture.Apply();

        Debug.Log("Terrain texture generation: " + (System.Environment.TickCount - timer) + "ms");
        return texture;
    }


    void OnDrawGizmos()
    {
        if (showPathfindingNodes)
        {
            Gizmos.color = Color.white;
            Vector2 arrayPos = Vector2.zero;
            for (int x = 0; x < hexGrid.GetLength(0); x++)
            {
                for (int y = 0; y < hexGrid.GetLength(1); y++)
                {
                    Gizmos.DrawWireSphere(hexGrid[x, y].worldPos, 0.35f);
                    if (hoverHex == hexGrid[x, y])
                        Gizmos.DrawWireSphere(hexGrid[x, y].worldPos, 0.5f);

                    for (int n = 0; n < 6; n++)
                    {
                        arrayPos = Hexagon.CubeToOffsetOddQ(hexGrid[x, y].cubeCoord + Hexagon.neighborDirs[n]);

                        if (arrayPos.x >= 0 && arrayPos.x < hexGrid.GetLength(0) && arrayPos.y >= 0 && arrayPos.y < hexGrid.GetLength(1))
                            Gizmos.DrawLine(hexGrid[x, y].worldPos, hexGrid[(int)arrayPos.x, (int)arrayPos.y].worldPos);
                    }

                }
            }
        }

        if (showCurrentPathNodes)
        {
            Gizmos.color = Color.blue;

            for (int a = 0; a < path.Count; a++)
            {
                Gizmos.DrawWireSphere(path[a].worldPos, .75f);

                if (a + 1 < path.Count)
                    Gizmos.DrawLine(path[a].worldPos, path[a + 1].worldPos);

            }
        }
        
    }

}
