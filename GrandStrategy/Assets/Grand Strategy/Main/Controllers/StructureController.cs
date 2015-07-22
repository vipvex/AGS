using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class StructureController : StructureControllerBase {
    
    public override void InitializeStructure(StructureViewModel viewModel) {
        base.InitializeStructure(viewModel);
        // This is called when a StructureViewModel is created
    }
}
