using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurboForest : MonoBehaviour
{

	List<Chunk> chunks = new List<Chunk>();

	public class Chunk
	{
		public string name;
		public int ix, iz;
		public Material material;
		public List<Quad> quads = new List<Quad>();
	}

	public class Quad // store each tree before generate batch mesh
	{
		public Vector3 pos;
	}

	public Material[] treeMaterials;

	[Range(0.0F, 100.0F)]
	public float baseSize = 1.0f;

	[Range(0.0F, 1.0F)]
	public float sizeRandomize = 0.2f;

	public int treesCount = 10000;
	public bool castShadows = true;
	public int seed = 0;
	public int chunkSize = 1000;

	public LayerMask raycastLayers;
	public float raycastFromY = 10000;
	public float raycastToY = -10000;

	// each tree vertices
	Vector2 qv0 = new Vector2(-1, -1);
	Vector2 qv1 = new Vector2(1, -1);
	Vector2 qv2 = new Vector2(1, 1);
	Vector2 qv3 = new Vector2(-1, 1);

	// uv frame shift (4 frames per 4 rows in tree texture)
	const float frameSize = 1.0f / 4.0f;

	// each tree uvs
	Vector2 uv0 = new Vector2(1, 0);
	Vector2 uv1 = new Vector2(0, 0);
	Vector2 uv2 = new Vector2(0, 1);
	Vector2 uv3 = new Vector2(1, 1);

	public void Generate()
	{

		if (HaveErrors())
			return;

		uv0 = new Vector2(1, 0);
		uv1 = new Vector2(0, 0);
		uv2 = new Vector2(0, 1);
		uv3 = new Vector2(1, 1);

		float scale = baseSize;

		qv0 = new Vector3(-1, -1, 0) * scale;
		qv1 = new Vector3(1, -1, 0) * scale;
		qv2 = new Vector3(1, 1, 0) * scale;
		qv3 = new Vector3(-1, 1, 0) * scale;

		var clearList = GetComponentsInChildren<TurboForestChunk>();

		foreach (var chunk in clearList)
		{
			if (Application.isPlaying)
				Destroy(chunk.gameObject);
			else
				DestroyImmediate(chunk.gameObject);
		}

		chunks = new List<Chunk>();

		var rpm = new TFRandomPointOnMesh();

		var filters = GetComponentsInChildren<MeshFilter>().ToList();
		filters.Add(GetComponent<MeshFilter>());
		filters.RemoveAll(f => f == null);
		filters = filters.Distinct().ToList();
		
		foreach(var f in filters)
			rpm.SetMeshFrom(f.gameObject,f);

		Random.seed = seed;

		for (int i = 0; i < treesCount; i++)
		{
			Vector3 pos = rpm.Get();

			Vector3 castFrom = new Vector3(pos.x, raycastFromY, pos.z);
			Vector3 castTo = new Vector3(pos.x, raycastToY, pos.z);
			RaycastHit info;

			if (Physics.Linecast(castFrom, castTo, out info, raycastLayers))
				pos.y = info.point.y;
			else
				continue;

			Material mat = treeMaterials[Random.Range(0, treeMaterials.Length)];

			int ix = (int)((pos.x + 100000) / chunkSize);
			int iz = (int)((pos.z + 100000) / chunkSize);

			var chunk = chunks.FirstOrDefault(p => p.material == mat && p.ix == ix && p.iz == iz);
			if (chunk == null)
			{
				chunk = new Chunk();
				chunk.material = mat;
				chunk.ix = ix;
				chunk.iz = iz;
				chunk.name = mat.name + "_" + ix.ToString() + "_" + iz.ToString();
				chunks.Add(chunk);
			}

			Quad q = new Quad();
			q.pos = pos;
			chunk.quads.Add(q);

			if (chunk.quads.Count == 10666) // max quads per mesh (42 664 indices)
			{
				BuildMesh(chunk);
				chunk.quads.Clear(); // clear quads list for next mesh
			}

		}

		foreach (var chunk in chunks)
		{
			if (chunk.quads.Count > 0)
			{
				BuildMesh(chunk);
				chunk.quads.Clear(); // clear quads list for next mesh
			}
		}

		System.GC.Collect();
	}

	void BuildMesh(Chunk chunk)
	{

		if (chunk.quads.Count == 0) return;

		Vector3[] verts = new Vector3[chunk.quads.Count * 4]; // sprite center
		Vector2[] uvs = new Vector2[chunk.quads.Count * 4]; // trees uvs
		Vector2[] uvs2 = new Vector2[chunk.quads.Count * 4]; // sprite corner

		int[] indices = new int[chunk.quads.Count * 4]; // quads in mesh

		for (int i = 0; i < chunk.quads.Count; i++) // fill arrays
		{

			Quad q = chunk.quads[i];

			int ii = i * 4;

			float scale = 1.0f - Random.value * sizeRandomize;

			// sprite corners
			uvs2[ii] = qv0 * scale;
			uvs2[ii + 1] = qv1 * scale;
			uvs2[ii + 2] = qv2 * scale;
			uvs2[ii + 3] = qv3 * scale;

			uvs[ii] = uv0;
			uvs[ii + 1] = uv1;
			uvs[ii + 2] = uv2;
			uvs[ii + 3] = uv3;

			indices[ii] = ii;
			indices[ii + 1] = ii + 1;
			indices[ii + 2] = ii + 2;
			indices[ii + 3] = ii + 3;

			// push tree up from groung on it height
			q.pos.y += qv2.y * scale;

			// sprite center position is same for all 4 corners
			verts[ii] = q.pos;
			verts[ii + 1] = q.pos;
			verts[ii + 2] = q.pos;
			verts[ii + 3] = q.pos;
		}

		// creating mesh
		GameObject trees = new GameObject(chunk.name);

		MeshFilter mf = trees.AddComponent<MeshFilter>();

		mf.sharedMesh = new Mesh();
		mf.sharedMesh.vertices = verts;

		mf.sharedMesh.uv = uvs;
		mf.sharedMesh.uv2 = uvs2;

		mf.sharedMesh.SetIndices(indices, MeshTopology.Quads, 0);

		MeshRenderer mr = trees.AddComponent<MeshRenderer>();
		mr.sharedMaterial = chunk.material;

		if (castShadows)
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		else
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		trees.AddComponent<TurboForestChunk>();
		trees.transform.parent = transform;
	}

	void OnDrawGizmos()
	{
		Vector3 size = Vector3.one;
		size.x = chunkSize;
		size.z = size.x;

		Vector3 center = Vector3.zero;
		center.x = chunkSize / 2;
		center.z = center.x;

		Gizmos.DrawWireCube(center, size);
	}

	#region check_errors

	bool HaveErrors()
	{

		if (treeMaterials.Length == 0)
		{
			Debug.LogError("You need to fill Trees Materials in Turbo Forest component");
			return true;
		}

		foreach(var mat in treeMaterials)
			if (mat == null)
			{
				Debug.LogError("One of material in Turbo Forest component is empty.");
				return true;
			}

		if (chunkSize < 1)
		{
			Debug.LogError("Chunk size in Turbo Forest component is very small, need to be not less 1.");
			return true;
		}

		if (raycastLayers.value == 0)
		{
			Debug.LogError("Raycast layers in Turbo Forest component not set.");
			return true;
		}

		var mf = GetComponent<MeshFilter>();
		if (mf == null)
		{
			Debug.LogError("Turbo Forest requires not empty mesh filter component on same object.");
			return true;
		}

		return false;
	}

	#endregion

}
