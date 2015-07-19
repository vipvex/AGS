using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class HexStructureController : HexStructureControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeHexStructure(HexStructureViewModel viewModel) {
        base.InitializeHexStructure(viewModel);
        // This is called when a HexStructureViewModel is created
    }
}
