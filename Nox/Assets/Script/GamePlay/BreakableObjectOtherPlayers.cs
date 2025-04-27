using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableObjectOtherPlayers : InteractableObject
{
    private DialogueMessage dialogueMessage;
    [SerializeField]
    private DialogueManager dialogueManager;
    private Protector protector;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        dialogueMessage = this.gameObject.GetComponent<DialogueMessage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Interact()
    {
        base.Interact();
        if (gameObject.layer == 14)
        {
            ShowDialogues();
        }
    }
    public void ShowDialogues()
    {
        GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        protector = playerObj.GetComponent<Protector>();
        if (protector.isProtector == false)
        {
            if (dialogueManager != null)
            {
                dialogueManager.ShowDialogue(dialogueMessage.GetDialogueMessage(0));
            }
        }
    }
}
