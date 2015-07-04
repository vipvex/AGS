using UnityEngine;
using System.Collections;

public class TFOrbitCamera : MonoBehaviour 
{

	float rx = 45;
	float ry = 0;
	float zoom = 300;

	Vector3 pos = Vector3.zero;
	Vector3 lmp = Vector3.zero;

	void Start () 
	{
		UpdateTransform();
	}
	
	void Update () 
	{
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
		{
			lmp = Input.mousePosition;
		}
		if (Input.GetMouseButton(0))
		{
			Vector3 ms = Input.mousePosition - lmp;
			lmp = Input.mousePosition;

			float mxs = ms.x * .5f;
			float mys = ms.y * .5f;

			rx -= mys;
			ry += mxs;

			while (ry > 360) ry -= 360;
			while (ry < 0) ry += 360;

			rx = Mathf.Clamp(rx, 1, 89);

			UpdateTransform();
		}
		if (Input.GetMouseButton(2))
		{
			Vector3 ms = Input.mousePosition - lmp;
			lmp = Input.mousePosition;

			Vector3 dir = Vector3.zero;
			dir.z = -ms.y;
			dir.x = -ms.x;

			pos += Quaternion.AngleAxis(ry, Vector3.up) * dir * zoom * .005f;
			UpdateTransform();

		}

		float z = Input.GetAxis("Mouse ScrollWheel");

		if (z != 0)
		{
			zoom -= z * zoom;
			zoom = Mathf.Clamp(zoom, 10, 2000);
			UpdateTransform();
		}


	}

	void UpdateTransform()
	{
		transform.position = pos;
		transform.rotation = Quaternion.Euler(rx, ry, 0);
		transform.Translate(0, 0, -zoom);
	}

}
