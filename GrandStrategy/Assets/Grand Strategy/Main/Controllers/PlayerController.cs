using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class PlayerController : PlayerControllerBase {
    
    public override void InitializePlayer(PlayerViewModel viewModel) {
        base.InitializePlayer(viewModel);
        // This is called when a PlayerViewModel is created
    }
}
