using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class TerrainController : TerrainControllerBase {
    
    public override void InitializeTerrain(TerrainViewModel viewModel) {
        base.InitializeTerrain(viewModel);
        // This is called when a TerrainViewModel is created
    }

    public override void GenerateTerrain(TerrainViewModel terrain)
    {
        Debug.Log("Generating terrain");

        if (terrain.RandomizedSeed)
            terrain.Seed = UnityEngine.Random.Range(0, Int16.MaxValue);

        UnityEngine.Random.seed = terrain.Seed;


        // calculate hexagon demensions
        HexProperties.SetProperties(terrain.HexSideLength, terrain.PixelsPerUnit);

        terrain.Rainfall = new float[terrain.Width + 1, terrain.Height + 1];
        terrain.GenerateTerrainHeights();
        terrain.TerrainHeightsToHexGrid();
        terrain.SetupHexesNeighbors();
        terrain.AddWaterPools();
        terrain.GenerateRivers();
        terrain.CalculateHumidity();
        terrain.CalculateTemperature();
        terrain.CalculateBiomes();
        terrain.SetupTerrainChunks();

        Timer.Print();

        terrain.GenerateChunks.OnNext(new GenerateChunksCommand() { Sender = Terrain });
    }

    public override void GenerateTerrainHandler(GenerateTerrainCommand command)
    {
        base.GenerateTerrainHandler(command);
    
    }

    public override void Erosion(TerrainViewModel terrain)
    {

    }
}
