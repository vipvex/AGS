using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TerrainTypesList : ScriptableObject
{
    [SerializeField]
    public List<TerrainTypeInfo> TerrainTypes;
    private Vector2 terrainTypePos;


    public TerrainType CalculateType(int temperature, int humidity)
    {
        terrainTypePos = new Vector2(temperature, humidity);
        
        for (int i = 0; i < TerrainTypes.Count; i++)
        {
            for (int a = 0; a < TerrainTypes[i].Areas.Count; a++)
            {
                //Debug.Log(TerrainTypes[i].Areas[a]);
                if (TerrainTypes[i].Areas[a].Contains(terrainTypePos))
                {
                    //Debug.Log(TerrainTypes[i].terrainType);
                    return TerrainTypes[i].terrainType;
                }
            }
        }
        return TerrainType.Arctic;
    }
}

[System.Serializable]
public class TerrainTypeInfo
{
    public TerrainType terrainType;
    public Color Color;
    public List<Rect> Areas;
}