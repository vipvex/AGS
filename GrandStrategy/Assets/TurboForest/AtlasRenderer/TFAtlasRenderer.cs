using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TFAtlasRenderer : MonoBehaviour
{
#if UNITY_EDITOR

	public Camera cam;

	public Shader atlasRenderingShader;

	const int previewSize = 100;
	
	public int singleSize = 512;
	public int atlasSize = 1024;

	public TFForest forest;

	public Material defaultDiffuseMaterial;

	TFTree.TreeType treeType;

	public void Render(bool visibleOnly = true)
	{

		treeType = forest.treesType;

		List<Transform> transforms = new List<Transform>();
		List<Transform> visible = new List<Transform>();

		foreach (Transform t in transform)
		{
			if (t == cam.transform)
				continue;

			if (t == transform)
				continue;

			if (visibleOnly && !t.gameObject.activeSelf)
				continue;

			if (t.gameObject.activeSelf)
				visible.Add(t);

			transforms.Add(t);
		}

		foreach (var t in transforms)
			t.gameObject.SetActive(false);

		foreach (var child in transforms)
		{
			child.gameObject.SetActive(true);

			RenderModel(child);

			child.gameObject.SetActive(false);
		}

		foreach (var t in visible)
			t.gameObject.SetActive(true);

		TFUtils.ShowProgress("Refreshing assets", 1);

		UnityEditor.AssetDatabase.Refresh();

		TFUtils.HideProgress();
	}

	void RenderModel(Transform model)
	{
		var mrs = model.GetComponentsInChildren<MeshRenderer>().ToList();
		mrs.Add(model.GetComponent<MeshRenderer>());

		mrs.RemoveAll(r => r == null);

		List<Material> materials = new List<Material>();

		foreach (var mr in mrs)
		{
			foreach (var mat in mr.sharedMaterials)
			{
				if (mat.shader == atlasRenderingShader)
					materials.Add(mat);
			}
		}

		

		var frameBuffer = new RenderTexture(singleSize, singleSize, 24);
		var frameBufferA = new RenderTexture(atlasSize / 4, atlasSize / 4, 24);
		var previewFrameBuffer = new RenderTexture(previewSize, previewSize, 24);

		Texture2D result = new Texture2D(singleSize, singleSize);
		Texture2D resultN = new Texture2D(singleSize, singleSize);
		Texture2D resultA = new Texture2D(atlasSize, atlasSize);
		Texture2D resultAN = new Texture2D(atlasSize, atlasSize);
		Texture2D resultP = new Texture2D(previewSize, previewSize);

		TFUtils.ShowProgress("Creating preview for: " + model.name, 0);

		// PREVIEW

		var camColor = cam.backgroundColor;
		var c = Color.white;
		c.a = 0;

		cam.backgroundColor = c;

		cam.targetTexture = previewFrameBuffer;

		cam.transform.rotation = Quaternion.identity;

		materials.ForEach(m => m.SetFloat("_normalsMode", 0));
		materials.ForEach(m => m.SetFloat("_simulateTFLight", 1));

		cam.Render();

		RenderTexture.active = previewFrameBuffer;
		resultP.ReadPixels(new Rect(0, 0, previewSize, previewSize), 0, 0);
		RenderTexture.active = null;

		cam.backgroundColor = camColor;

		materials.ForEach(m => m.SetFloat("_simulateTFLight", 0));		

		// SINGLE

		TFUtils.ShowProgress("Creating preview for: " + model.name, 0);

		cam.targetTexture = frameBuffer;

		cam.transform.rotation = Quaternion.identity;

		materials.ForEach(m => m.SetFloat("_normalsMode", 0));

		cam.Render();

		RenderTexture.active = frameBuffer;
		result.ReadPixels(new Rect(0, 0, frameBuffer.width, frameBuffer.height), 0, 0);
		RenderTexture.active = null;

		materials.ForEach(m => m.SetFloat("_normalsMode", 1));

		cam.Render();

		RenderTexture.active = frameBuffer;
		resultN.ReadPixels(new Rect(0, 0, frameBuffer.width, frameBuffer.height), 0, 0);
		RenderTexture.active = null;

		materials.ForEach(m => m.SetFloat("_normalsMode", 0));

		// ATLAS
		UnityEditor.EditorUtility.DisplayProgressBar("TF", "Creating atlas for: " + model.name, 0);

		cam.targetTexture = frameBufferA;

		float frame = 0;

		int w = atlasSize / 4;
		int h = w;

		for (int j = 0; j < 4; j++)
		{
			for (int i = 0; i < 4; i++)
			{

				float delta = frame / 16;

				frame += 1.0666f;

				UnityEditor.EditorUtility.DisplayProgressBar("TF", "Creating atlas for: " + model.name, delta);

				float angle = Mathf.Lerp(0, -90f, 1f - delta);

				cam.transform.rotation = Quaternion.AngleAxis(angle, Vector3.left);

				materials.ForEach(m => m.SetFloat("_normalsMode", 0));

				cam.Render();

				RenderTexture.active = frameBufferA;
				resultA.ReadPixels(new Rect(0, 0, w, h), (3 - i) * w, j * h);
				RenderTexture.active = null;

				materials.ForEach(m => m.SetFloat("_normalsMode", 1));

				cam.Render();

				RenderTexture.active = frameBufferA;
				resultAN.ReadPixels(new Rect(0, 0, w, h), (3 - i) * w, j * h);
				RenderTexture.active = null;

				materials.ForEach(m => m.SetFloat("_normalsMode", 0));

			}
		}


		cam.transform.rotation = Quaternion.identity;
		cam.targetTexture = null;

		string prefix = GetPrefix();

		var diffuseName = model.name + "";
		var normalsName = model.name + "_n";
		var diffuseAName = model.name + "_a";
		var normalsAName = model.name + "_a_n";
		var previewName = model.name + "_preview";

		SaveTexture(result, diffuseName);
		SaveTexture(resultN, normalsName);
		SaveTexture(resultA, diffuseAName);
		SaveTexture(resultAN, normalsAName);
		SaveTexture(resultP, previewName);

		var diffuse = LoadTexture(diffuseName);
		var diffuseA = LoadTexture(diffuseAName);
		var normals = LoadTexture(normalsName);
		var normalsA = LoadTexture(normalsAName);
		var preview = LoadTexture(previewName);

		CreateAdditionalAssets(model.name, diffuse, diffuseA, normals, normalsA, preview);

		DestroyImmediate(result);
		DestroyImmediate(resultN);
		DestroyImmediate(resultA);
		DestroyImmediate(resultAN);
		DestroyImmediate(resultP);

		DestroyImmediate(frameBuffer);
		DestroyImmediate(frameBufferA);
		DestroyImmediate(previewFrameBuffer);

	}

	void SaveTexture(Texture2D tex, string name)
	{
		UnityEditor.EditorUtility.DisplayProgressBar("TF", "Saving: " + name, .5f);

		var bytes = tex.EncodeToPNG();
		var file = File.Open(Application.dataPath + "/TurboForest/Atlases/" + name + ".png", FileMode.Create);
		var binary = new BinaryWriter(file);
		binary.Write(bytes);
		file.Close();
	}

	void CreateAdditionalAssets(string name, Texture2D diffuse, Texture2D diffuseA, Texture2D normals, Texture2D normalsA, Texture2D preview)
	{
		var prefix = GetPrefix();

		string treesPath = "Assets/TurboForest/_Trees/";
		string treePath = treesPath + name;

		if (!UnityEditor.AssetDatabase.IsValidFolder(treePath))
			UnityEditor.AssetDatabase.CreateFolder("Assets/TurboForest/_Trees", name);

		treePath += "/";

		var tree = CreateIfNotExist<TFTree>(name, treesPath + name + prefix + ".asset");

		var matName = treePath + name + prefix;

		var material = CreateIfNotExist<Material>(matName, matName + ".mat", defaultDiffuseMaterial);

		material.shader = defaultDiffuseMaterial.shader;

		if (TFUtils.IsAtlasType(forest.treesType))
		{
			material.SetTexture("_MainTex", diffuseA);
			material.SetTexture("_Bump", normalsA);
		}
		else
		{
			material.SetTexture("_MainTex", diffuse);
			material.SetTexture("_Bump", normals);
		}

		if (TFUtils.IsCylType(forest.treesType))
		{
			material.SetTexture("_ShadowTex", diffuseA);
		}

		tree.material = material;
		tree.preview = preview;

		if (!forest.trees.Contains(tree))
		{
			forest.trees.Add(tree);
			UnityEditor.EditorUtility.SetDirty(forest);
		}

		forest.trees.RemoveAll(t => t == null);

		UnityEditor.EditorUtility.SetDirty(tree);

		UnityEditor.AssetDatabase.SaveAssets();



	}

	Texture2D LoadTexture(string name)
	{
		return UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/TurboForest/Atlases/" + name + ".png", typeof(Texture2D)) as Texture2D;
	}

	bool assetCreated = false;

	T CreateIfNotExist<T>(string name, string path, Material defaulMaterial = null) where T : Object
	{
		var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T));

		assetCreated = false;

		if (asset == null)
		{
			assetCreated = true;

			if (typeof(T) == typeof(Material))
				asset = new Material(defaulMaterial);
			else
				asset = System.Activator.CreateInstance<T>();

			asset.name = name;
			UnityEditor.AssetDatabase.CreateAsset(asset, path);

			Debug.Log("Created: " + name + "<" + typeof(T).ToString() + ">");
		}

		return asset as T;
	}

	public string GetPrefix()
	{
		string prefix = "";
		
		if (TFUtils.IsAtlasType(treeType))
			prefix = "_a";

		if (TFUtils.IsCylType(treeType))
			prefix += "_c";

		if (TFUtils.IsMeshType(treeType))
			prefix += "_m";

		if (TFUtils.IsCullNearType(treeType))
			prefix += "_cn";
		
		return prefix;
	}
	

#endif
}
