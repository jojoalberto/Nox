using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class InteractableObject : MonoBehaviourPunCallbacks
{
    private DialogueMessage dialogueMessage;
    [SerializeField]
    private DialogueManager dialogueUI;
    public List<GameObject> disableObject;
    public List<GameObject> enableObject;
    //public PhotonView photonView;

    private void Awake()
    {

        dialogueMessage = this.gameObject.GetComponent<DialogueMessage>();
        gameObject.AddComponent<PhotonTransformView>();
        if (gameObject.TryGetComponent(out PhotonView cPhotonView))
        {
            Debug.Log("PhotonView found on " + gameObject.name);
        }
        else
        {
            //gameObject.AddComponent<PhotonView>();
            //photonView = gameObject.GetComponent<PhotonView>();
        }

    }

    public void Interact()
    {
       
        //if (photonView.IsMine || !PhotonNetwork.IsConnected) 
        //{
            if(gameObject.tag == "Clue" || !dialogueUI.gameObject.activeSelf)
            {
                Debug.Log("player interact Clue" + gameObject);
                photonView.RPC("ShowDialogueRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(0));

                dialogueUI.gameObject.SetActive(true);

            }
            else
            {
                Debug.Log("player interact the object " + gameObject);
                photonView.RPC("ShowDialogueRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(0));
                //photonView.RPC("OnInteract", RpcTarget.All);
            }

        //}
    }

    [PunRPC]
    void OnInteract()
    {
        Debug.Log($"{gameObject.name} interacted with!");
        for (int i = 0; i < enableObject.Count; i++)
        {
            if (enableObject[i] != null && enableObject[i].activeSelf)
            {
                enableObject[i].SetActive(true);
            }
        }
        for (int i = 0; i < disableObject.Count; i++)
        {
            if (disableObject[i]!=null && !enableObject[i].activeSelf)
            {
                disableObject[i].SetActive(false);                
            }
        }

    }

    public void CallForceActivation()
    {

        photonView.RPC("ShowDialogueRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(0));

    }

    [PunRPC]
    void ShowDialogueRPC(string message)
    {
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(message);
            for (int i = 0; i < disableObject.Count; i++)
            {
                if (disableObject[i] != null)
                {
                    disableObject[i].SetActive(false);
                }
            }
            for (int i = 0; i < enableObject.Count; i++)
            {
                if (enableObject[i] != null)
                {
                    enableObject[i].SetActive(true);
                }
            }
        }
    }

    [PunRPC]
    public void ForceDisableAndEnableObjectsRPC(string message)
    {
        if (dialogueUI != null)
        {
            dialogueUI.ShowDialogue(message);
            for (int i = 0; i < disableObject.Count; i++)
            {
                if (disableObject[i] != null)
                {
                    disableObject[i].SetActive(false);
                }
            }
            for (int i = 0; i < enableObject.Count; i++)
            {
                if (enableObject[i] != null)
                {
                    enableObject[i].SetActive(true);
                }
            }
        }
    }
}
