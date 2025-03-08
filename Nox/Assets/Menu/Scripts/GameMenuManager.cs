using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    string gameVersion = "0.1";

    public GameObject menu;
    public GameObject status;
    public GameObject roomNameInput;

    public Transform scrollList;
    public GameObject room;

    List<RoomInfo> createdRooms = new List<RoomInfo>();
    string roomName = "Room 1";
    Vector2 roomListScroll = Vector2.zero;

    bool joiningRoom = false;

    public GameObject player;
    public FirebaseManagerScript firebaseManagerScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnFailedToConnectPhoton.Network. StatusCode: " + cause.ToString() + " ServerAddrdess: " + PhotonNetwork.ServerAddress);
        status.GetComponent<TextMeshProUGUI>().SetText("Status: " + PhotonNetwork.NetworkClientState);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server: " + PhotonNetwork.CloudRegion + " server.");
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        status.GetComponent<TextMeshProUGUI>().SetText("Status: " + PhotonNetwork.NetworkClientState);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("We received the room list");
        if (createdRooms.Count <= 0)
        {
            createdRooms = roomList;
        }
        else
        {
            foreach (var room in roomList)
            {
                for (int i = 0; i < createdRooms.Count; i++)
                {
                    if (createdRooms[i].Name == room.Name)
                    {
                        List<RoomInfo> newList = createdRooms;

                        if (room.RemovedFromList)
                        {
                            newList.Remove(newList[i]);
                        }
                        else
                        {
                            newList[i] = room;
                        }

                        createdRooms = newList;
                    }
                }
            }
        }
        RefreshRooms();
    }

    public void UpdateUI()
    {

    }

    public void CreateRoom()
    {

        if (roomNameInput.GetComponent<TextMeshProUGUI>().text != "")
        {
            roomName = roomNameInput.GetComponent<TextMeshProUGUI>().text;
            joiningRoom = true;
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.MaxPlayers = (byte)4;

            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
            menu.SetActive(false);
        }
    }

    public void RefreshRooms()
    {
            foreach(Transform roomItem in scrollList)
            {
                Destroy(roomItem.gameObject);
            }

            for (int i = 0; i < createdRooms.Count; i++)
            {
                //playerName = playerNameInput.GetComponent<TextMeshProUGUI>().text;
                room.transform.Find("Room Name").GetComponent<TextMeshProUGUI>().text = createdRooms[i].Name;
                room.transform.Find("AmountOfPeople").GetComponent<TextMeshProUGUI>().text = createdRooms[i].PlayerCount.ToString() + "/" + createdRooms[i].MaxPlayers.ToString();
                room.transform.Find("JoinButton").GetComponent<JoinLobby>().roomName = createdRooms[i].Name;

                var instantiated = Instantiate(room);
                instantiated.transform.SetParent(scrollList);

            }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to room");

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            //Player 1
            GameObject tempPlayer = PhotonNetwork.Instantiate(player.name, new Vector3(-1f, 4f, -16f), Quaternion.identity, 0);
            firebaseManagerScript.SetNickname(tempPlayer);

            Debug.Log(tempPlayer.GetComponent<PlayerIGN>().GetNickname());
        }
        else
        {
            GameObject tempPlayer = PhotonNetwork.Instantiate(player.name, new Vector3(-1f, 4f, -16f), Quaternion.identity, 0);
            firebaseManagerScript.SetNickname(tempPlayer);

            Debug.Log(tempPlayer.GetComponent<PlayerIGN>().GetNickname());
        }
    }

    public void JoinRoom()
    {
        joiningRoom = true;
        menu.SetActive(false);
    }
}
