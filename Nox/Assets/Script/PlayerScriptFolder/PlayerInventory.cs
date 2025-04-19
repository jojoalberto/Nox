using Photon.Pun;
using UnityEngine;

public class PlayerInventory : MonoBehaviourPunCallbacks
{
    public InventorySO inventory;
    [SerializeField]
    private ClueItemSO heldVinyl;

    private void Start()
    {
        PhotonNetwork.LocalPlayer.TagObject = this.gameObject;
    }

    public bool IsHoldingVinyl() => heldVinyl != null;
    public void PickupVinyl(ClueItemSO vinyl, DialogueManager dialogueManager, DialogueMessage vinylDialogueMessage)
    {
        if (heldVinyl == null && vinyl.itemType == ClueItemType.Vinyl) // or ClueItemType.Vinyl
        {
            heldVinyl = vinyl;
            Debug.Log("Picked up: " + vinyl.name);
        }
        else
        {
            dialogueManager.gameObject.SetActive(true);
            if (dialogueManager != null)
            {
                dialogueManager.ShowDialogue(vinylDialogueMessage.GetDialogueMessage(1));
            }

        }
    }
    public ClueItemSO DropVinyl()
    {
        ClueItemSO temp = heldVinyl;
        heldVinyl = null;
        return temp;
    }

    public ClueItemSO GetHeldVinyl() => heldVinyl;
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
