using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject playerGameObject;

    private void Start()
    {
        GameObject player = PhotonNetwork.Instantiate(playerGameObject.name, Vector3.zero, Quaternion.identity);
        player.GetComponent<PlayerIGN>().SetNickname(PhotonNetwork.NickName);
    }
}
