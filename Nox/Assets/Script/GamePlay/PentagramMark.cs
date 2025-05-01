using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PentagramMark : MonoBehaviourPunCallbacks
{
    public int markID; // Assign different ID to each mark
    public int correctActorNumber = -1;
    public bool isOccupied = false;
    public bool isCorrectlyOccupied = false;
    public UnityEvent onEnterCorrectMark;
    public UnityEvent onExitMark;
    private GameObject playerObj;
    private PlayerHealth playerhealth;
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView != null)
            {
                Debug.Log(playerPhotonView.Owner.ActorNumber + " enter actor number");
                photonView.RPC("SetOccupied", RpcTarget.All, playerPhotonView.Owner.ActorNumber, true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!photonView.IsMine) return;
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView != null)
            {
                Debug.Log(playerPhotonView.Owner.ActorNumber + " enter actor number");

                playerhealth = other.GetComponent<PlayerHealth>();
                if (playerhealth != null)
                {
                    bool playerState = playerhealth.isDead;
                    if (playerState)
                    {
                        Debug.Log("gathering, PlayerDead");
                    }
                    else
                    {
                        Debug.Log("gathering, PlayerAlive");
                    }
                    photonView.RPC("OnPlayersDeathRPC", RpcTarget.All, playerPhotonView.Owner.ActorNumber, true, playerState);
                }
                else
                    Debug.Log("gathering, no health script found");

                
            }
        }

        

    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            Debug.Log(playerPhotonView.Owner.ActorNumber + " exit actor number");
            if (playerPhotonView != null)
            {
                photonView.RPC("SetOccupied", RpcTarget.All, playerPhotonView.Owner.ActorNumber, false);
            }
        }
        photonView.RPC("CallExitTriggerRPC", RpcTarget.All);
    }

    [PunRPC]
    public void CallExitTriggerRPC()
    {
        onExitMark.Invoke();
    }

    [PunRPC]
    public void OnPlayersDeathRPC(int actorNumber, bool occupied)
    {
        isOccupied = occupied;
        isCorrectlyOccupied = (occupied && actorNumber == correctActorNumber);
        if (isCorrectlyOccupied)
        {
            GameObject tempPlayerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            playerObj = tempPlayerObj;
            playerhealth = playerObj.GetComponentInChildren<PlayerHealth>();
            if (playerhealth.isDead)
            {
                onExitMark.Invoke();
            }
        }
    }

    [PunRPC]
    private void SetOccupied(int actorNumber, bool occupied)
    {
        isOccupied = occupied;
        isCorrectlyOccupied = (occupied && actorNumber == correctActorNumber);
        if (isCorrectlyOccupied)
        {
            onEnterCorrectMark.Invoke();
        }
        PentagramPuzzleManager.Instance.CheckPuzzleStatus();
    }
}
