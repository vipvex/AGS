using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TFAtlasRenderer))]
public class TFAtlasRendererEditor : Editor
{
	public override void OnInspectorGUI()
	{
		GUILayout.BeginVertical();

		if (GUILayout.Button("Render visible"))
			(target as TFAtlasRenderer).Render(true);
		
		if (GUILayout.Button("Render all"))
			(target as TFAtlasRenderer).Render(false);

		GUILayout.EndVertical();

		base.OnInspectorGUI();
	}
}
