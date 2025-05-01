using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class AllPlayerEnterRoom : MonoBehaviourPunCallbacks
{
    
    private HashSet<int> playersInTrigger = new HashSet<int>();
    public UnityEvent onAllplayerEntered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView otherPhotonView = other.GetComponent<PhotonView>();
            if (otherPhotonView != null && otherPhotonView.IsMine)
            {
                photonView.RPC("PlayerEnteredTrigger", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView otherPhotonView = other.GetComponent<PhotonView>();
            if (otherPhotonView != null && otherPhotonView.IsMine)
            {
                photonView.RPC("PlayerExitedTrigger", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }

    [PunRPC]
    void PlayerEnteredTrigger(int actorNumber)
    {
        if (!playersInTrigger.Contains(actorNumber))
            playersInTrigger.Add(actorNumber);

        CheckIfAllPlayerEnter();
    }
    [PunRPC]
    void PlayerExitedTrigger(int actorNumber)
    {
        playersInTrigger.Remove(actorNumber);
    }

    void CheckIfAllPlayerEnter()
    {
        if (PhotonNetwork.IsMasterClient && playersInTrigger.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("InvokeAllplayerEnter", RpcTarget.All);
        }
    }

    [PunRPC]
    void InvokeAllplayerEnter()
    {
        onAllplayerEntered.Invoke();
    }
}
