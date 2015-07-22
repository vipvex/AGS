using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class WorldController : WorldControllerBase {
    
    public override void InitializeWorld(WorldViewModel viewModel) {
        base.InitializeWorld(viewModel);
        // This is called when a WorldViewModel is created
    }
    
    public override void GenerateWorld(WorldViewModel viewModel) {
        base.GenerateWorld(viewModel);
    }
}
