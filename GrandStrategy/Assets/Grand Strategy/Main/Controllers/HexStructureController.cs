using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class HexStructureController : HexStructureControllerBase {
    
    public override void InitializeHexStructure(HexStructureViewModel viewModel) {
        base.InitializeHexStructure(viewModel);
        // This is called when a HexStructureViewModel is created
    }
}
