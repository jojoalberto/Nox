using Photon.Pun;
using UnityEngine;

public class InteractableObject : MonoBehaviourPunCallbacks
{
    public void Interact()
    {
        Debug.Log("player interact12 " + gameObject);
        if (photonView.IsMine || !PhotonNetwork.IsConnected) 
        {
            Debug.Log("player interact " + gameObject);
            photonView.RPC("OnInteract", RpcTarget.All);
        }
    }

    [PunRPC]
    void OnInteract()
    {
        Debug.Log($"{gameObject.name} interacted with!");
       
    }
}
