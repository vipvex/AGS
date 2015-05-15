using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;


public class ChunkController : ChunkControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeChunk(ChunkViewModel viewModel) {
        base.InitializeChunk(viewModel);
        // This is called when a ChunkViewModel is created
    }
}
