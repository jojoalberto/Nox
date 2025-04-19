using Photon.Pun;
using UnityEngine;

public class PlayerInventory : MonoBehaviourPunCallbacks
{
    public InventorySO inventory;

    public void AddItem(ClueItemSO item)
    {
        if (!inventory.clueItems.Contains(item))
        {
            inventory.AddItem(item);
            photonView.RPC("SyncInventory", RpcTarget.AllBuffered, item.itemName);
        }
    }

    [PunRPC]
    void SyncInventory(string itemName)
    {
        ClueItemSO item = Resources.Load<ClueItemSO>("ClueItems/" + itemName);
        if (item != null && !inventory.clueItems.Contains(item))
        {
            inventory.AddItem(item);
        }
    }



}
