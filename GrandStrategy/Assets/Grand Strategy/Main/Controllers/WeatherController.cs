using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


public class WeatherController : WeatherControllerBase {
    
    public override void Setup() {
        base.Setup();
        // This is called when the controller is created
    }
    
    public override void InitializeWeather(WeatherViewModel viewModel) {
        base.InitializeWeather(viewModel);
        // This is called when a WeatherViewModel is created
    }
}
