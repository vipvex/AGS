using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class ResourceController : ResourceControllerBase {
    
    public override void InitializeResource(ResourceViewModel viewModel) {
        base.InitializeResource(viewModel);
        // This is called when a ResourceViewModel is created
    }
}
