using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.IOC;
using uFrame.Kernel;
using uFrame.MVVM;
using uFrame.Serialization;
using UnityEngine;


public class TestingSceneLoader : TestingSceneLoaderBase {
    
    protected override IEnumerator LoadScene(TestingScene scene, Action<float, string> progressDelegate) {
        yield break;
    }
    
    protected override IEnumerator UnloadScene(TestingScene scene, Action<float, string> progressDelegate) {
        yield break;
    }
}
