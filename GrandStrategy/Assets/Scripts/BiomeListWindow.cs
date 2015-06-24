// C# example:
using UnityEngine;
using UnityEditor;
public class BiomeListWindow : EditorWindow
{

    private static BiomeListWindow window = null;
    private TerrainTypesList terrainTypes;
    private int cellSize = 10;
    private int selectedTerrainType = 0;
    private int selectedTerrainArea = 0;


    [MenuItem("Window/Biome list editor")]
    private static void Init()
    {

        // Get existing open window or if none, make a new one:

        if (window == null)
        {
            window = (BiomeListWindow)EditorWindow.GetWindow(typeof(BiomeListWindow));
        }
    }

    private void OnGUI()
    {
        if (Selection.activeObject && Selection.activeObject.GetType() == typeof(TerrainTypesList))
        {
            if (terrainTypes == null)
            {
                terrainTypes = (TerrainTypesList)Selection.activeObject;
            }

            GUI.Box(new Rect(0, 0, 50 * 10, 50 * 10), "");
            for (int x = 0; x < 11; x++)
            {
                GUI.Label(new Rect(x * 50, 500, 100, 50), "" + (x - 2) * 5);
                GUI.Label(new Rect(500, x * 50, 100, 50), "" + (10 - x) * 5);
            }


            if (terrainTypes.TerrainTypes != null)
            {
                for (int i = 0; i < terrainTypes.TerrainTypes.Count; i++)
			    {

                    GUI.backgroundColor = terrainTypes.TerrainTypes[i].Color;

                    for (int a = 0; a < terrainTypes.TerrainTypes[i].Areas.Count; a++)
			        {
                        if (GUI.Button(new Rect((terrainTypes.TerrainTypes[i].Areas[a].x + 10) * 10,
                                               (50 - terrainTypes.TerrainTypes[i].Areas[a].y - terrainTypes.TerrainTypes[i].Areas[a].height) * 10,
                                               terrainTypes.TerrainTypes[i].Areas[a].width * 10,
                                               (terrainTypes.TerrainTypes[i].Areas[a].height) * 10),
                                               terrainTypes.TerrainTypes[i].terrainType.ToString()))
                        {
                            selectedTerrainType = i;
                            selectedTerrainArea = a;
                        }
			        }
			    }
            }

            GUI.backgroundColor = Color.white;

            GUILayout.BeginArea(new Rect(Screen.width - 350, 50, 125, Screen.height));

            if (terrainTypes.TerrainTypes[selectedTerrainType].Areas.Count > 0 && terrainTypes.TerrainTypes[selectedTerrainType].Areas.Count - 1 >= selectedTerrainArea)
            {
                terrainTypes.TerrainTypes[selectedTerrainType].Areas[selectedTerrainArea] = EditorGUILayout.RectField(terrainTypes.TerrainTypes[selectedTerrainType].Areas[selectedTerrainArea]);
                //terrainTypes.TerrainTypes[selectedTerrainType].Areas[selectedTerrainArea].x = int.Parse(GUILayout.TextField("" + terrainTypes.TerrainTypes[selectedTerrainType].Areas[selectedTerrainArea].x));
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(Screen.width - 155, 0, 150, Screen.height));
            for (int i = 0; i < terrainTypes.TerrainTypes.Count; i++)
		    {
                GUI.backgroundColor = terrainTypes.TerrainTypes[i].Color;
			    if (GUILayout.Button(terrainTypes.TerrainTypes[i].terrainType.ToString(), GUILayout.Width(150), GUILayout.Height(35)))
                {
                    selectedTerrainType = i;
                    selectedTerrainArea = 0;
                }
		    }

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Add Area", GUILayout.Width(150), GUILayout.Height(35)))
            {
                terrainTypes.TerrainTypes[selectedTerrainType].Areas.Add(new Rect(0, 0, 10, 10));
            }
            GUILayout.EndArea();
        }
    }
}