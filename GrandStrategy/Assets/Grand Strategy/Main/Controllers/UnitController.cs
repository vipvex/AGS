using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class UnitController : UnitControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeUnit(UnitViewModel viewModel) {
        base.InitializeUnit(viewModel);
        // This is called when a UnitViewModel is created
    }
}
