using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;


public class TerrainPreviewView : TerrainPreviewViewBase {


    public Texture2D terrainHeightsTexture;
    public Texture2D terrainHexElevationsTexture;

    public Gradient terrainGradient;
    public Gradient rainfallGradient;
    public int iterations = 0;


    public Vector2 terrainScrollPosPreview;
    public Rect terrainScrollRectPreview;
    public float terrainScrollScale = 1;
    public float mouseSpeed = 10;



    protected override void InitializeViewModel(ViewModel model)
    {
        base.InitializeViewModel(model);
    }

    public override void Bind()
    {
        base.Bind();
    }

    public void OnGUI()
    {
        //GUILayout.Label("Map Preview " + iterations);
        if (GUILayout.Button("Generate Terrain", GUILayout.Width(200), GUILayout.Height(75)))
        {
            this.Publish(new GenerateTerrainCommand() { Sender = Terrain });
        }

        if (terrainHexElevationsTexture)
        {
            //GUI.DrawTexture(new Rect(terrainScrollPosPreview.x, terrainScrollPosPreview.y, Screen.height * terrainScrollScale, Screen.height * terrainScrollScale), terrainHexElevationsTexture);
        }
    }

    public override void Update()
    {
        terrainScrollScale += Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.Mouse0))
        {
            terrainScrollPosPreview += new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * mouseSpeed * terrainScrollScale;
        }
    }

    public override void GenerateChunksExecuted(GenerateChunksCommand command)
    {
        float[,] hexHeights = new float[Terrain.Width, Terrain.Height];
        for (int x = 0; x < Terrain.Width; x++)
        {
            for (int y = 0; y < Terrain.Height; y++)
            {
                hexHeights[x, y] = Terrain.Hexes[x, y].Elevation / Terrain.Elevations;

                if (Terrain.Hexes[x, y].TerrainType == TerrainType.River)
                {
                    hexHeights[x, y] = 0;
                }
            }
        }

        if (Terrain.TerrainHeights != null)
        {
            terrainHexElevationsTexture = DiamondSquare.ToTexture2D(hexHeights, Terrain.SeaLevel / Terrain.Elevations);
            //terrainHeightsTexture = DiamondSquare.ToTexture2D(Terrain.TerrainHeights, Terrain.SeaLevel / Terrain.Elevations);
        }
    }
}
