using UnityEngine;
using System.Collections;

public class TFTree : ScriptableObject
{

	public enum TreeType
	{
		BillboardAtlas = 0, // main technique
		Billboard = 1, // dynamic
		BillboardCylindrical = 2, // turning only by Y
		Mesh = 3, // 2quads cross
		
		BillboardAtlasCullNear = 4,
		BillboardCullNear = 5,
		BillboardCylindricalCullNear = 6,
		MeshCullNear = 7

	}

	public Material material;

	public int chance;

	[Range(0.0F, 100.0F)]
	public float size = 1.0f;

	[Range(0.0F, 1.0F)]
	public float sizeRandomize = 0.2f;

	public float saturationRandomize = 0.5f;
	public float brightnessRandomize = 0.5f;

	public float colliderRadius = 1;
	public float colliderHeight = 10;

	public float cullNearDistance = 200;

	public GameObject terrainTreeConformity;

	public Texture2D preview;

	public float lightMultiplier = 1;
	public float cutout = .5f;

}
