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
using UnityEngine.EventSystems;

public class PlayerUI : PlayerUIBase {
    
    
    protected override void InitializeViewModel(uFrame.MVVM.ViewModel model) {
        base.InitializeViewModel(model);
        // NOTE: this method is only invoked if the 'Initialize ViewModel' is checked in the inspector.
        // var vm = model as PlayerViewModel;
        // This method is invoked when applying the data from the inspector to the viewmodel.  Add any view-specific customizations here.
    }
    
    public override void Bind() {
        base.Bind();
        // Use this.Player to access the viewmodel.
        // Use this method to subscribe to the view-model.
        // Any designer bindings are created in the base implementation.
    }



    public Image ToolTip;
    public Text ToolTipText;

    public Text unitName;

        
    void Update()
    {
        if (Player != null && Player.HoverHex != null)
        {
            ToolTip.rectTransform.anchoredPosition = new Vector2(Input.mousePosition.x + ToolTip.rectTransform.sizeDelta.x / 2 + 10,
                                                                 Input.mousePosition.y - ToolTip.rectTransform.sizeDelta.y / 2 - 10);
        }
    }

    /// Subscribes to the property and is notified anytime the value changes.
    public override void HoverHexChanged(Hex hex)
    {
        if (hex != null)
        {
            if (ToolTip.gameObject.activeSelf == false)
            {
                ToolTip.gameObject.SetActive(true);
            }

            ToolTipText.text = "Hex \n" +
                               "Elevation: " + hex.Elevation + "\n" +
                               "Index: " + hex.XIndex + ", " + hex.YIndex + "\n" +
                               "Type: " + hex.TerrainType + "\n" +
                               "Temperature: " + hex.Temperature + "\n " +
                               "Humidity: " + hex.Humidity;
        }
        else
        {
            ToolTip.gameObject.SetActive(false);
        }
    }
}
