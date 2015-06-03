using UnityEngine;
using UnityEditor;

public class YourClassAsset
{
    [MenuItem("Assets/Create/Biomes")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<TerrainTypesList>();
    }
}