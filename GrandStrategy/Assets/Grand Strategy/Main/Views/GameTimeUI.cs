using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Kernel;
using uFrame.MVVM;
using uFrame.MVVM.Services;
using uFrame.MVVM.Bindings;
using uFrame.Serialization;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


public class GameTimeUI : GameTimeUIBase
{

    public Text Day, Month, Year, Speed;

    public GameObject PausedPanel;


    public String[] Months;


    protected override void InitializeViewModel(uFrame.MVVM.ViewModel model)
    {
        base.InitializeViewModel(model);
        // NOTE: this method is only invoked if the 'Initialize ViewModel' is checked in the inspector.
        // var vm = model as GameTimeViewModel;
        // This method is invoked when applying the data from the inspector to the viewmodel.  Add any view-specific customizations here.
    }

    public override void Bind()
    {
        base.Bind();
        // Use this.GameTime to access the viewmodel.
        // Use this method to subscribe to the view-model.
        // Any designer bindings are created in the base implementation.
    }


    public override void DayChanged(int arg1)
    {
        Day.text = arg1.ToString();
    }

    public override void MonthChanged(int arg1)
    {
        Month.text = Months[arg1 - 1];
    }

    public override void YearChanged(int arg1)
    {
        Year.text = "Year " + arg1.ToString();
    }

    public override void SeasonChanged(Seasons arg1)
    {
        Day.text = arg1.ToString();
    }

    public override void PausedChanged(Boolean arg1)
    {
        PausedPanel.SetActive(arg1);
    }

    public override void GameSpeedChanged(Int32 arg1)
    {
        Speed.text = arg1.ToString();
    }

}
