using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.IOC;
using uFrame.Kernel;
using uFrame.MVVM;
using uFrame.Serialization;
using UnityEngine;


public class GameSceneLoader : GameSceneLoaderBase {
    
    protected override IEnumerator LoadScene(GameScene scene, Action<float, string> progressDelegate) {
        yield break;
    }
    
    protected override IEnumerator UnloadScene(GameScene scene, Action<float, string> progressDelegate) {
        yield break;
    }
}
