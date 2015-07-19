using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;


public partial class TerrainViewModel : TerrainViewModelBase {

    public override void Bind() {
        base.Bind();
    }

    public void GenerateTerrainHeights()
    {
        //Timer.Start("Terrain heighmap generation");

        TerrainHeights = new float[Width + 1, Height + 1];

        //Pathfinding.gridSize = TerrainWidth * TerrainWidth + 2;

        // Set corner heights
        TerrainHeights[0, 0]          = 0.5f;
        TerrainHeights[Width, 0]      = 0.5f;
        TerrainHeights[0, Height]     = 0.5f;
        TerrainHeights[Width, Height] = 0.5f;
        

        //TerrainHeights[(int)(Width * 0.75f), (int)(Height * 0.25f)] = 1;
        //TerrainHeights[(int)(Width * 0.75f), (int)(Height * 0.75f)] = 1;
        //TerrainHeights[(int)(Width * 0.25f), (int)(Height * 0.25f)] = 1;
        //TerrainHeights[(int)(Width * 0.25f), (int)(Height * 0.75f)] = 1;
        //TerrainHeights[(int)(Width * 0.5f), (int)(Height * 0.5f)] = 0;



        int mountainRangeX = 0;
        int mountainRangeY = 0;

        // Place mountain ranges
        for (int i = 0; i < MountainRangeFrequency; i++)
        {
            mountainRangeX = UnityEngine.Random.Range(0, Width);
            mountainRangeY = UnityEngine.Random.Range(0, Height);
            for (int a=0; a < MountainRangeScale; a++)
            {
                TerrainHeights[mountainRangeX, mountainRangeY] = 1;
                mountainRangeX = Mathf.Clamp(mountainRangeX + UnityEngine.Random.Range(-MountainSpacing, MountainSpacing), 0, Width);
                mountainRangeY = Mathf.Clamp(mountainRangeY + UnityEngine.Random.Range(-MountainSpacing, MountainSpacing), 0, Height);
            }
        }

        DiamondSquare.Generate(TerrainHeights, 0, 0, Width, Height, DiamonDetail, DiamondVariation);
        CalculateTerrainErosion();
        Timer.Print();
    }

    public void CalculateTerrainErosion()
    {
        float RainAmount = 0.1f; // how much rain is deposited on each cell each iteration
        float Solubility = 0.02f; // how much soil is eroded into sediment each iteration and how much is added with evaporation
        float Evaporation = 0.01f; // what percentage of water evaporates each iteration


        float lowestHeight = 0.0f;
        int LowestPointIndex = 0;
        float difference;
        Vector2Int[] neighbors = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };


        //Vector2Int[] viableRiverLocations = ViableRiverLocations();

        for (int e = 0; e < ErrosionPasses; e++)
        {
            Rainfall = new float[Width + 1, Height + 1];

            // Simulate water movement
            for (int a = 0; a < RainfallMovementSteps; a++)
			{

                // Rainfall
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {

                        // if this is the first iteration of the water movement add some water
                        if (a == 0)
                        {
                            //WaterTiles = new WaterTiles
                            Rainfall[x, y] += RainAmount;
                        }

                        // Subtract height before moving
                        if (Rainfall[x, y] <= 0)
                            continue;                  

                        if (a != 0)
                        {
                            // Have water remove soil height
                            TerrainHeights[x, y] -= Solubility;
                        }

                        // Movement
                        LowestPointIndex = -1;
                        lowestHeight = TerrainHeights[x, y];
                        for (int i = 0; i < RainfallNeighbors; i++)
                        {
                            // if outside of array range
                            if (x + neighbors[i].x > Width || x + neighbors[i].x < 0 || y + neighbors[i].y > Height || y + neighbors[i].y < 0)
                                continue;

                            // Get the lowest point
                            // otherheight < height
                            if (TerrainHeights[x + neighbors[i].x, y + neighbors[i].y] <= lowestHeight)
                            {
                                lowestHeight = TerrainHeights[x + neighbors[i].x, y + neighbors[i].y];
                                LowestPointIndex = i;
                            }
                        }

                        // Move water to lowest tile                  
                        if (LowestPointIndex > -1)
                        {
                            // Remove water
                            // if lowestheight + water < height - move all the water to that tile
                            if (lowestHeight + Rainfall[x + neighbors[LowestPointIndex].x, y + neighbors[LowestPointIndex].y] < TerrainHeights[x, y])
                            {
                                Rainfall[x + neighbors[LowestPointIndex].x, y + neighbors[LowestPointIndex].y] += Rainfall[x, y];
                                Rainfall[x, y] = 0;
                            }
                            else
                            {
                                difference = (Rainfall[x, y] - Rainfall[x + neighbors[LowestPointIndex].x, y + neighbors[LowestPointIndex].y]) / 2;

                                Rainfall[x, y] -= difference;
                                Rainfall[x + neighbors[LowestPointIndex].x, y + neighbors[LowestPointIndex].y] += difference;
                            }
                        }

                        // Add height
                        if (Rainfall[x, y] > 0 && a != 0)
                        {
                            TerrainHeights[x, y] += Solubility;
                            Rainfall[x, y] -= Evaporation;
                        }
                    }
                }
            }
        }
    }
    

    public void AddWaterPools()
    {
        Timer.Start("Adding water pool");

        WaterHexes = new ModelCollection<Hex>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (Hexes[x, y].Elevation < SeaLevel)
                {
                    WaterHexes.Add(Hexes[x, y]);
                }
            }   
        }
        GenerateOcean(WaterHexes.ToList());

        // Go through all the water tiles in the map
        //List<Hex> scannedHexes = new List<Hex>();
        //while (seaLevelHexes.Count > 0)
        //{
        //    scannedHexes.Clear();
        //    scannedHexes.AddRange(seaLevelHexes[0].SurroundingWaterHexes(SeaLevel));
        //    //scannedHexes.RemoveAll(item => item == null);
        //    Debug.Log(scannedHexes.Count);
        //
        //    // Search outwards from the water tile to find ALL other water tiles around it
        //    //Hex.SearchNeighbors(scannedHexes[0], p_hex => p_hex.Elevation <= SeaLevel, scannedHexes);
        //
        //
        //    // water pools
        //    if (scannedHexes.Count < LakeMinSize)
        //    {
        //        //FlattenArea(scannedHexes);
        //    }
        //    else if (scannedHexes.Count < SeaMinSize)  // make lake
        //    {
        //        GenerateLake(scannedHexes);
        //    }
        //    else                                      // make ocean
        //    {
        //        GenerateOcean(scannedHexes);
        //    }
        //
        //    for (int i = 0; i < seaLevelHexes.Count; i++)
        //    {
        //        seaLevelHexes.Remove(seaLevelHexes[i]);                
        //    }
        //}

        Timer.End();
    }

    public Hex GetRandomOceanTile()
    {
        return WaterHexes[UnityEngine.Random.Range(0, WaterHexes.Count)];
    }

    public Hex GetRandomRiverTile()
    {
        return RiverHexes[UnityEngine.Random.Range(0, RiverHexes.Count)];
    }

    public void FlattenHexes(List<Hex> hexes)
    {
        //for (int i = 0; i < hexes.Count; i++)
        //{
        //    hexes[i].height = 2;
        //    hexes[i].heightmapHeight = hexes[i].height / terrainManager.Altitudes;
        //    terrainManager.waterTiles.Remove(hexes[i]);
        //}
    }

    public void GenerateLake(List<Hex> hexes)
    {
        for (int i = 0; i < hexes.Count; i++)
        {
            hexes[i].TerrainType = TerrainType.Lake;
        }
    }

    public void GenerateSea(List<Hex> hexes)
    {
        for (int i = 0; i < hexes.Count; i++)
        {
            hexes[i].TerrainType = TerrainType.Sea;
        }
    }

    public void GenerateOcean(List<Hex> hexes)
    {
        for (int i = 0; i < hexes.Count; i++)
        {
            hexes[i].TerrainType = TerrainType.Ocean;
        }
    }

    public void GenerateRivers()
    {
        int randX;
        int randY;
        int riverCount = 0;

        
        int mainRivers = 3;
        int smallerRivers = 5;
        RiverHexes = new ModelCollection<Hex>();

        while(riverCount < mainRivers)
        {
            randX = UnityEngine.Random.Range(0, Width);
            randY = UnityEngine.Random.Range(0, Height);

            if (Hexes[randX, randY].Elevation >= RiverMinHeight)
            {
                GenerateRiver(Hexes[randX, randY], GetRandomOceanTile());
                riverCount++;
            }
        }

        riverCount = 0;

        while (riverCount < smallerRivers)
        {
            randX = UnityEngine.Random.Range(0, Width);
            randY = UnityEngine.Random.Range(0, Height);

            if (Hexes[randX, randY].Elevation >= RiverMinHeight + 4)
            {
                GenerateRiver(Hexes[randX, randY], GetRandomRiverTile());
                riverCount++;
            }
        }


        Debug.Log("Revers" + riverCount);
    }

    public void GenerateRiver(Hex fromHex, Hex toHex)
    {

        List<Hex> river = new List<Hex>();

        river = Pathfinding.FindPath(fromHex, toHex);

        for (int i = 0; i < river.Count; i++)
        {
            river[i].TerrainType = TerrainType.River;
        }

        RiverHexes.AddRange(river);
    }

    public void AddRivers()
    {

    }

    //public void CalculateHumidity()
    //{
    //    Timer.Start("Calculating humidity");
    //
    //    for (int i = 0; i < terrainManager.waterTiles.Count; i++)
    //    {
    //        Hex.HumiditySpread(terrainManager.waterTiles[i], 3, 50, terrainManager.HumidySpreadDecrease, terrainManager.waterTiles);
    //    }
    //    // loop through rivers
    //    for (int i = 0; i < terrainManager.riverTiles.Count; i++)
    //    {
    //        Hex.HumiditySpread(terrainManager.riverTiles[i], 15, 30, 3, terrainManager.riverTiles);
    //    }
    //
    //    int randomHumiditySpread = 10;
    //    int humMin = 5;
    //    int humMax = 30;
    //    int rangeMin = 4;
    //    int rangeMax = 30;
    //
    //
    //
    //    for (int i = 0; i < randomHumiditySpread; i++)
    //    {
    //        Hex.HumiditySpread(terrainManager.GetRandomLandTile(), UnityEngine.Random.Range(rangeMin, rangeMax), UnityEngine.Random.Range(humMin, humMax), 3, terrainManager.riverTiles);
    //    }
    //
    //
    //    Timer.End();
    //}
    //
    //public void CalculateTemperature(TerrainManagerViewModel terrainManager)
    //{
    //    for (int x = 0; x < terrainManager.TerrainWidth; x++)
    //    {
    //        for (int y = 0; y < terrainManager.TerrainHeight; y++)
    //        {
    //            terrainManager.hexGrid[x, y].Temperature -= (int)terrainManager.HumidityTemperature.Evaluate(terrainManager.hexGrid[x, y].Humidity);
    //            terrainManager.hexGrid[x, y].Temperature -= (int)terrainManager.HeightTemperature.Evaluate(terrainManager.hexGrid[x, y].height);
    //        }
    //    }
    //}
    //
    //public void CalculateBiomes(TerrainManagerViewModel terrainManager)
    //{
    //    Timer.Start("Calculating biomeList");
    //
    //    for (int x = 0; x < terrainManager.TerrainWidth; x++)
    //    {
    //        for (int y = 0; y < terrainManager.TerrainHeight; y++)
    //        {
    //            if (terrainManager.hexGrid[x, y].terrainType == TerrainType.None)
    //                terrainManager.hexGrid[x, y].terrainType = TerrainType.Grassland;
    //        }
    //    }
    //
    //    Timer.End();
    //}

    public void CalculateHumidity()
    {

    }

    public void CalculateTemperature()
    {
        int temperature = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                temperature = (int)LatitudeTempCurve.Evaluate((float)y / (float)Height); // - (int)AltitudeTempCurve.Evaluate(Hexes[x, y].Elevation / Elevations); Mathf.Abs((Height / 2f) - y) / 

                Hexes[x, y].Temperature = temperature;
                Hexes[x, y].Humidity = 15;
            }
        }
    }

    public void CalculateBiomes()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // Water hexes are already defind
                if (Hexes[x, y].WaterHex())
                    continue;

                Hexes[x, y].TerrainType = TerrainTypesList.CalculateType(Hexes[x, y].Temperature, Hexes[x, y].Humidity);
            }
        }
    }

    public void CalculateVegetation()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {

            }
        }
    }


    public Vector2Int[] ViableRiverLocations()
    {
        int count = 0;
        Vector2Int[] rivers = new Vector2Int[RiverFrequency];

        int randX = 0;
        int randY = 0;

        while(count < RiverFrequency)
        {
            randX = UnityEngine.Random.Range(0, Width);
            randY = UnityEngine.Random.Range(0, Height);

            if (TerrainHeights[randX, randY] >= RiverMinHeight)
            {
                rivers[count] = new Vector2Int(randX, randY);
                count++;
            }
        }

        return rivers;
    }

    public void TerrainHeightsToHexGrid()
    {
        Timer.Start("Terrain heights to hexes");

        int elevation;
        Vector3 worldPos = new Vector3();

        Hexes = new Hex[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                elevation = Mathf.RoundToInt((float)Elevations * TerrainHeights[x, y]);

                worldPos.x = Mathf.RoundToInt(x * 2 * HexProperties.tileR + (y % 2 == 0 ? 0 : 1) * HexProperties.tileR + HexProperties.tileR) / (float)PixelsPerUnit;
                worldPos.y = (elevation / Elevations) * PixelsToHeight;
                worldPos.z = Mathf.RoundToInt(y * (HexProperties.tileH + HexProperties.side) + HexProperties.side) / (float)PixelsPerUnit;

                Hexes[x, y] = new Hex(x, y, elevation, worldPos);
            }
        }

        Timer.End();
    }

    public void SetupHexesNeighbors()
    {
        Timer.Start("Setup hexes neighbors");

        int XIndex = 0;
        int YIndex = 0;
        Vector3 arrayIndex;


        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (Hexes[x, y] == null) continue;

                    arrayIndex = Hex.CubeToOffsetOddQ(Hexes[x, y].CubeIndex + Hex.neighborDirs[i]);
                    XIndex = (int)arrayIndex.x;
                    YIndex = (int)arrayIndex.y;

                    if (XIndex > -1 && XIndex < Width && YIndex > -1 && YIndex < Height)
                    {
                        Hexes[x, y].neighbors.Add(Hexes[XIndex, YIndex]);
                    }
                }
            }
        }

        Timer.End();
    }

    public void SetupTerrainChunks()
    {
        Timer.Start("Generating chunks");

        ChunkHexCountX = (int)(ChunkSize / HexProperties.width);
        ChunkHexCountY = (int)(ChunkSize / (HexProperties.tileH + HexProperties.side));

        int chunkCountX = Mathf.CeilToInt(Width * HexProperties.width / (float)ChunkSize);
        int chunkCountY = Mathf.CeilToInt(Height * (HexProperties.tileH + HexProperties.side) / (float)ChunkSize);

        Chunks = new ChunkViewModel[chunkCountX, chunkCountY];

        for (int x = 0;  x < chunkCountX; x++)
        {
            for (int y = 0; y < chunkCountY; y++)
            {
                Chunks[x, y] = new ChunkViewModel(this.Aggregator)
                {
                    XIndex = x,
                    YIndex = y
                };
            }
        }

        Timer.End();
    }

    public Vector3 ChunkWorldPos(int XIndex, int YIndex)
    {
        return( new Vector3(XIndex * (ChunkSize / PixelsPerUnit),
                            0,
                            YIndex * (ChunkSize / PixelsPerUnit)));
    }

    public Vector3 ChunkCenterWorldPos(int XIndex, int YIndex)
    {
        return( new Vector3(XIndex * (ChunkSize / PixelsPerUnit) + (ChunkSize / PixelsPerUnit) / 2, 
                            0,
                            YIndex * (ChunkSize / PixelsPerUnit) + (ChunkSize / PixelsPerUnit) / 2));
    }

    public Vector3 ChunkWorldSize()
    {
        return new Vector3(ChunkSize / PixelsPerUnit,
                           PixelsToHeight,
                           ChunkSize / PixelsPerUnit);
    }

    public Hex GetHexAtPos(Vector3 pos)
    {
        float pointX = pos.x;
        float pointZ = pos.z;
        pointX = pointX * PixelsPerUnit;
        pointZ = pointZ * PixelsPerUnit;
        pointZ += HexProperties.tileH;
        pointX -= HexProperties.width / 2;
        pointZ -= (HexProperties.height - HexProperties.tileH);


        float q = (1f / 3f * Mathf.Sqrt(3f) * pointX - 1f / 3f * pointZ) / HexProperties.side;
        float r = 2f / 3f * pointZ / HexProperties.side;


        Vector3 cube = new Vector3();
        cube.x = q;
        cube.z = r;
        cube.y = -cube.x - cube.z;

        cube = Hexagon.RoundCubeCoord(cube);

        Vector2 hoverHexAraray = Hexagon.CubeToOffsetOddQ(cube);
        Hex hex = null;

        if (hoverHexAraray.x >= 0 && hoverHexAraray.y >= 0 && hoverHexAraray.x < Hexes.GetLength(0) && hoverHexAraray.y < Hexes.GetLength(1))
            hex = Hexes[(int)hoverHexAraray.x, (int)hoverHexAraray.y];

        return hex;
    }


    #region Helper Methods

    public int ChunkCountX() { return Chunks.GetLength(0); }

    public int ChunkCountY() { return Chunks.GetLength(1); }

    #endregion

}
