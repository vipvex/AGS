using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomPlayerList : MonoBehaviour 
{

    public GameObject playerRoomInfoPrefab;
    public Transform playerRoomListContainer;
    private List<RoomPlayerInfoUI> players = new List<RoomPlayerInfoUI>();

	void Start () 
    {
        InitilizeRoomPlayerList();
	}

    void OnJoinedRoom()
    {
        Debug.Log("Connected to Room");

        gameObject.SetActive(true);
        UpdateRoomPlayerList();
    }

    void OnLeftRoom()
    {
        Debug.Log("Leaft room");
        gameObject.SetActive(false);
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        UpdateRoomPlayerList();
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        UpdateRoomPlayerList();
    }

    private void InitilizeRoomPlayerList()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject playerInfo = Instantiate(playerRoomInfoPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            playerInfo.transform.SetParent(playerRoomListContainer, false);

            players.Add(playerInfo.GetComponent<RoomPlayerInfoUI>());
        }
    }

    public void UpdateRoomPlayerList()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (PhotonNetwork.playerList.Length > i)
            {
                players[i].SetInfo(PhotonNetwork.playerList[i]);
                players[i].gameObject.SetActive(true);
            }
            else
            {
                players[i].gameObject.SetActive(false);
            }

        }
    }
}
