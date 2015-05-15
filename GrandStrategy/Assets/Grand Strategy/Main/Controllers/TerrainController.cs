using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class TerrainController : TerrainControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeTerrain(TerrainViewModel viewModel) {
        base.InitializeTerrain(viewModel);
        // This is called when a TerrainViewModel is created
    }

    public override void GenerateTerrain(TerrainViewModel terrain)
    {
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
        
        
        Timer.Print();
        this.Publish(new GenerateChunksCommand() { Sender = Terrain });
    }

    public override void GenerateChunks(TerrainViewModel terrain)
    {
        terrain.SetupTerrainChunks();
    }

    public override void Erosion(TerrainViewModel terrain)
    {
        
    }

}
