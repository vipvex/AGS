using UnityEngine;
using System.Collections;

public class HexagonPathfinding : MonoBehaviour 
{

    public int terrainWidth;
    public int terrainHeight;

    public int chunkSize = 512;
    private int chunkHexCountX;
    private int chunkHexCountY;


    Hexagon[,] hexagons;
    public float[,] terrainData;

    public GameObject hPrefab;

    public Texture2D chunkTexture;


    public ProceduralMaterial material;


    public HexProperties hexProperties;
    [System.Serializable]
    public struct HexProperties
    {
        public int side;
        public int height;
        public int width; 

        public float tileR;
        public float tileH;
    }


        [UnityEngine.Space(10)]
    
    [Range(0f, 1f)]
    public float altitudes = 5;
    [Range(0f, 1f)]
    public float altitudeVariation = 0.4f;
    [Range(0f, 1f)]
    public float grain = 0.5f;

    public GameObject chunkPrefab;
    public Texture2D[,] chunks;


	void GenerateMap () 
    {
        int timeStart = System.Environment.TickCount;

        material = GetComponent<Renderer>().material as ProceduralMaterial;

        SetHexProperties();

        terrainData = new float[terrainWidth + 1, terrainWidth + 1];


        terrainData[0, 0] = Random.Range(0.1995f, 0.8005f);
        terrainData[terrainWidth, 0] = Random.Range(0.2995f, 0.9005f);
        terrainData[0, terrainWidth] = Random.Range(0.2995f, 1.005f);
        terrainData[terrainWidth, terrainWidth] = Random.Range(0.1995f, 0.6005f);

        DiamondSquare(0, 0, terrainWidth, terrainWidth, altitudeVariation, grain);


        Debug.Log("World size is: " + terrainWidth + "by" + terrainWidth);
        Debug.Log("Chunks: " + Mathf.Ceil(terrainWidth * hexProperties.width / (float)chunkSize) + "by" + Mathf.Ceil(terrainHeight * hexProperties.height / (float)chunkSize));


        chunkHexCountX = chunkSize / hexProperties.width;
        chunkHexCountY = (int)(chunkSize / (hexProperties.tileH + hexProperties.side));

        int chunkCountX = Mathf.CeilToInt(terrainWidth * hexProperties.width / (float)chunkSize);
        int chunkCountY = Mathf.CeilToInt(terrainHeight * hexProperties.height / (float)chunkSize);

        chunks = new Texture2D[chunkCountX, chunkCountY];

        for (int x = 0; x < chunkCountX; x++)
        {
            for (int y = 0; y < chunkCountY; y++)
            {
                GameObject chunk = Instantiate(chunkPrefab, new Vector3(x * 10, 0, y * 10), Quaternion.identity) as GameObject;
                chunk.name = "Chunk [" + x + "," + y + "]";
                chunks[x, y] = new Texture2D(chunkSize, chunkSize);
                chunks[x, y].wrapMode = TextureWrapMode.Clamp;
                GenerateChunkHexes(chunks[x, y], x * chunkHexCountX, y * chunkHexCountY);
                chunk.GetComponent<Renderer>().material.mainTexture = chunks[x, y];
            }
        }

        chunkTexture = DataToBiomeMap(terrainData);
        //renderer.material.mainTexture = chunkTexture;
        //SetSubstanceMaterial();
        
        Debug.Log("Terrain generated: " + (System.Environment.TickCount - timeStart) + "ms");

	}

    public void GenerateChunkHexes(Texture2D texture, int dataX, int dataY)
    {

        float terrainVal = 0;
        int posX;
        int posY;

        for (int x = 0; x < chunkHexCountX + 1 && x + dataX < terrainWidth; x++)
        {
            for (int y = 0; y < chunkHexCountY + 1 && y + dataY < terrainHeight; y++)
            {
                posX = Mathf.RoundToInt((x) * 2 * hexProperties.tileR + (y % 2 == 0 ? 0 : 1) * hexProperties.tileR + hexProperties.tileR);
                posY = Mathf.RoundToInt((y) * (hexProperties.tileH + hexProperties.side));


                terrainVal = Mathf.Round(terrainData[x + dataX, y + dataY] / altitudes) * altitudes;
                DrawHex(texture, posX, posY, new Color(terrainVal, terrainVal, terrainVal));
            }
        }
        texture.Apply();
    }


    void DrawHex(Texture2D texture, int centerX, int centerY, Color color)
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
                  texture.SetPixel(x + centerX, y + centerY, color);
            }
        }
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
            {
                float height = Mathf.Round(data[x, y] / 0.16f) * 0.16f;
                texture.SetPixel(x, y, new Color(height, height, height));
                //if (height <= 0.16f)
                    //texture.SetPixel(x, y, Color.blue);

                //if (height >= 1-0.16f)
                  //  texture.SetPixel(x, y, Color.yellow);
                /*
                if (height <= 0.16f)
                    texture.SetPixel(x, y, Color.blue);

                if (height == 0.16f*2)
                    texture.SetPixel(x, y, Color.yellow);

                if (height == 0.16f * 3)
                    texture.SetPixel(x, y, Color.green);

                if (height == 0.16f * 4)
                    texture.SetPixel(x, y, Color.gray);

                if (height == 0.16f * 5)
                    texture.SetPixel(x, y, new Color(87/256, 1/256, 1/256));*/

            }
        texture.Apply();

        Debug.Log("Data to biome: " + ((System.Environment.TickCount - timer) / 100f) + "ms");
        return texture;
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateMap();
        }
    }

    private void DiamondSquare(int xbegin, int ybegin, int xend, int yend, float randomRange, float randomDiminish)
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

        Debug.Log("Diamond Square: " + ((System.Environment.TickCount - diamondTimer) / 100f) + "ms");
    }


    private void SetHexProperties()
    {
        hexProperties.tileH = Mathf.Sin((30 * Mathf.PI) / 180) * hexProperties.side;
        hexProperties.tileR = Mathf.Cos((30 * Mathf.PI) / 180) * hexProperties.side;

        hexProperties.width = Mathf.RoundToInt(2 * hexProperties.tileR);
        hexProperties.height = Mathf.RoundToInt(hexProperties.side + 2 * hexProperties.tileH);
    }

    private void GenerateGrid ()
    {

        int pixelX = 0;
        int pixelY = 0;

        int hexCountX = Mathf.FloorToInt(terrainWidth / hexProperties.width);
        int hexCountY = Mathf.FloorToInt(terrainWidth / (hexProperties.height - hexProperties.tileH));

        Debug.Log("X count " + hexCountX + " Y count " + hexCountY);

        hexagons = new Hexagon[hexCountX, hexCountY];


        for (int x = 0; x < hexagons.GetLength(0); x++)
        {
            for (int y = 0; y < hexagons.GetLength(1); y++)
            {

                pixelX = Mathf.RoundToInt(x * 2 * hexProperties.tileR + (y % 2 == 0 ? 0 : 1) * hexProperties.tileR + hexProperties.tileR);
                pixelY = Mathf.RoundToInt(y * (hexProperties.tileH + hexProperties.side));

                ConvertToHex(pixelX, pixelY);

            }
        }
    }


    void ConvertToHex (int centerX, int centerY)
    {
        int startX = 0, endX = 0;


        float r = Random.Range(0, 1f);
        r = Mathf.Round(r / 0.16f) * 0.16f;
        Color newCol = new Color(r, r, r);  

        for (int y = hexProperties.height; y > 0; y--)
        {

            if (y < hexProperties.tileH) // TOP
            {
                startX = (int)-(y / hexProperties.tileH * hexProperties.tileR);
                endX = (int)(y / hexProperties.tileH * hexProperties.tileR);
            }
            else if (y >= hexProperties.tileH && y <= hexProperties.side + hexProperties.tileH) // MIDDLE
            {
                startX = (int)-hexProperties.tileR;
                endX = (int)hexProperties.tileR;
            }
            else // BOTTOM
            {
                startX = (int)-((hexProperties.height - y) / hexProperties.tileH * hexProperties.tileR);
                endX = (int)((hexProperties.height - y) / hexProperties.tileH * hexProperties.tileR);
            }

            for (int x = startX-1; x < endX+1; x++)
            {
                chunkTexture.SetPixel((int)centerX + x, (int)centerY + y, newCol );
            }
        }
    }


    public void SetSubstanceMaterial ()
    {
        int timeStart = System.Environment.TickCount;

        
        material.SetProceduralTexture("Hexagon_Heights", chunkTexture);

        material.RebuildTextures();
        
        
        Debug.Log("Rebulidng textures took: " + (System.Environment.TickCount - timeStart) + "ms");
    }


}
