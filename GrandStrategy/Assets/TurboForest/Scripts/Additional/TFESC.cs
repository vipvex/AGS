using UnityEngine;
using System.Collections;

public class TFESC : MonoBehaviour 
{
	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
}
