using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class SettlmentController : SettlmentControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeSettlment(SettlmentViewModel viewModel) {
        base.InitializeSettlment(viewModel);
        // This is called when a SettlmentViewModel is created
    }
}
