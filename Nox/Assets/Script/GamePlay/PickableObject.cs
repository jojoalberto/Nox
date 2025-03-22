using UnityEngine;
using Photon.Pun;

public class PickableObject : MonoBehaviourPun
{
    public ClueItemSO itemData;
    [SerializeField]
    private PlayerData playerData;

    public void Interact()
    {
        Debug.Log("GameObject in layer :" + gameObject.layer);
        if (gameObject.layer == 10)
        {
            CollectTrapperPickup();
        }
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
            if(playerData != null && itemData.isFlashlight)
            {
                playerData.setFlashlight(true);
            }
            InventoryManager.Instance.AddItem(itemData);
            Destroy(gameObject); 
        }
    }

    void CollectTrapperPickup()
    {
        Destroy(gameObject);
    }
}
