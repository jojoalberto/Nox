using UnityEngine;
using Photon.Pun;

public class PickableObject : MonoBehaviourPun
{
    public ClueItemSO itemData; 

    public void Interact()
    {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            photonView.RPC("CollectItem", RpcTarget.All);
        }
    }

    [PunRPC]
    void CollectItem()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemData);
            Destroy(gameObject); 
        }
    }
}
