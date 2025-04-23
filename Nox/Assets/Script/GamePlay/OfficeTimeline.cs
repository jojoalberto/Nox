using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class OfficeTimeline : MonoBehaviourPunCallbacks
{
    public PlayableDirector timelineDirector;
    private HashSet<int> playersInTrigger = new HashSet<int>();
    private bool hasPlayed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed && other.CompareTag("Player"))
        {
            PhotonView otherPhotonView = other.GetComponent<PhotonView>();
            if (otherPhotonView != null && otherPhotonView.IsMine)
            {
                photonView.RPC("PlayerEnteredTrigger", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                hasPlayed = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!hasPlayed && other.CompareTag("Player"))
        {
            PhotonView otherPhotonView = other.GetComponent<PhotonView>();
            if (otherPhotonView != null && otherPhotonView.IsMine)
            {
                photonView.RPC("PlayerExitedTrigger", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }

    [PunRPC]
    void PlayerEnteredTrigger(int actorNumber)
    {
        if (!playersInTrigger.Contains(actorNumber))
            playersInTrigger.Add(actorNumber);

        TryPlayCutscene();
    }
    void PlayerExitedTrigger(int actorNumber)
    {
        playersInTrigger.Remove(actorNumber);
    }

    void TryPlayCutscene()
    {
        if (PhotonNetwork.IsMasterClient && playersInTrigger.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("PlayCutscene", RpcTarget.All);
            hasPlayed = true;
        }
    }

    [PunRPC]
    void PlayCutscene()
    {
        timelineDirector?.Play();
    }
}
