using UnityEngine;
using System.Collections;

public class TFFPS : MonoBehaviour 
{

	float frames = 0;
	string fps = "---";
	
	void Start () 
	{
		//StartCoroutine(UpdateFPS());
	}

	IEnumerator UpdateFPS()
	{
		var delay = new WaitForSeconds(0.5f);
		
		while (true)
		{
			yield return delay;
			
			fps = "FPS: " + ((int)(frames * 2)).ToString();
			frames = 0;
		}
	}

	void OnGUI()
	{
		Rect r = new Rect(10, 10, 200, 30);
		Rect sr = new Rect(11, 11, 200, 30);
		GUI.color = Color.black;
		GUI.Label(sr, fps);
		GUI.color = Color.white;
		GUI.Label(r, fps);
	}
	
	void OnPreRender()
	{
		frames++;
		dt += Time.deltaTime;
		if (dt > 1.0f / updateRate)
		{
			fps = "" + ((int)(frames / dt));
			frames = 0;
			dt -= 1.0f / updateRate;
		}
	}

	float dt = 0;
	float updateRate = 2;
	float deltaTime = 0;
	
/*
	void Update()
	{
		deltaTime += Time.deltaTime;
		deltaTime /= 2.0f;
		fps = fps = "" + ((int)(1.0f / deltaTime));
	}
*/
}
