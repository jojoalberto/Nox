using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using TMPro;
using UnityEngine;

public class JoinLobby : MonoBehaviour
{
    public string roomName = "";
    public GameObject playerNameInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void JoinRoom()
    {
        
        //playerNameInput = GameObject.Find("PlayerNameInput").gameObject;
        PhotonNetwork.NickName = "HELLO";

        PhotonNetwork.JoinRoom(roomName);

        GameObject.Find("MenuManager").GetComponent<GameManager>().JoinRoom();
    }
}
