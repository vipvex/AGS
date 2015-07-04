using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TurboForest))]
public class TurboForestEditor : Editor
{
	public override void OnInspectorGUI()
	{

		var tf = target as TurboForest;

		GUILayout.BeginVertical();

		if (GUILayout.Button("Generate"))
		{
			tf.Generate();
		}

		GUILayout.EndVertical();

		base.OnInspectorGUI();
	}
}
