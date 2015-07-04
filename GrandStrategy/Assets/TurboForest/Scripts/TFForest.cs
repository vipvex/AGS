using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TFForest : ScriptableObject
{

	public TFTree.TreeType treesType = TFTree.TreeType.BillboardAtlas;

	public List<TFTree> trees;

	public float globalScale = 1;
	public float density = 0.5f;
	public float yShift = 0;
	public float areaRadius = 10;
	public bool castShadows = true;
	public int seed = 0;
	public int chunkSize = 1000;

	public LayerMask raycastLayers;
	public float raycastFromY = 10000;
	public float raycastToY = -10000;

	public TFTree GetTreeByTerrainTree(GameObject terrainTree)
	{
		var c = trees.FirstOrDefault(t => t.terrainTreeConformity == terrainTree);
		return c;
	}

	public bool IsMeshTrees()
	{
		return treesType == TFTree.TreeType.Mesh || treesType == TFTree.TreeType.MeshCullNear;
	}

	[System.NonSerialized]
	public Vector3 linecastResult = Vector3.zero;
	
	public bool Linecast(Vector3 pos)
	{
		Vector3 castFrom = new Vector3(pos.x, raycastFromY, pos.z);
		Vector3 castTo = new Vector3(pos.x, raycastToY, pos.z);
		RaycastHit info;
		if (Physics.Linecast(castFrom, castTo, out info, raycastLayers))
		{
			linecastResult = info.point;
			return true;
		}
		return false;
	}

#if UNITY_EDITOR

	/// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static void CreateAsset<T>() where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T>();

		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path == "")
		{
			path = "Assets";
		}
		else if (Path.GetExtension(path) != "")
		{
			path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
		}

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

		AssetDatabase.CreateAsset(asset, assetPathAndName);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}

	[MenuItem("Assets/Create/TF Forest")]
	public static void CreateOptions()
	{
		TFForest.CreateAsset<TFForest>();
	}
	
	[MenuItem("Assets/Create/TF Tree")]
	public static void CreateTFTree()
	{
		TFForest.CreateAsset<TFTree>();
	}

	[MenuItem("GameObject/3D Object/Turbo Forest")]
	public static void CreateTurboForest()
	{
		var go = new GameObject("TurboForest");
		go.AddComponent<TF>();
		UnityEditor.Selection.activeGameObject = go;
	}

#endif	
}
