using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class StructureController : StructureControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeStructure(StructureViewModel viewModel) {
        base.InitializeStructure(viewModel);
        // This is called when a StructureViewModel is created
    }
}
