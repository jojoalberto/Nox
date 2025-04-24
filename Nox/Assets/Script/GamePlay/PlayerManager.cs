using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager Instance;
    public List<GameObject> loadedPlayers = new List<GameObject>();
    private bool allPlayerHasLoaded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            {
                Instance = this;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        StartPlayerStoring();
    }

    public void StartPlayerStoring()
    {
        if (loadedPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount && !allPlayerHasLoaded)
        {
            StartCoroutine(FinishLoad());
        }
    }
    public void PlayerFinishedLoading()
    {
        photonView.RPC("RPC_PlayerLoaded", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }
    [PunRPC]
    void RPC_PlayerLoaded(GameObject player)
    {
        if (!loadedPlayers.Contains(player))
        {
            loadedPlayers.Add(player);
        }
        if (loadedPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("All players have loaded!");
            allPlayerHasLoaded = true;
        }
    }
    IEnumerator FinishLoad()
    {
        yield return new WaitForSeconds(2); // replace with actual load condition
        PlayerFinishedLoading();
    }

}
