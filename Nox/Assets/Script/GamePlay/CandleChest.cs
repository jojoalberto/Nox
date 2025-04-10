using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class CandleChest : MonoBehaviourPun
{
    public static CandleChest Instance;
    private bool isUnlocked = false;
    [SerializeField]
    private DialogueManager dialogueUI;
    private DialogueMessage dialogueMessage;
    private void Awake()
    {
        Instance = this;
    }

    [PunRPC]
    public void UnlockChestRPC(string message)
    {
        isUnlocked = true;
        dialogueUI.ShowDialogue(message);
        Debug.Log("Chest Unlocked!");
        // Add chest opening animation or sound
    }

    public void UnlockChest()
    {
        if (!isUnlocked)
        {
            photonView.RPC("UnlockChestRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(1));
            dialogueUI.gameObject.SetActive(true);
        }
    }



    [PunRPC]
    void ShowChestDialogueRPC(string message)
    {
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(message);
        }
    }
}
