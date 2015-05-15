using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class ResourceController : ResourceControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeResource(ResourceViewModel viewModel) {
        base.InitializeResource(viewModel);
        // This is called when a ResourceViewModel is created
    }
}
