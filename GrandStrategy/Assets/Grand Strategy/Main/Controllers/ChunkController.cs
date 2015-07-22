using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;


public class ChunkController : ChunkControllerBase {
    
    public override void InitializeChunk(ChunkViewModel viewModel) {
        base.InitializeChunk(viewModel);
        // This is called when a ChunkViewModel is created
    }
    
    public override void GenerateChunk(ChunkViewModel viewModel) {
        base.GenerateChunk(viewModel);
    }
}
