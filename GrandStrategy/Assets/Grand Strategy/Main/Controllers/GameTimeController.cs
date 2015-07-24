using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFrame.Serialization;
using uFrame.MVVM;
using uFrame.Kernel;
using uFrame.IOC;
using UniRx;
using UnityEngine;

public class GameTimeController : GameTimeControllerBase {
    
    public override void InitializeGameTime(GameTimeViewModel viewModel) {
        base.InitializeGameTime(viewModel);
        // This is called when a GameTimeViewModel is created


    }



    public override void Setup()
    {
        base.Setup();

        //Debug.Log("Setup!");
        //
        //this.OnEvent<StartGameCommand>().Subscribe(StartGame =>
        //{
        //    Debug.Log("Game started!");
        //
        //    //Observable.Interval(new TimeSpan(((GameLogicViewModel)StartGame.Sender)))
        //    this.Publish(new GameTickCommand());
        //
        //    EventAggregator.Publish(new GameTickCommand());
        //
        //});
    }


    public override void GameTick(GameTimeViewModel viewModel, GameTick arg)
    {
        Debug.Log("Game tick");
    }

    public override void GameTickHandler(GameTickCommand command)
    {

        GameTimeViewModel GameTime = ((GameTimeViewModel)command.Sender);

        GameTime.Day += 1;

        if (GameTime.Day > 30)
        {
            GameTime.Day = 1;
            GameTime.Month += 1;
        }

        if (GameTime.Month > 11)
        {
            GameTime.Month = 1;
            GameTime.Year += 1;
        }
    }


    public override void IncreaseGameSpeed(GameTimeViewModel GameTime)
    {
        if (GameTime.Paused == true)
        {
            GameTime.Paused = false;
            return;
        }

        GameTime.GameSpeed += 1;

        if (GameTime.GameSpeed > 5)
        {
            GameTime.GameSpeed = 5;
        }
    }

    public override void DecreaseGameSpeed(GameTimeViewModel GameTime)
    {
        if (GameTime.Paused == true)
        {
            GameTime.Paused = false;
            return;
        }

        GameTime.GameSpeed -= 1;

        if (GameTime.GameSpeed < 1)
        {
            GameTime.GameSpeed = 1;
        }
    }

    public override void TogglePause(GameTimeViewModel GameTime)
    {
        base.TogglePause(GameTime);

        GameTime.Paused = !GameTime.Paused;
        Debug.Log("Pausing");
    }

}
