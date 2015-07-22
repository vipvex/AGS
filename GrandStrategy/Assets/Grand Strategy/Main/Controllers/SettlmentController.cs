using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class SettlmentController : SettlmentControllerBase {
    
    public override void InitializeSettlment(SettlmentViewModel viewModel) {
        base.InitializeSettlment(viewModel);
        // This is called when a SettlmentViewModel is created
    }
}
