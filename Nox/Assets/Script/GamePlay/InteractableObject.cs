using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviourPunCallbacks
{
    private ClueItemSO itemData;
    private ClueItemSO tempItemData;
    private DialogueMessage dialogueMessage;
    [SerializeField]
    private DialogueManager dialogueManager;
    public List<GameObject> disableObject;
    public List<GameObject> enableObject;
    public UnityEvent onInteract;
    private Protector protector;
    //public PhotonView photonView;

    private void Awake()
    {

        dialogueMessage = this.gameObject.GetComponent<DialogueMessage>();
        if (!gameObject.TryGetComponent(out PhotonTransformView transformView))
        {
            gameObject.AddComponent<PhotonTransformView>();
        }
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

    public DialogueManager GetDialogueManager()
    {
        return dialogueManager;
    }

    public virtual void Interact()
    {
        Debug.Log("on layer is " + gameObject.layer);
        if (gameObject.tag == "Clue" || !dialogueManager.gameObject.activeSelf)
        {
            Debug.Log("player interact Clue" + gameObject);
            photonView.RPC("ShowItemDialogueRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(0),dialogueMessage.GetImage());
            dialogueManager.gameObject.SetActive(true);

        }
        else if(gameObject.layer == LayerMask.NameToLayer("Destroyable"))
        {
            Debug.Log("layer is " + gameObject.layer);
            ShowDialogue();
        }
        else
        {
            Debug.Log("player interact the object " + gameObject);
            photonView.RPC("ShowDialogueRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(0));

        }
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

        onInteract.Invoke();
    }

    public void CallForceActivation(ClueItemSO data)
    {
        itemData = data;
        photonView.RPC("ShowItemDialogueRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(0), itemData.itemID);
    }
    [PunRPC]
    public void ShowItemDialogueRPC(string message,string data)
    {
        if (dialogueManager != null)
        {
            dialogueManager.ShowDialogue(message);
            if(data != null)
            {
                Debug.Log($"data is : "+ data);
                dialogueManager.ShowImage(data);
            }
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
    public void ShowDialogueRPC(string message)
    {
        if (dialogueManager != null)
        {
            dialogueManager.ShowDialogue(message);
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
        onInteract.Invoke();
    }

    [PunRPC]
    public void ForceDisableAndEnableObjectsRPC(string message)
    {
        if (dialogueManager != null)
        {
            dialogueManager.ShowDialogue(message);
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
    public void ShowDialogue()
    {
        GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        protector = playerObj.GetComponentInChildren<Protector>();
        Debug.Log("photon local player is: " + protector + protector.GetComponentInChildren<Protector>().isProtector);
        if (protector.isProtector == false)
        {
            if (dialogueManager != null)
            {
                dialogueManager.ShowDialogue(dialogueMessage.GetDialogueMessage(0));
            }
        }
    }
}
