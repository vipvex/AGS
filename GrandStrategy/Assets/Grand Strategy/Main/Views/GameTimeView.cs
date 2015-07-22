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


public class GameTimeView : GameTimeViewBase {
    
    protected override void InitializeViewModel(uFrame.MVVM.ViewModel model) {
        base.InitializeViewModel(model);
        // NOTE: this method is only invoked if the 'Initialize ViewModel' is checked in the inspector.
        // var vm = model as GameTimeViewModel;
        // This method is invoked when applying the data from the inspector to the viewmodel.  Add any view-specific customizations here.
    }
    
    public override void Bind() {
        base.Bind();
        // Use this.GameTime to access the viewmodel.
        // Use this method to subscribe to the view-model.
        // Any designer bindings are created in the base implementation.

        this.OnEvent<StartGameCommand>().Subscribe(StartGame =>
        {
            StartCoroutine(GameTick());
        });

    }

    public override void Update()
    {   
        base.Update();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            ExecuteTogglePause(new TogglePauseCommand() { Sender = GameTime });
        }
    }

    public IEnumerator GameTick()
    { 
        while(true)
        {
            if (GameTime.Paused)
            {
                yield return 1;
            }
            else
            {
                yield return new WaitForSeconds(1f / GameTime.GameSpeed);
                ExecuteGameTick(new GameTickCommand() { Sender = GameTime });
            }
        }
    }
}
