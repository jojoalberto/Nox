using UnityEngine;
using Photon.Pun;
using System.Linq;

public class DoorInteraction : InteractableObject
{
    public ClueItemSO keyID;
    public Animator doorAnimator;
    private bool isOpen = false;
    private DialogueMessage dialogueMessages;

    public override void Interact()
    {
        Debug.Log("door interact");
        if (isOpen) return;

        if (InventoryManager.Instance != null && InventoryManager.Instance.keys != null && InventoryManager.Instance.keys.Contains(keyID))
        {
            Debug.Log("door is opening");
            photonView.RPC("OpenDoor", RpcTarget.All);
        }
        else
        {
            dialogueMessages = this.gameObject.GetComponent<DialogueMessage>();
            photonView.RPC("ShowDialogueRPC", RpcTarget.All, dialogueMessages.GetDialogueMessage(0));
        }
    }

    [PunRPC]
    void OpenDoor()
    {
        isOpen = true;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        if (doorAnimator != null)
        {
            doorAnimator.Play("OpenDoor");
        }
    }
}
