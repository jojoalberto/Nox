using Photon.Pun;
using UnityEngine;
using TMPro;

public class InteractableObject : MonoBehaviourPunCallbacks
{
    public string dialogueMessage;
    [SerializeField]
    private DialogueUI dialogueUI;
    public ClueItemSO clueItem;

    private void Awake()
    {
        gameObject.AddComponent<PhotonTransformView>();
    }

    public void Interact()
    {
       
        if (photonView.IsMine || !PhotonNetwork.IsConnected) 
        {
            if(gameObject.tag == "Clue" || !dialogueUI.gameObject.activeSelf)
            {
                Debug.Log("player interact Clue" + gameObject);
                photonView.RPC("ShowDialogueRPC", RpcTarget.All, dialogueMessage);

                dialogueUI.gameObject.SetActive(true);

            }
            else
            {
                Debug.Log("player interact the object " + gameObject);
                photonView.RPC("OnInteract", RpcTarget.All);
            }

        }
    }

    [PunRPC]
    void OnInteract()
    {
        Debug.Log($"{gameObject.name} interacted with!");
       
    }

    [PunRPC]
    void ShowDialogueRPC(string message)
    {
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(message);
        }
    }
}
