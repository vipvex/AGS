using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class FactionController : FactionControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeFaction(FactionViewModel viewModel) {
        base.InitializeFaction(viewModel);
        // This is called when a FactionViewModel is created
    }
}
