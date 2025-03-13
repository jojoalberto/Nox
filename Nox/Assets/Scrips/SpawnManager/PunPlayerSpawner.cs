using Photon.Pun;
using UnityEngine;

public class PunPlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject player;

    public GameObject playerSpawnsLocation;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PhotonNetwork.Instantiate(player.name, playerSpawnsLocation.transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber - 1).transform.position + new Vector3(-0.0875f, 2.979f, -8.35f), Quaternion.identity, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
