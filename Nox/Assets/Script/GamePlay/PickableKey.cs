using UnityEngine;
using Photon.Pun;

public class PickableKey : PickableObject
{
    protected override void CollectItem()
    {
        base.CollectItem(); // Run base logic first

        if (itemData != null)
        {
            GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            if (playerObj != null)
            {
                PlayerInventory inventory = playerObj.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.AddKey(itemData.keyID);
                }
            }
        }
    }
}
