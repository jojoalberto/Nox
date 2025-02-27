using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected");
    }
    public void PhotonConnect(string name)
    {
        PhotonNetwork.NickName = name;
        PhotonNetwork.ConnectUsingSettings();
    }

}
