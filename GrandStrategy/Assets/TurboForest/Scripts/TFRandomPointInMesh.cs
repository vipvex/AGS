using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TFRandomPointOnMesh
{
	float[] areas;
	public float totalSurfaceArea = 0;

	List<int> inds = new List<int>();
	List<Vector3> verts = new List<Vector3>();
	bool initiated = false;

	void Init()
	{
		if (initiated)
			return;

		initiated = true;
		CalculateAreas();
		NormalizeAreaWeights();
	}

	public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
	{
		initiated = false;
		int maxIndex = verts.Count;
		verts.Add(a);
		verts.Add(b);
		verts.Add(c);
		inds.Add(maxIndex);
		inds.Add(maxIndex + 1);
		inds.Add(maxIndex + 2);
	}

	public void SetMeshFrom(GameObject gameObject, MeshFilter mf, bool clear = false)
	{
		initiated = false;

		Mesh mesh = mf.sharedMesh;

		int maxIndex = verts.Count;

		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			verts.Add(gameObject.transform.TransformPoint(mesh.vertices[i]));
		}

		for (int i = 0; i < mesh.triangles.Length; i++)
		{
			inds.Add(mesh.triangles[i] + maxIndex);
		}

	}

	public float GetTotalArea()
	{
		Init();
		return totalSurfaceArea;
	}

	public Vector3 Get()
	{
		Init();

		int tri = SelectRandomTriangle();

		Vector3 a = verts[inds[tri * 3 + 0]];
		Vector3 b = verts[inds[tri * 3 + 1]];
		Vector3 c = verts[inds[tri * 3 + 2]];

		return GetRandomPointInTriangle(a, b, c);

	}

	private static Vector3 GetRandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 ab = b - a;
		Vector3 ac = c - a;

		float r = Random.value;       //  % along ab
		float s = Random.value;       //  % along ac

		if (r + s >= 1f)
		{
			r = 1f - r;
			s = 1f - s;
		}

		//  Now add the two weighted vectors to a
		return a + ((ab * r) + (ac * s));
	}

	void CalculateAreas()
	{

		int triangleCount = inds.Count / 3;

		areas = new float[triangleCount];

		Vector3[] points = new Vector3[3];

		for (int triangleIndex = 0; triangleIndex < triangleCount; triangleIndex++)
		{
			points[0] = verts[inds[triangleIndex * 3 + 0]];
			points[1] = verts[inds[triangleIndex * 3 + 1]];
			points[2] = verts[inds[triangleIndex * 3 + 2]];

			// calculate the three sidelengths and use those to determine the area of the triangle
			// http://www.wikihow.com/Sample/Area-of-a-Triangle-Side-Length
			float a = (points[0] - points[1]).magnitude;
			float b = (points[0] - points[2]).magnitude;
			float c = (points[1] - points[2]).magnitude;

			float s = (a + b + c) / 2;

			areas[triangleIndex] = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
		}
	}

	void NormalizeAreaWeights()
	{

		totalSurfaceArea = 0;

		foreach (float surfaceArea in areas)
		{
			totalSurfaceArea += surfaceArea;
		}

		for (int i = 0; i < areas.Length; i++)
		{
			areas[i] = areas[i] / totalSurfaceArea;
		}
	}

	int SelectRandomTriangle()
	{
		float triangleSelectionValue = Random.value;

		float accumulated = 0;

		for (int i = 0; i < areas.Length; i++)
		{
			accumulated += areas[i];

			if (accumulated >= triangleSelectionValue)
			{
				return i;
			}
		}

		// unless we were handed malformed normalizedAreaWeights, we should have returned from this already.
		//Debug.LogError("Normalized Area Weights were not normalized properly, or triangle selection value was not [0, 1]");

		return 0;

	}

}
