using Photon.Pun;
using UnityEngine;

public class PlayerInventory : MonoBehaviourPunCallbacks
{
    public InventorySO inventory;
    [SerializeField]
    private ClueItemSO heldVinyl;
    private ClueItemSO tempVinyl;
    public string requiredVinylID;

    private void Start()
    {


    }

    public bool IsHoldingVinyl() => heldVinyl != null;
    public void PickupVinyl(GameObject gameobject,ClueItemSO vinyl, DialogueManager dialogueManager, DialogueMessage vinylDialogueMessage)
    {
        if (heldVinyl != null)
        {
            Debug.Log("Cannot pick up vinyl. Already holding: " + heldVinyl.name);

            // Optional: show dialogue when already holding one
            if (dialogueManager != null)
            {
                dialogueManager.gameObject.SetActive(true);
                dialogueManager.ShowDialogue("You're already holding a vinyl.");
            }

            return;
        }

        if (vinyl.itemType == ClueItemType.Vinyl)
        {
            AddItem(vinyl);
            heldVinyl = vinyl;
            Debug.Log("Picked up: " + vinyl.name);
            PickableObject pickable = gameobject.GetComponent<PickableObject>();
            if (dialogueManager != null)
            {
                if(vinyl.itemID == requiredVinylID)
                {
                    dialogueManager.gameObject.SetActive(true);
                    dialogueManager.ShowDialogue("There is blood on this recording.");
                }
                else
                {
                    dialogueManager.gameObject.SetActive(true);
                    dialogueManager.ShowDialogue("You got a vinyl.");
                }

            }
            if (pickable != null)
            {
                pickable.photonView.RPC("CallVinylDeactivateObject", RpcTarget.AllBuffered);
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
            Debug.Log("123Picked up: "+ item);
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
