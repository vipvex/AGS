using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerUI : PlayerUIBase 
{

    public Image ToolTip;
    public Text ToolTipText;

    public Text unitName;


    protected override void InitializeViewModel(ViewModel model) {
        base.InitializeViewModel(model);
    }
    
    public override void Bind() {
        base.Bind();
    }

    void Update()
    {
        if (Player.SelectedHex != null)
        {
            ToolTip.rectTransform.anchoredPosition = new Vector2(Input.mousePosition.x + ToolTip.rectTransform.sizeDelta.x / 2 + 10,
                                                                 Input.mousePosition.y - ToolTip.rectTransform.sizeDelta.y / 2 - 10);

            //GUI.Box(new Rect(Input.mousePosition.x, Input.mousePosition.y - Screen.height, 200, 100),
            //        "Hex: " + Player.SelectedHex.XIndex + ", " + Player.SelectedHex.YIndex + 
            //        "\n" + Player.SelectedHex.TerrainType + 
            //        "\n Temperature:" + Player.SelectedHex.Temperature +
            //        "\n Humidity:" + Player.SelectedHex.Humidity);
        }
    }

    /// Subscribes to the property and is notified anytime the value changes.
    public override void SelectedHexChanged(Hex hex)
    {
        if (hex != null)
        {
            if (ToolTip.gameObject.activeSelf == false)
            {
                ToolTip.gameObject.SetActive(true);
            }

            ToolTipText.text = "Hex \n" +
                               "Elevation: " + hex.Elevation + "\n" +
                               "Index: " + Player.SelectedHex.XIndex + ", " + Player.SelectedHex.YIndex + "\n" +
                               "Type: " + Player.SelectedHex.TerrainType + "\n" +
                               "Temperature: " + Player.SelectedHex.Temperature + "\n " +
                               "Humidity: " + Player.SelectedHex.Humidity;
        }
        else
        {
            ToolTip.gameObject.SetActive(false);
        }
    }

}
