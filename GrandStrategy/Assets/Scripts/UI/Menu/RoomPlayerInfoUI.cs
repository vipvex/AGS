using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RoomPlayerInfoUI : MonoBehaviour 
{

    public Text playerName;
    public Text faction;
    public Button kick;

    public void SetInfo(PhotonPlayer player)
    {
        playerName.text = player.name;
        faction.text = "Humans";

        if (PhotonNetwork.player.isMasterClient && player != PhotonNetwork.player)
        {
            kick.gameObject.SetActive(true);
            kick.onClick.RemoveAllListeners();
            kick.onClick.AddListener(() => PhotonNetwork.CloseConnection(player));
        }

        if (player == PhotonNetwork.player)
        {
            gameObject.GetComponent<Image>().color = Color.green;
        }
    }
}
