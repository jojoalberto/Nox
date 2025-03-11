using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject playerGameObject;
    public FirebaseManagerScript firebaseManagerScript;

    private void Start()
    {
        GameObject player = PhotonNetwork.Instantiate(playerGameObject.name, Vector3.zero, Quaternion.identity);
        //firebaseManagerScript.SetNickname(player);
    }
}
