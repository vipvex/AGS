using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


public class PlayerView : PlayerViewBase 
{

    private GameObject selectedObj;



    protected override void InitializeViewModel(ViewModel model) {
        base.InitializeViewModel(model);
    }
    
    public override void Bind() {
        base.Bind();
    }

    public override void Update()
    {
        MouseSelect();
    }


    void MouseSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500))
        {

            selectedObj = hit.collider.gameObject;

            // Hovering over gameplay objects
            if (selectedObj.CompareTag("terrain"))
            {
                Player.SelectedHex = Player.Terrain.GetHexAtPos(hit.point);
            }
        }
    }

}
