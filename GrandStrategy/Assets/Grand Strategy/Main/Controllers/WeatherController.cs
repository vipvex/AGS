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


public class WeatherController : WeatherControllerBase {
    
    public override void InitializeWeather(WeatherViewModel viewModel) {
        base.InitializeWeather(viewModel);
        // This is called when a WeatherViewModel is created
    }
}
