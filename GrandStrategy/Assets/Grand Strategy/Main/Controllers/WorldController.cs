using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class WorldController : WorldControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeWorld(WorldViewModel viewModel) {
        base.InitializeWorld(viewModel);
        // This is called when a WorldViewModel is created
    }
    
    public override void GenerateWorld(WorldViewModel viewModel) {
        base.GenerateWorld(viewModel);
    }
    
    public override void GenerateWorldHandler(GenerateWorldCommand command) {
        base.GenerateWorldHandler(command);
    }
}
