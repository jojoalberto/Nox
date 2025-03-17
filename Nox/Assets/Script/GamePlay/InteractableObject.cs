using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class InteractableObject : MonoBehaviourPunCallbacks
{
    public string dialogueMessage;
    [SerializeField]
    private DialogueUI dialogueUI;
    public List<GameObject> disableObject;
    //public PhotonView photonView;

    private void Awake()
    {
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
                photonView.RPC("ShowDialogueRPC", RpcTarget.All, dialogueMessage);
                //photonView.RPC("OnInteract", RpcTarget.All);
            }

        }
    }

    [PunRPC]
    void OnInteract()
    {
        Debug.Log($"{gameObject.name} interacted with!");
        for (int i = 0; i < disableObject.Count; i++)
        {
            if (disableObject[i]!=null)
            {
                disableObject[i].SetActive(false);
            }
        }
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
        }
    }
}
