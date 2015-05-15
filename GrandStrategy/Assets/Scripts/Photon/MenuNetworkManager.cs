using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuNetworkManager : MonoBehaviour {

    private const string roomName = "RoomName";
    private RoomInfo[] roomInfoList;


    public GameObject roomInfoPrefab;
    public Transform roomListContainer;
    private List<RoomInfoUI> rooms = new List<RoomInfoUI>();

    public GameObject playerRoomInfoPrefab;
    public Transform playerRoomListContainer;
    private List<RoomPlayerInfoUI> players = new List<RoomPlayerInfoUI>();


    public GameObject roomBrowserWindow;
    public GameObject roomInfoWindow;


    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("1");
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.player.name = "Player " + PhotonNetwork.player.ID;
        PhotonNetwork.player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "PlayerColor", Color.blue.ToString() }, { "PlayerFaction", "Humans" } });

        InitilizeRoomList();
        InitilizeRoomPlayerList();
        RefreshGameList();

        roomBrowserWindow.SetActive(true);
        roomInfoWindow.SetActive(false);
    }
 
    public void RefreshGameList()
    {
        roomInfoList = PhotonNetwork.GetRoomList();
        UpdateRoomList();
    }

    public void UpdateRoomList()
    {
        if (roomInfoList == null) return;

        Debug.Log(roomInfoList.Length);
        for (int i = 0; i < rooms.Count; i++)
        {
            if (roomInfoList.Length > i)
            {
                rooms[i].SetInfo(roomInfoList[i]);
                rooms[i].gameObject.SetActive(true);
            }
            else
            {
                rooms[i].gameObject.SetActive(false);
            }

        }
    }

    public void UpdateRoomPlayerList()
    {
        //if ( == null) return;

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

    void OnJoinedRoom()
    {
        Debug.Log("Connected to Room");

        roomBrowserWindow.SetActive(false);
        roomInfoWindow.SetActive(true);
        UpdateRoomPlayerList();
    }

    void OnLeftRoom()
    {
        roomBrowserWindow.SetActive(true);
        roomInfoWindow.SetActive(false);
        RefreshGameList();
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        UpdateRoomPlayerList();
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        UpdateRoomPlayerList();
    }

    public void HostGame()
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions() { isOpen = true, isVisible = true, maxPlayers = 8 }, null);
    }

    public void LeaveRoom()
    {
        Debug.Log("Leaving room");

        PhotonNetwork.LeaveRoom();

        roomBrowserWindow.SetActive(true);
        roomInfoWindow.SetActive(false);
    }

    [RPC]
    public void StartGame()
    {
        PhotonNetwork.LoadLevel("Main");
    }

    private void InitilizeRoomList()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject roomInfo = Instantiate(roomInfoPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            roomInfo.transform.SetParent(roomListContainer, false);

            rooms.Add(roomInfo.GetComponent<RoomInfoUI>());
        }
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

}
