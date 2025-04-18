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

    public void AddKey(string keyID)
    {
        if (inventory != null && !string.IsNullOrEmpty(keyID))
        {
            inventory.AddKey(keyID); // Adds key to the inventory (consider implementing this in your InventorySO)
            photonView.RPC("SyncKeys", RpcTarget.AllBuffered, keyID);  // Sync keys across all players
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

    [PunRPC]
    void SyncKeys(string keyID)
    {
        if (!string.IsNullOrEmpty(keyID) && !inventory.keyIDs.Contains(keyID))
        {
            inventory.AddKey(keyID);  // Ensure key is added across all players
        }
    }
    public bool HasKey(int keyID)
    {
        return inventory.keyIDs.Contains(keyID.ToString());  // Assuming keys are stored as strings
    }
}
