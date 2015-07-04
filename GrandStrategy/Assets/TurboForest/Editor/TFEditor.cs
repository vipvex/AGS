using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TF))]
public class TFEditor : Editor
{

	const float densityMin = 0.001f;
	const float densityMax = 2;
	const float areaMin = .0001f;
	const float areaMax = 1000;

	Quaternion circleRotation = Quaternion.AngleAxis(90, Vector3.left);

	TF tf;
	Event e;

	bool lineMode = false;
	Vector3 lineStart;

	int seed = 0;

	float Round2(float v)
	{
		return ((float)((int)(v * 100))) / 100;
	}
	float Round4(float v)
	{
		return ((float)((int)(v * 10000))) / 10000;
	}

	bool showDensity = false;

	void OnEnable()
	{
		if (Application.isPlaying)
			return;

		if (SceneView.sceneViews.Count > 0)
			(SceneView.sceneViews[0] as SceneView).Focus();

		tf = target as TF;

		var renderers = tf.GetComponentsInChildren<MeshRenderer>();
		foreach (var r in renderers)
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetSelectedWireframeHidden(r, true);
#endif
		}

	}

	void OnDisable()
	{

	}

	public override void OnInspectorGUI()
	{

		tf = target as TF;
		
		base.OnInspectorGUI();

		if (tf.forest == null)
			return;

		GUILayout.BeginVertical();

		if (GUILayout.Button("Generate"))
		{
			if (tf.placedTreesCount == 0 || TFUtils.Confirm("All existing trees will be deleted and generated again. Contine?", "Yes"))
				tf.Generate(tf.forest.density);
		}

		if (GUILayout.Button("Grab from terrains"))
		{
			if (tf.placedTreesCount == 0 || TFUtils.Confirm("All existing trees will be deleted and grabbed again. Contine?", "Yes"))
				tf.GrabFromTerrains();
		}

		if (GUILayout.Button("Align to ground"))
			tf.AlignToGround();

		if (GUILayout.Button("Clear"))
			if (tf.placedTreesCount == 0 || TFUtils.Confirm("All existing trees will be deleted. Contine?", "Yes"))
				tf.Clear();

		if (GUILayout.Button("Rebuild colliders"))
			tf.RebuildColliders();

		if (GUILayout.Button("Clear colliders"))
			tf.ClearColliders();


		tf.forest.globalScale = EditorGUILayout.FloatField("Global scale", tf.forest.globalScale);
		tf.forest.areaRadius = EditorGUILayout.FloatField("Draw radius", tf.forest.areaRadius);
		tf.forest.density = EditorGUILayout.FloatField("Density", tf.forest.density);
		tf.forest.chunkSize = EditorGUILayout.IntField("Chunk size", tf.forest.chunkSize);
		tf.forest.yShift = EditorGUILayout.FloatField("Y shift", tf.forest.yShift);

		GUILayout.Label("trees: " + tf.placedTreesCount);
		GUILayout.Label("chunks: " + tf.chunks.Count);


		foreach (var t in tf.forest.trees)
		{

			if (t == null)
				continue;

			GUILayout.Label(t.name);
			
			GUILayout.BeginHorizontal();

			GUILayout.Label(t.preview);

			GUILayout.BeginVertical();

			t.chance = EditorGUILayout.IntField("Chance", t.chance);
			
			t.size = EditorGUILayout.FloatField("Size", t.size);
			t.sizeRandomize = EditorGUILayout.FloatField("Size randomize", t.sizeRandomize);

			t.brightnessRandomize = Mathf.Clamp01(EditorGUILayout.FloatField("Brightness randomize", t.brightnessRandomize));
			t.saturationRandomize = Mathf.Clamp01(EditorGUILayout.FloatField("Saturation randomize", t.saturationRandomize));
			t.lightMultiplier = Mathf.Max(EditorGUILayout.FloatField("Light multiplier", t.lightMultiplier), 0);
			t.cutout = Mathf.Clamp01(EditorGUILayout.FloatField("Cutout", t.cutout));

			t.material.SetFloat("_BrightnessRandomize", t.brightnessRandomize);
			t.material.SetFloat("_SaturationRandomize", t.saturationRandomize);
			t.material.SetFloat("_LightMult", t.lightMultiplier);
			t.material.SetFloat("_Cutout", t.cutout);

			t.colliderRadius = EditorGUILayout.FloatField("Collider radius", t.colliderRadius);
			t.colliderHeight = EditorGUILayout.FloatField("Collider height", t.colliderHeight);

			EditorUtility.SetDirty(t);

			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		GUILayout.EndVertical();
		
		EditorUtility.SetDirty(tf.forest);
		
	}

	

	void OnSceneGUI()
	{

		tf = target as TF;

		if (tf.forest == null)
			return;

		e = Event.current;
		
		if (!tf.disableHelp)
		{
			Handles.BeginGUI();

			var tipsSize = new Rect(5, 5, 400, 145);

			GUI.color = new Color(0, 0, 0, .5f);

			GUI.Box(tipsSize, GUIContent.none);

			GUI.color = Color.white;

			var wasLabelColor = GUI.skin.label.normal.textColor;
			GUI.skin.label.normal.textColor = new Color(1, 1, 1, .7f);

			GUILayout.BeginArea(tipsSize);

			if (!lineMode)
			{
				GUILayout.Label("Drawing forest tips:");
				GUILayout.Label("Hit Z to place one tree, hold to draw");
				GUILayout.Label("Hit X to place trees in circle, hold to draw");
				GUILayout.Label("Hit C to place trees in rect");
				GUILayout.Label("Hit V to clear trees in circle (hold for continous clear)");
				GUILayout.Label("Hit B to clear random 10% of trees in circle (hold for continous)");
				GUILayout.Label("Hold LeftShift and scroll to change area size");
				GUILayout.Label("Hold LeftShift + LeftAlt and scroll to change drawing density");
			}
			else
			{
				GUILayout.Label("Rect drawing mode:");
				GUILayout.Label("Move mouse to set direction and length");
				GUILayout.Label("Hold LeftShift and scroll to change width");
				GUILayout.Label("Hold LeftControl and scroll to change density");
				GUILayout.Label("Hit C to place trees");
			}

			GUILayout.EndArea();

			GUI.skin.label.normal.textColor = wasLabelColor;


			Handles.EndGUI();
		}

		Handles.CircleCap(0, mousePosition3D, circleRotation, tf.forest.globalScale * .1f);

		if (lineMode)
		{
			DrawLine(false);
		}
		else
		{
			DrawArea(false);
		}

		showDensity = e.modifiers == (EventModifiers.Alt | EventModifiers.Shift);

		if (e.type == EventType.ScrollWheel)
		{
			if (e.modifiers == EventModifiers.Shift)
			{
				var areaRadius = tf.forest.areaRadius;
				areaRadius += areaRadius * -e.delta.y * .05f;
				areaRadius = Mathf.Clamp(areaRadius, areaMin, areaMax);
				tf.forest.areaRadius = areaRadius;
				EditorUtility.SetDirty(tf);
				e.Use();
			}
			if (e.modifiers == (EventModifiers.Alt | EventModifiers.Shift))
			{
				var density = tf.forest.density;
				density -= e.delta.y * .01f;
				density = Mathf.Clamp(density, densityMin, densityMax);
				tf.forest.density = density;
				EditorUtility.SetDirty(tf);
				e.Use();
			} 
			return;
		}

		if (e.isMouse)
		{
			if (e.type == EventType.MouseMove)
			{
				RaycastScene();
			}

		}

		if (e.type == EventType.KeyDown)
		{
			if (RaycastScene())
			{

				switch (e.keyCode)
				{
					case KeyCode.Z:
						if (ResetLine())
							break;
						tf.InitSources();
						tf.AddTree(mousePosition3D);
						tf.BuildMeshes(false);
						e.Use();
						break;
					case KeyCode.X:
						if (ResetLine())
							break;
						DrawArea(true);
						UpdateSeed();
						e.Use();
						break;
					case KeyCode.V:
						tf.InitSources();
						tf.RemoveTreesInArea(mousePosition3D, tf.forest.areaRadius * tf.forest.globalScale);
						tf.BuildMeshes();
						UpdateSeed();
						e.Use();
						break;
					case KeyCode.B:
						UpdateSeed();
						Random.seed = seed;
						tf.InitSources();
						tf.RemoveTreesInArea(mousePosition3D, tf.forest.areaRadius * tf.forest.globalScale, .1f);
						tf.BuildMeshes();
						e.Use();
						break;
					case KeyCode.C:
						lineMode = !lineMode;

						if (lineMode)
						{
							lineStart = mousePosition3D;
							UpdateSeed();
						}
						else
						{
							DrawLine(true);
						}
						e.Use();
						break;
					case KeyCode.Escape:
						lineMode = false;
						e.Use();
						break;
				}
			}

		}
	}

	bool ResetLine()
	{
		if (lineMode)
		{
			lineMode = false;
			return true;
		}
		return false;
	}

	void UpdateSeed()
	{
		seed = System.DateTime.Now.Millisecond;
	}

	void Rect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		Handles.DrawLine(a, b);
		Handles.DrawLine(b, c);
		Handles.DrawLine(c, d);
		Handles.DrawLine(d, a);
	}

	void DrawLine(bool putTrees)
	{

		if (putTrees)
			tf.InitSources();

		Random.seed = seed;

		var dir = mousePosition3D - lineStart;

		var len = dir.magnitude;
		dir.Normalize();
		var cross = Vector3.Cross(dir, Vector3.up);

		var pos = lineStart;
		var rpos = lineStart;

		var right = cross * tf.forest.areaRadius * tf.forest.globalScale;

		var _a = lineStart + right;
		var _b = lineStart - right;
		var _c = mousePosition3D - right;
		var _d = mousePosition3D + right;

		Rect(_a, _b, _c, _d);

		if (!putTrees && !showDensity)
			return;

		Random.seed = seed;

		float area = tf.forest.areaRadius * tf.forest.globalScale;

		int count = (int)((area + area) * len * tf.forest.density * TF.areaTreesDensityMultiplier / tf.forest.globalScale);

		for (int i = 0; i < count; i++)
		{
			var front = dir * (Random.value * len);
			var side = cross * Random.Range(-area, area);

			rpos = pos + front + side;

			Handles.CircleCap(0, rpos, circleRotation, tf.forest.globalScale * .1f);

			if (putTrees)
			{
				if (tf.forest.Linecast(rpos))
					tf.AddTree(tf.forest.linecastResult);
			}
		}

		if (putTrees)
			tf.BuildMeshes(false);

	}

	void DrawArea(bool putTrees)
	{


		Handles.CircleCap(0, mousePosition3D, circleRotation, tf.forest.areaRadius * tf.forest.globalScale);

		if (!putTrees && !showDensity)
			return;

		int count = (int)(Mathf.PI * tf.forest.areaRadius * tf.forest.areaRadius * tf.forest.density * TF.areaTreesDensityMultiplier * tf.forest.globalScale);

		if (putTrees)
			tf.InitSources();

		Random.seed = seed;

		for (int i = 0; i < count; i++)
		{

			var tpos = TFUtils.RandomPointInCircle(mousePosition3D, tf.forest.areaRadius * tf.forest.globalScale);

			Handles.CircleCap(0, tpos, circleRotation, tf.forest.globalScale * .1f);

			if (putTrees)
			{
				if (putTrees)
				{
					if (tf.forest.Linecast(tpos))
						tf.AddTree(tf.forest.linecastResult);
				}
			}
		}

		if (putTrees)
			tf.BuildMeshes(false);

	}

	Vector3 mousePosition3D;

	bool RaycastScene()
	{
		Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
		RaycastHit info;
		if (Physics.Raycast(ray, out info, Mathf.Infinity))
		{
			mousePosition3D = info.point;
			return true;
		}
		return false;
	}

}
