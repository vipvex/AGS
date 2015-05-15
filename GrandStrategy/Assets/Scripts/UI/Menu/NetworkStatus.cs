using UnityEngine;
using System.Collections;

public class NetworkStatus : MonoBehaviour {

	// Use this for initialization
	void OnGUI () {
        if (PhotonNetwork.connected)
        {
            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        }
	}
}
