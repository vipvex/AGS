using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RoomInfoUI : MonoBehaviour 
{

    public Text roomName;
    public Text players;
    public Button join;

    public void SetInfo (RoomInfo roomInfo)
    {
        roomName.text = roomInfo.name;
        players.text = roomInfo.playerCount + " / " + roomInfo.maxPlayers;

        join.onClick.RemoveAllListeners();
        join.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomInfo.name));
    }

}
