using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class GameLogicController : GameLogicControllerBase {
    
    public override void InitializeGameLogic(GameLogicViewModel viewModel) {
        base.InitializeGameLogic(viewModel);
        // This is called when a GameLogicViewModel is created
    }
    
    public override void StartGame(GameLogicViewModel viewModel) {
        base.StartGame(viewModel);

        Terrain.GenerateTerrain.OnNext(new GenerateTerrainCommand());   
    }
}
