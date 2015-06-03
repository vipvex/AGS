using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class PlayerController : PlayerControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializePlayer(PlayerViewModel viewModel) {
        base.InitializePlayer(viewModel);
        // This is called when a PlayerViewModel is created
    }
}
