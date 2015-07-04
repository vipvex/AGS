using UnityEngine;
using System.Collections;
using System.Linq;

public class TFTreeRenderer : MonoBehaviour
{

	public Transform mainCamera;
	public Transform directionalLight;

	public Transform treeDiffuseCamera;
	public Transform treeShadowCamera;

	void Start ()
	{
		if (mainCamera == null)
			mainCamera = Camera.main.transform;

		if (directionalLight == null)
		{
			var light = Object.FindObjectsOfType<Light>().FirstOrDefault(l => l.type == LightType.Directional);
			if(light)
				directionalLight = light.transform;
		}
	}
	
	void Update ()
	{
		if (treeDiffuseCamera && mainCamera)
			treeDiffuseCamera.rotation = mainCamera.rotation;

		if(treeShadowCamera && directionalLight)
			treeShadowCamera.rotation = directionalLight.rotation;
	}
}
