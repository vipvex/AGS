using UnityEngine;
using System.Collections;

public class DiamondSquare 
{

    public static void Generate(float[,] terrainData, int xbegin, int ybegin, int xend, int yend, float randomRange, float randomDiminish)
    {
        
        float sum, randomNow = randomRange;
        int squareSize = xend - xbegin; // Length of x
        int x0, y0, x1, y1; 

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

                    if (terrainData[midx, midy] != 0)
                        continue;

                    sum = (terrainData[x0, y0] + terrainData[x0, y1] + terrainData[x1, y0] + terrainData[x1, y1]) / 4.0f; // Get avarage of the 4 points
                    terrainData[midx, midy] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1); // middle * (0.6 - 1.4) - essentially adds a bit of vaRIATION TO 
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
                        terrainData[midx, y0] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }
                    else
                    {
                        sum = (terrainData[x0, y0] + terrainData[x1, y0] + terrainData[midx, midy] + terrainData[midx, midy - squareSize]) / 4.0f; // Bottom
                        terrainData[midx, y0] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }

                    if (y1 == yend)
                    {
                        sum = (terrainData[x0, y1] + terrainData[x1, y1] + terrainData[midx, midy]) / 3.0f;
                        terrainData[midx, y1] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }
                    else
                    {
                        sum = (terrainData[x0, y1] + terrainData[x1, y1] + terrainData[midx, midy] + terrainData[midx, midy + squareSize]) / 4.0f;
                        terrainData[midx, y1] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }

                    if (x0 == xbegin)
                    {
                        sum = (terrainData[x0, y0] + terrainData[x0, y1] + terrainData[midx, midy]) / 3.0f;
                        terrainData[x0, midy] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }
                    else
                    {
                        sum = (terrainData[x0, y0] + terrainData[x0, y1] + terrainData[midx, midy] + terrainData[midx - squareSize, midy]) / 4.0f;
                        terrainData[x0, midy] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }

                    if (x1 == xend)
                    {
                        sum = (terrainData[x1, y0] + terrainData[x1, y1] + terrainData[midx, midy]) / 3.0f;
                        terrainData[x1, midy] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }
                    else
                    {
                        sum = (terrainData[x1, y0] + terrainData[x1, y1] + terrainData[midx, midy] + terrainData[midx + squareSize, midy]) / 4.0f;
                        terrainData[x1, midy] = Mathf.Clamp(sum * ((1.0f + UnityEngine.Random.Range(-randomNow, randomNow))), 0, 1);
                    }
                }
            }

            squareSize /= 2; // Divide the map
            randomNow *= randomDiminish; // Decrease the randomization
        }
    }

    public static Texture2D ToTexture2D(float[,] terrainData)
    {
        float height;
        Texture2D texture = new Texture2D(terrainData.GetLength(0), terrainData.GetLength(1));

        for (int x = 0; x < terrainData.GetLength(0); x++)
        {
            for (int y = 0; y < terrainData.GetLength(1); y++)
            {
                height = terrainData[x, y];
                texture.SetPixel(x, y, new Color(height, height, height));
                if (terrainData[x, y] < 0.5f)
                    texture.SetPixel(x, y, Color.blue);
                
            }
        }

        texture.Apply();

        return texture;
    }

    public static Texture2D ToTexture2D(float[,] terrainData, Gradient gradient)
    {
        float height;
        Texture2D texture = new Texture2D(terrainData.GetLength(0), terrainData.GetLength(1));
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < terrainData.GetLength(0); x++)
        {
            for (int y = 0; y < terrainData.GetLength(1); y++)
            {
                height = terrainData[x, y];
                texture.SetPixel(x, y, gradient.Evaluate(height));

            }
        }

        texture.Apply();

        return texture;
    }


    public static Texture2D ToTexture2D(float[,] terrainData, float seaLevel)
    {
        float height;
        Texture2D texture = new Texture2D(terrainData.GetLength(0), terrainData.GetLength(1));
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < terrainData.GetLength(0); x++)
        {
            for (int y = 0; y < terrainData.GetLength(1); y++)
            {
                height = terrainData[x, y];
                texture.SetPixel(x, y, new Color(height, height, height));

                if (seaLevel >= height)
                {
                    texture.SetPixel(x, y, Color.blue);
                }
            }
        }

        texture.Apply();

        return texture;
    }

}
