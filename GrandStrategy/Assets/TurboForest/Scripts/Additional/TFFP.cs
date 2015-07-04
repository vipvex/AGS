using UnityEngine;
using System.Collections;

public class TFFP : MonoBehaviour
{
	Camera cam;
	CharacterController body;

	float rx, ry;

	Vector3 lmp;

	void Start ()
	{
		cam = Camera.main;

		var orbit = cam.GetComponent<TFOrbitCamera>();
		if (orbit)
			Destroy(orbit);

		body = GetComponent<CharacterController>();
		lmp = Input.mousePosition;
	}

	float lastTouchTime = 0;
	bool running = false;
	bool lastFrameTouched = false;

	void Update ()
	{
		Vector3 move = Vector3.zero;

		if (Input.GetKey(KeyCode.W)) move.z = 1;
		if (Input.GetKey(KeyCode.S)) move.z = -1;
		if (Input.GetKey(KeyCode.A)) move.x = -1;
		if (Input.GetKey(KeyCode.D)) move.x = 1;

		move *= 4;

		if (Input.GetKey(KeyCode.LeftShift))
			move *= 5;

		if (Input.GetMouseButtonDown(0))
		{
			if (lastFrameTouched || (Time.time - lastTouchTime < .5f))
				running = true;

			lastFrameTouched = true;
			lastTouchTime = Time.time;
			lmp = Input.mousePosition;
		}
		else
		{
			lastFrameTouched = false;
		}

		if (Input.GetMouseButton(0))
		{
			var ms = lmp - Input.mousePosition;

			rx -= ms.y * .2f;
			ry -= ms.x * .2f;

			if (running)
				move.z = 20;

		}
		else
		{
			running = false;
		}

		var rotY = Quaternion.AngleAxis(ry, Vector3.up);
		var rotX = Quaternion.AngleAxis(rx, Vector3.left);

		cam.transform.rotation = rotY * rotX;

		move.y = -100;

		body.Move(rotY * move * Time.deltaTime);

		cam.transform.position = transform.position + new Vector3(0, .8f, 0);

		lmp = Input.mousePosition;

	}
}
