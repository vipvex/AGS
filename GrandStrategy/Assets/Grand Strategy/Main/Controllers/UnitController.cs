using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class UnitController : UnitControllerBase {
    
    public override void InitializeUnit(UnitViewModel viewModel) {
        base.InitializeUnit(viewModel);
        // This is called when a UnitViewModel is created
    }
}
