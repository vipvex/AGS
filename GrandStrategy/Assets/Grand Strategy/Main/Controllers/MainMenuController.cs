using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.MVVM;
using uFrame.Serialization;
using uFrame.IOC;
using uFrame.Kernel;
using UniRx;


public class MainMenuController : MainMenuControllerBase {
    
    public override void InitializeMainMenu(MainMenuViewModel viewModel) {
        base.InitializeMainMenu(viewModel);
        // This is called when a MainMenuViewModel is created
    }
}
