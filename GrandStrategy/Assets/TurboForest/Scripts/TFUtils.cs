using UnityEngine;
using System.Collections;

public class TFUtils
{
	public static Vector3 RandomPointInCircle(Vector3 center, float radius)
	{
		var a = 2 * Mathf.PI * Random.value;
		var r = Mathf.Sqrt(Random.value);
		var x = (radius * r) * Mathf.Cos(a) + center.x;
		var z = (radius * r) * Mathf.Sin(a) + center.z;
		return new Vector3(x, center.y, z);
	}

	public static void ShowProgress(string title, float progress)
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.DisplayProgressBar("TF", title, progress);
#endif
	}
	
	public static void ShowProgress(string title, int i, int count, int step)
	{
#if UNITY_EDITOR

		if ((int)(((float)(i)) / step) * step != i)
			return;

		if (count == 0)
			return;

		UnityEditor.EditorUtility.DisplayProgressBar("TF", title, ((float)(i)) / count);
#endif
	}

	public static void HideProgress()
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.ClearProgressBar();
#endif
	}

	public static void Destroy(Object obj)
	{
		if (!Application.isPlaying)
			MonoBehaviour.DestroyImmediate(obj);
		else
			MonoBehaviour.Destroy(obj);
	}

	public static bool DestroyIfNotNull(Object obj)
	{
		bool result = obj != null;
		
		if(result)
			Destroy(obj);

		return result;
	}
	
	public static bool DestroyIfNotNull(Transform tform)
	{
		bool result = tform != null;

		if (result)
			Destroy(tform.gameObject);

		return result;
	}

	public static bool Confirm(string title, string ok)
	{
#if UNITY_EDITOR
		return UnityEditor.EditorUtility.DisplayDialog("TF", title, ok, "Cancel");
#else
		return true;
#endif
	}

	public static void Notify(string title)
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.DisplayDialog("TF", title, "Got it");
#else
		Debug.Log("TF: " + title);
#endif		
	}

	public static bool NotifyIfNull(string title, Object some)
	{
#if UNITY_EDITOR
		if (some == null)
		{
			Notify(title);
			return true;
		}
		else
			return false;
#else
		return some == null;
#endif
	}

	public static bool IsAtlasType(TFTree.TreeType type)
	{
		switch (type)
		{
			case TFTree.TreeType.BillboardAtlas:
			case TFTree.TreeType.BillboardAtlasCullNear:
				return true;
		}
		return false;
	}

	public static bool IsCylType(TFTree.TreeType type)
	{
		switch (type)
		{
			case TFTree.TreeType.BillboardCylindrical:
			case TFTree.TreeType.BillboardCylindricalCullNear:
				return true;
		}
		return false;
	}
	
	public static bool IsCullNearType(TFTree.TreeType type)
	{
		switch (type)
		{
			case TFTree.TreeType.BillboardAtlasCullNear:
			case TFTree.TreeType.BillboardCylindricalCullNear:
			case TFTree.TreeType.BillboardCullNear:
			case TFTree.TreeType.MeshCullNear:
				return true;
		}
		return false;
	}

	public static bool IsMeshType(TFTree.TreeType type)
	{
		switch (type)
		{
			case TFTree.TreeType.Mesh:
			case TFTree.TreeType.MeshCullNear:
				return true;
		}
		return false;
	}	


}
