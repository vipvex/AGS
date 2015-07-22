using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class FactionController : FactionControllerBase {
    
    public override void InitializeFaction(FactionViewModel viewModel) {
        base.InitializeFaction(viewModel);
        // This is called when a FactionViewModel is created
    }
}
