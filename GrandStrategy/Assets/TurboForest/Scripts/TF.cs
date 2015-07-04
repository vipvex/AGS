using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TF : MonoBehaviour
{
	#region consts

	// developer consts, change it using your head
	public const float areaTreesDensityMultiplier = .1f;
	public const float minDensity = 0.001f;
	public const float maxDensity = 4f;

	public const int maxTreesPerChunk = 16250;
	public const int maxTreesPerColliderChunk = 10922;

	const float frameSize = 1.0f / 4.0f;
	public const float negativePositionShift = 1000000;

	#endregion

	public bool disableHelp = false;

	public TFForest forest;

	public GameObject[] distributeObjects;

	[HideInInspector]
	public int placedTreesCount = 0;

	public void Generate(float density)
	{

		if (HaveErrors())
			return;

		var distribs = distributeObjects.Where(o => o != null).ToList();

		if (distribs.Count == 0)
		{
			Debug.LogError("TF need at least one distribute object to generate trees on it.");
			return;
		}

		TFUtils.ShowProgress("Prepare distribute objects", 0);

		var rpm = new TFRandomPointOnMesh();

		List<Terrain> terrains = new List<Terrain>();

		foreach (var go in distribs)
		{
			var filters = go.GetComponentsInChildren<MeshFilter>().ToList();
			filters.Add(go.GetComponent<MeshFilter>());
			filters.RemoveAll(f => f == null);
			filters = filters.Distinct().ToList();

			foreach (var f in filters)
				rpm.SetMeshFrom(go, f, false);

			var terrain = go.GetComponent<Terrain>();
			if (terrain)
				terrains.Add(terrain);

		}

		Random.seed = forest.seed;

		Clear();

		InitSources();

		foreach (var terrain in terrains)
		{
			var td = terrain.terrainData;

			var s = td.size;

			var a = new Vector3(0, 0, 0);
			var b = new Vector3(s.x, 0, 0);
			var c = new Vector3(s.x, 0, s.z);
			var d = new Vector3(0, 0, s.z);

			a += terrain.transform.position;
			b += terrain.transform.position;
			c += terrain.transform.position;
			d += terrain.transform.position;

			rpm.AddTriangle(a, b, c);
			rpm.AddTriangle(c, d, a);
		}

		TFUtils.ShowProgress("Generating", 0);

		if (density == 0)
			density = forest.density;

		int count = (int)(rpm.GetTotalArea() * density * areaTreesDensityMultiplier / forest.globalScale);

		for (int i = 0; i < count; i++)
		{

			TFUtils.ShowProgress("Generating", i, count, 1000);

			Vector3 pos = rpm.Get();

			Vector3 castFrom = new Vector3(pos.x, forest.raycastFromY, pos.z);
			Vector3 castTo = new Vector3(pos.x, forest.raycastToY, pos.z);
			RaycastHit info;

			if (Physics.Linecast(castFrom, castTo, out info, forest.raycastLayers))
				pos.y = info.point.y;
			else
				continue;

			AddTree(pos);

		}

		BuildMeshes(false);

#if UNITY_EDITOR
		UnityEditor.EditorUtility.ClearProgressBar();
#endif
	}

	public void GrabFromTerrains()
	{

		if (HaveErrors())
			return;

		TFUtils.ShowProgress("Prepare distribute objects", 0);

		var terrains = GetTerrains();

		if (terrains.Count == 0)
		{
			Debug.LogError("No any terrain found in distribute objects list");
			return;
		}

		Random.seed = forest.seed;

		Clear();

		InitSources();

		foreach (var terrain in terrains)
		{
			var td = terrain.terrainData;
			var insts = td.treeInstances;
			var protos = td.treePrototypes;

			for (int i = 0; i < insts.Length; i++)
			{
				TFUtils.ShowProgress("Collecting terrain trees", i, insts.Length, 1000);
				var tree = insts[i];
				var pos = Vector3.Scale(tree.position, td.size) + terrain.transform.position;

				var tfTree = forest.GetTreeByTerrainTree(protos[tree.prototypeIndex].prefab);

				if (tfTree)
					AddTree(pos, tfTree, tree.heightScale, tree.color, Quaternion.AngleAxis(tree.rotation, Vector3.up));
			}
		}

		BuildMeshes(false);

#if UNITY_EDITOR
		UnityEditor.EditorUtility.ClearProgressBar();
#endif
	}

	public void AlignToGround()
	{

		int total = chunks.Sum(c => c.trees.Count);
		int cnt = 0;

		foreach (var c in chunks)
		{
			foreach (var t in c.trees)
			{
				TFUtils.ShowProgress("Align to ground", cnt, total, 1000);
				cnt++;

				if (forest.Linecast(t.position))
					t.position.y = forest.linecastResult.y;
			}
			c.needRebuild = true;
		}

		TFUtils.ShowProgress("Building meshes", 0);
		BuildMeshes(true);

		TFUtils.HideProgress();

	}

	List<Terrain> GetTerrains()
	{
		var distribs = distributeObjects.Where(o => o != null).ToList();

		List<Terrain> terrains = new List<Terrain>();

		foreach (var go in distribs)
		{
			var terrain = go.GetComponent<Terrain>();
			if (terrain)
				terrains.Add(terrain);
		}
		return terrains;
	}

	Transform AddChildTransform(string name)
	{
		var go = new GameObject(name);
		go.transform.parent = transform;
		return go.transform;
	}

	void OnDrawGizmos()
	{

		if (forest == null)
			return;

		Vector3 size = Vector3.one;
		size.x = forest.chunkSize;
		size.z = size.x;

		Vector3 center = Vector3.zero;
		center.x = forest.chunkSize / 2;
		center.z = center.x;

		Gizmos.DrawWireCube(center, size);

		/*
		foreach (var c in chunks)
		{
			if (c.mesh != null)
			{
				var b = c.mesh.bounds;
				Gizmos.DrawWireCube(c.diffuse.transform.position + b.center, b.size);
			}
		}
		*/
	}

	#region mesh_building



	[System.Serializable]
	public class Tree
	{
		public Vector3 position;
		public Quaternion rotation;
		public float size;
		public Color color = Color.white;

		[System.NonSerialized]
		public bool deleted = false;

		[System.NonSerialized]
		public TFTree sourceTree;
	}

	[System.Serializable]
	public class Chunk
	{
		public List<Tree> trees;
		public int ix, iz;
		public Vector3 position;
		public TFTree sourceTree;
		public GameObject diffuse;

		public Mesh mesh;

		public int initialChunkSize;

		[System.NonSerialized]
		public bool needRebuild = false;

		[System.NonSerialized]
		public bool deleted = false;

		public void DeleteMeshes()
		{
			if (mesh != null)
				DestroyImmediate(mesh);
			if (diffuse)
				DestroyImmediate(diffuse);

		}

	}

	[HideInInspector]
	public List<Chunk> chunks = new List<Chunk>();

	[HideInInspector]
	public Transform diffuseChunks;
	[HideInInspector]
	public Transform collidersChunks;

	private List<int> sources;

	public void InitSources()
	{
		sources = new List<int>();
		for (int t = 0; t < forest.trees.Count; t++)
		{
			var tree = forest.trees[t];
			for (int i = -1; i < tree.chance; i++)
				sources.Add(t);
		}
	}

	Chunk GetChunk(Vector3 pos, TFTree tree)
	{
		int ix = (int)((pos.x + negativePositionShift) / forest.chunkSize);
		int iz = (int)((pos.z + negativePositionShift) / forest.chunkSize);

		int max = maxTreesPerChunk;
		
		if(forest.IsMeshTrees())
			max = maxTreesPerChunk / 2;

		var c = chunks.FirstOrDefault(tc => tc.sourceTree == tree &&
												tc.ix == ix &&
												tc.iz == iz &&
												tc.trees.Count < max);

		if (c == null)
		{
			c = new Chunk();
			c.sourceTree = tree;
			c.initialChunkSize = forest.chunkSize;
			c.ix = ix;
			c.iz = iz;

			c.position = new Vector3(c.ix * c.initialChunkSize + c.initialChunkSize / 2 - negativePositionShift,
										0,
										c.iz * c.initialChunkSize + c.initialChunkSize / 2 - negativePositionShift);

			c.trees = new List<Tree>();
			c.needRebuild = true;

			chunks.Add(c);
		}
		return c;
	}

	public void AddTree(Vector3 pos)
	{
		var t = new Tree();
		t.position = pos;
		t.rotation = Quaternion.AngleAxis(Random.value * 360f, Vector3.up);

		t.color = Color.white;
		t.deleted = false;

		int type = sources[Random.Range(0, sources.Count)];
		var tree = forest.trees[type];

		t.size = (1.0f - Random.value * tree.sizeRandomize);

		var c = GetChunk(pos, tree);

		c.trees.Add(t);
		c.needRebuild = true;
	}

	public void AddTree(Vector3 pos, TFTree sourceTree, float scale, Color color, Quaternion rotation)
	{
		var t = new Tree();
		t.position = pos;
		t.rotation = rotation;
		t.color = color;
		t.deleted = false;

		t.size = scale;

		var c = GetChunk(pos, sourceTree);

		c.trees.Add(t);
		c.needRebuild = true;
	}

	public void RemoveTreesInArea(Vector3 pos, float area, float chance = 1.1f)
	{
		foreach (var c in chunks)
			foreach (var t in c.trees)
			{
				if (!t.deleted)
					t.deleted = false;
			}

		foreach (var c in chunks)
		{
			foreach (var t in c.trees)
			{
				if (Random.value < chance)
					if ((t.position - pos).magnitude < area)
						t.deleted = true;
			}
		}
	}

	public void Clear()
	{
		foreach (var c in chunks)
			TFUtils.DestroyIfNotNull(c.mesh);

		TFUtils.DestroyIfNotNull(diffuseChunks);

		diffuseChunks = AddChildTransform("Diffuse_chunks");

		chunks.Clear();

		ClearColliders();

		placedTreesCount = 0;
	}

	public void CheckAndRebuildChunks()
	{
		bool needRebuild = false;

		for (int i = 1; i < chunks.Count; i++)
		{
			var prev = chunks[i - 1];
			var c = chunks[i];

			if (c.initialChunkSize != prev.initialChunkSize)
			{
				needRebuild = true;
				break;
			}

			if (c.initialChunkSize != forest.chunkSize || prev.initialChunkSize != forest.chunkSize)
			{
				needRebuild = true;
				break;
			}
		}

		if (needRebuild)
		{
			List<Tree> allTrees = new List<Tree>();

			TFUtils.ShowProgress("Chunk size changing", 0);

			foreach (var c in chunks)
			{
				c.DeleteMeshes();
				foreach (var t in c.trees)
				{
					t.sourceTree = c.sourceTree;
					allTrees.Add(t);
				}
			}

			chunks.Clear();

			for (int i = 0; i < allTrees.Count; i++)
			{
				TFUtils.ShowProgress("Chunk size changing", i, allTrees.Count, 100);

				var t = allTrees[i];
				var c = GetChunk(t.position, t.sourceTree);
				c.trees.Add(t);
			}

			allTrees.Clear();

			TFUtils.HideProgress();

		}

	}

	public void BuildMeshes(bool forceRebuildAll = false)
	{

		CheckAndRebuildChunks();

		foreach (var c in chunks)
			c.deleted = false;

		foreach (var c in chunks)
		{

			if (forceRebuildAll)
				c.needRebuild = true;

			if (c.trees.RemoveAll(t => t.deleted) > 0)
				c.needRebuild = true;

			if (c.trees.Count == 0 || c.sourceTree == null)
			{
				c.trees.Clear();
				c.deleted = true;
			}
		}

		foreach (var c in chunks)
		{
			if (c.needRebuild || c.deleted)
				c.DeleteMeshes();
		}

		chunks.RemoveAll(c => c.deleted);

		placedTreesCount = 0;

		foreach (var c in chunks)
		{

			placedTreesCount += c.trees.Count;

			if (!c.needRebuild)
				continue;

			var tree = c.sourceTree;

			Material material = tree.material;

			GameObject trees = new GameObject(material.name + "_" + c.ix + "_" + c.iz);

			trees.isStatic = true;

			trees.transform.position = c.position;

			MeshData md = new MeshData();

			if(forest.IsMeshTrees())
				md.FillMeshArrays(c, trees, forest);
			else
				md.FillBillboardsArrays(c, trees, forest);

			MeshFilter mf = trees.AddComponent<MeshFilter>();

			mf.sharedMesh = new Mesh();
			c.mesh = mf.sharedMesh;


			mf.sharedMesh.vertices = md.verts;

			mf.sharedMesh.normals = md.normals;

			mf.sharedMesh.uv = md.uvs;
			mf.sharedMesh.uv2 = md.uvs2;
			mf.sharedMesh.colors = md.colors;

			mf.sharedMesh.triangles = md.indices;

			if (!forest.IsMeshTrees())
			{
				var bounds = mf.sharedMesh.bounds;
				var e = bounds.size;
				e.y = md.maxY - md.minY;
				bounds.size = e;
				mf.sharedMesh.bounds = bounds;
			}

			mf.sharedMesh.UploadMeshData(true);

			MeshRenderer mr = trees.AddComponent<MeshRenderer>();
			mr.sharedMaterial = material;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			
			mr.receiveShadows = false;
			mr.useLightProbes = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetSelectedWireframeHidden(mr, true);
#endif

			trees.transform.parent = diffuseChunks;

			c.diffuse = trees;

			if (forest.castShadows)
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			else
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		}

	}

	public class MeshData
	{
		public Vector3[] verts;
		public Vector3[] normals;
		public Vector2[] uvs;
		public Vector2[] uvs2;
		public Color[] colors;
		public int[] indices;
		public float minY = Mathf.Infinity;
		public float maxY = -Mathf.Infinity;

		public void FillBillboardsArrays(Chunk c, GameObject root, TFForest forest)
		{
			var tree = c.sourceTree;

			Vector2 uv0 = new Vector2(1, 0);
			Vector2 uv1 = new Vector2(0, 0);
			Vector2 uv2 = new Vector2(0, 1);
			Vector2 uv3 = new Vector2(1, 1);

			float baseScale = tree.size * forest.globalScale;

			Vector3 qv0 = new Vector3(-1, -1 + forest.yShift, 0) * baseScale;
			Vector3 qv1 = new Vector3(1, -1 + forest.yShift, 0) * baseScale;
			Vector3 qv2 = new Vector3(1, 1 + forest.yShift, 0) * baseScale;
			Vector3 qv3 = new Vector3(-1, 1 + forest.yShift, 0) * baseScale;

			verts = new Vector3[c.trees.Count * 4]; // sprite center
			uvs = new Vector2[c.trees.Count * 4]; // trees uvs
			uvs2 = new Vector2[c.trees.Count * 4]; // sprite corner
			colors = new Color[c.trees.Count * 4]; // colors

			indices = new int[c.trees.Count * 6]; // trees in mesh

			minY = Mathf.Infinity;
			maxY = -Mathf.Infinity;

			for (int i = 0; i < c.trees.Count; i++) // fill arrays
			{

				var t = c.trees[i];

				int ii = i * 4;

				float scale = t.size;

				// push tree up from groung on it height
				var pos = t.position;
				pos = root.transform.InverseTransformPoint(pos);
				pos.y += qv2.y * scale;

				// sprite corners
				uvs2[ii] = qv0 * scale;
				uvs2[ii + 1] = qv1 * scale;
				uvs2[ii + 2] = qv2 * scale;
				uvs2[ii + 3] = qv3 * scale;

				// fix bounds
				minY = Mathf.Min(pos.y + uvs2[ii].y, minY);
				maxY = Mathf.Max(pos.y + uvs2[ii + 2].y, maxY);

				uvs[ii] = uv0;
				uvs[ii + 1] = uv1;
				uvs[ii + 2] = uv2;
				uvs[ii + 3] = uv3;

				int iii = i * 6;

				indices[iii] = ii;
				indices[iii + 1] = ii + 1;
				indices[iii + 2] = ii + 2;
				indices[iii + 3] = ii + 2;
				indices[iii + 4] = ii + 3;
				indices[iii + 5] = ii;

				// sprite center position is same for all 4 corners
				verts[ii] = pos;
				verts[ii + 1] = pos;
				verts[ii + 2] = pos;
				verts[ii + 3] = pos;

				colors[ii] = t.color;
				colors[ii + 1] = t.color;
				colors[ii + 2] = t.color;
				colors[ii + 3] = t.color;
			}
		}

		public void FillMeshArrays(Chunk c, GameObject root, TFForest forest)
		{
			var tree = c.sourceTree;

			Vector2 uv0 = new Vector2(1, 0);
			Vector2 uv1 = new Vector2(0, 0);
			Vector2 uv2 = new Vector2(0, 1);
			Vector2 uv3 = new Vector2(1, 1);

			float baseScale = tree.size * forest.globalScale;

			Vector3 qv0 = new Vector3(-1, -1 + forest.yShift, 0) * baseScale;
			Vector3 qv1 = new Vector3(1, -1 + forest.yShift, 0) * baseScale;
			Vector3 qv2 = new Vector3(1, 1 + forest.yShift, 0) * baseScale;
			Vector3 qv3 = new Vector3(-1, 1 + forest.yShift, 0) * baseScale;

			Vector3 qv4 = new Vector3(0, -1 + forest.yShift, 1) * baseScale;
			Vector3 qv5 = new Vector3(0, -1 + forest.yShift, -1) * baseScale;
			Vector3 qv6 = new Vector3(0, 1 + forest.yShift, -1) * baseScale;
			Vector3 qv7 = new Vector3(0, 1 + forest.yShift, 1) * baseScale;

			verts = new Vector3[c.trees.Count * 8]; // sprite center
			normals = new Vector3[c.trees.Count * 8]; // sprite center
			uvs = new Vector2[c.trees.Count * 8]; // trees uvs
			uvs2 = new Vector2[c.trees.Count * 8]; // sprite corner
			colors = new Color[c.trees.Count * 8]; // colors

			indices = new int[c.trees.Count * 12]; // trees in mesh

			minY = Mathf.Infinity;
			maxY = -Mathf.Infinity;

			for (int i = 0; i < c.trees.Count; i++) // fill arrays
			{

				var t = c.trees[i];

				int ii = i * 8;

				float scale = t.size;

				// push tree up from groung on it height
				var pos = t.position;
				pos = root.transform.InverseTransformPoint(pos);
				pos.y += qv2.y * scale;

				// sprite center position is same for all 4 corners
				verts[ii] = pos + t.rotation * qv0 * scale;
				verts[ii + 1] = pos + t.rotation * qv1 * scale;
				verts[ii + 2] = pos + t.rotation * qv2 * scale;
				verts[ii + 3] = pos + t.rotation * qv3 * scale;
				verts[ii + 4] = pos + t.rotation * qv4 * scale;
				verts[ii + 5] = pos + t.rotation * qv5 * scale;
				verts[ii + 6] = pos + t.rotation * qv6 * scale;
				verts[ii + 7] = pos + t.rotation * qv7 * scale;

				var n1 = t.rotation * Vector3.back;
				var n2 = t.rotation * Vector3.left;

				normals[ii] = n1;
				normals[ii + 1] = n1;
				normals[ii + 2] = n1;
				normals[ii + 3] = n1;
				normals[ii + 4] = n2;
				normals[ii + 5] = n2;
				normals[ii + 6] = n2;
				normals[ii + 7] = n2;

				uvs[ii] = uv0;
				uvs[ii + 1] = uv1;
				uvs[ii + 2] = uv2;
				uvs[ii + 3] = uv3;
				uvs[ii + 4] = uv0;
				uvs[ii + 5] = uv1;
				uvs[ii + 6] = uv2;
				uvs[ii + 7] = uv3;

				for (int j = 0; j < 8; j++)
					uvs2[ii + j] = uvs[ii + j];

				int iii = i * 12;

				indices[iii] = ii;
				indices[iii + 1] = ii + 1;
				indices[iii + 2] = ii + 2;
				indices[iii + 3] = ii + 2;
				indices[iii + 4] = ii + 3;
				indices[iii + 5] = ii;

				indices[iii + 6] = ii + 4;
				indices[iii + 7] = ii + 5;
				indices[iii + 8] = ii + 6;
				indices[iii + 9] = ii + 6;
				indices[iii + 10] = ii + 7;
				indices[iii + 11] = ii + 4;

				colors[ii] = t.color;
				colors[ii + 1] = t.color;
				colors[ii + 2] = t.color;
				colors[ii + 3] = t.color;
				colors[ii + 4] = t.color;
				colors[ii + 5] = t.color;
				colors[ii + 6] = t.color;
				colors[ii + 7] = t.color;
			}
		}
	}


	#endregion

	#region colliders_building

	[System.Serializable]
	public class ColliderCunk
	{
		public Mesh mesh;
		public GameObject gameObject;
		
		[System.NonSerialized]
		public List<Tree> trees = new List<Tree>();
	}

	[HideInInspector]
	public List<ColliderCunk> colliderChunks = new List<ColliderCunk>();

	public void ClearColliders()
	{
		foreach (var cc in colliderChunks)
			TFUtils.DestroyIfNotNull(cc.mesh);

		colliderChunks.Clear();

		TFUtils.DestroyIfNotNull(collidersChunks);

	}

	public void RebuildColliders()
	{

		TFUtils.ShowProgress("Collectind trees positions", 0);

		ClearColliders();

		collidersChunks = AddChildTransform("Colliders_chunks");

		foreach (var c in chunks)
		{
			foreach (var t in c.trees)
			{
				var cc = colliderChunks.FirstOrDefault(tc => tc.trees.Count < maxTreesPerColliderChunk);
				if (cc == null)
				{
					cc = new ColliderCunk();
					colliderChunks.Add(cc);
				}
				t.sourceTree = c.sourceTree;
				cc.trees.Add(t);
			}
		}

		Vector3[] v = new Vector3[6];
		List<int> inds = new List<int>();

		v[0] = new Vector3(-1, 0, -1);
		v[1] = new Vector3(1, 0, -1);
		v[2] = new Vector3(0, 0, 1);

		v[3] = v[0];
		v[4] = v[1];
		v[5] = v[2];

		v[3].y = 1;
		v[4].y = 1;
		v[5].y = 1;

		AddColliderQuad(0, 1, 4, 3, inds);
		AddColliderQuad(1, 2, 5, 4, inds);
		AddColliderQuad(2, 0, 3, 5, inds);

		int total = colliderChunks.Sum(cc => cc.trees.Count);
		int cnt = 0;

		Random.seed = forest.seed;

		foreach (var cc in colliderChunks)
		{
			List<Vector3> verts = new List<Vector3>();
			List<int> indices = new List<int>();

			for (int i = 0; i < cc.trees.Count; i++)
			{

				TFUtils.ShowProgress("Building colliders", cnt, total, 200);
				cnt++;

				var tree = cc.trees[i];

				int maxIndex = verts.Count;

				var rot = Quaternion.AngleAxis(Random.value * 360, Vector3.up);

				for (int j = 0; j < 6; j++)
				{
					Vector3 cv = rot * v[j];
					cv.x *= tree.sourceTree.colliderRadius;
					cv.z *= tree.sourceTree.colliderRadius;
					cv.y *= tree.sourceTree.colliderHeight;

					verts.Add(cv + tree.position);
				}

				for (int j = 0; j < 18; j++)
					indices.Add(inds[j] + maxIndex);

			}

			var go = new GameObject("ColliderCunk");
			go.transform.parent = collidersChunks;

			cc.mesh = new Mesh();
			cc.mesh.vertices = verts.ToArray();
			cc.mesh.triangles = indices.ToArray();

			var mf = go.AddComponent<MeshFilter>();
			mf.sharedMesh = cc.mesh;

			go.AddComponent<MeshCollider>();

			cc.trees.Clear();
		}

		TFUtils.HideProgress();

	}

	void AddColliderQuad(int a, int b, int c, int d, List<int> inds)
	{
		inds.Add(c);
		inds.Add(b);
		inds.Add(a);

		inds.Add(a);
		inds.Add(d);
		inds.Add(c);
	}

	#endregion

	#region check_errors

	bool HaveErrors()
	{
		if (distributeObjects.Length == 0)
		{
			TFUtils.Notify("You need to add at least one distribute object to TF!");
			return true;
		}

		for (int i = 0; i < distributeObjects.Length; i++)
		{
			if (TFUtils.NotifyIfNull("One of distribute object is null in TF!", distributeObjects[i]))
				return true;
		}

		if (TFUtils.NotifyIfNull("forest field in TF component is empty.", forest))
			return true;

		if (forest.trees.Count == 0)
		{
			TFUtils.Notify("Forest opions must have at last one tree!");
			
			return true;
		}

		for (int i = 0; i < forest.trees.Count; i++)
		{
			if (TFUtils.NotifyIfNull("One of trees in forest options is null!", forest.trees[i]))
				return true;			
		}

		if (forest.chunkSize < 100)
		{
			TFUtils.Notify("Chunk size in Turbo Forest options is very small, need to be not less 100.");
			return true;
		}

		if (forest.raycastLayers.value == 0)
		{
			TFUtils.Notify("Raycast layers in Turbo Forest component not set.");
			return true;
		}

		return false;
	}

	#endregion

}
