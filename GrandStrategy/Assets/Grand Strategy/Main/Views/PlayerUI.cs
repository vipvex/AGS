using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerUI : PlayerUIBase {

    public Image toolTip;
    public Text toolTipDescription;

    public Text unitName;


    protected override void InitializeViewModel(ViewModel model) {
        base.InitializeViewModel(model);
    }
    
    public override void Bind() {
        base.Bind();
    }
}
