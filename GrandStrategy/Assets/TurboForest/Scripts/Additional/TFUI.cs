using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TFUI : MonoBehaviour
{

	public GameObject orbitUI;
	public GameObject fpsUI;
	public GameObject mainMenuUI;
	public GameObject loadingUI;
	public Text FPS;
	public GameObject mainMenuButton;

	int frames = 0;

	void Start ()
	{
		loadingUI.SetActive(false);
		mainMenuButton.SetActive(true);

		switch (Application.loadedLevelName)
		{
			case "tf_main_menu":
				orbitUI.SetActive(false);
				fpsUI.SetActive(false);
				mainMenuUI.SetActive(true);
				mainMenuButton.SetActive(false);
				break;
			case "1_tf_atlas_trees":
			case "4_tf_dynamic_trees":
			case "5_tf_terrain_trees":
				orbitUI.SetActive(true);
				fpsUI.SetActive(false);
				mainMenuUI.SetActive(false);
				break;
			case "2_tf_cylindrical_trees":
			case "3_tf_mesh_trees":
				orbitUI.SetActive(false);
				fpsUI.SetActive(true);
				mainMenuUI.SetActive(false);
				break;
		}

		StartCoroutine(UpdateFPS());

		if (nativeScreenHeight == -1)
		{
			nativeScreenWidth = Screen.width;
			nativeScreenHeight = Screen.height;
		}

	}

	static int nativeScreenWidth = -1;
	static int nativeScreenHeight = -1;

	public void SetResolution(int res)
	{
		
		switch (res)
		{
			case 0:
				Screen.SetResolution(nativeScreenWidth, nativeScreenHeight, Screen.fullScreen);
				break;
			case 1:
				Screen.SetResolution(nativeScreenWidth / 2, nativeScreenHeight / 2, Screen.fullScreen);
				break;
			case 2:
				Screen.SetResolution(nativeScreenWidth / 4, nativeScreenHeight / 4, Screen.fullScreen);
				break;

		}
	}

	void InitForDevice()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			var tfs = Object.FindObjectsOfType<TF>();

			foreach (var tf in tfs)
			{
				var renderers = tf.GetComponentsInChildren<Renderer>();

				foreach (var r in renderers)
				{
					r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				}
			}

		}
	}

	IEnumerator UpdateFPS()
	{
		var delay = new WaitForSeconds(0.5f);

		while (true)
		{
			yield return delay;

			FPS.text = "FPS: " + ((int)(frames * 2)).ToString();
			frames = 0;
		}
	}

	void Update ()
	{
		frames++;
	}

	public void LoadScene(string sceneName)
	{
		orbitUI.SetActive(false);
		fpsUI.SetActive(false);
		mainMenuUI.SetActive(false);		
		loadingUI.SetActive(true);

		StartCoroutine(LoadNextFrame(sceneName));
	}

	IEnumerator LoadNextFrame(string sceneName)
	{
		yield return null;
		Application.LoadLevel(sceneName);
	}

	public void Exit()
	{
		Application.Quit();
	}

}
