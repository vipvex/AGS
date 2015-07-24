using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class FogOfWarController : FogOfWarControllerBase {
    
    public override void InitializeFogOfWar(FogOfWarViewModel viewModel) {
        base.InitializeFogOfWar(viewModel);
        // This is called when a FogOfWarViewModel is created
    }
}
