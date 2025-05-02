using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class PickableObject : MonoBehaviourPun
{
    public UnityEvent onPickup;
    public UnityEvent onPickupVinyl;
    public ClueItemSO itemData; 
    [SerializeField] protected PlayerData playerData;  
    protected InteractableObject interactableObject;
    public PlayerInventory inventory;

    protected virtual void Start()
    {
        interactableObject = GetComponent<InteractableObject>();
    }

    public virtual void Interact()
    {
        if (gameObject.layer == 10)  
        {
            photonView.RPC("CollectTrapperPickup", RpcTarget.All);
            return;
        }

        photonView.RPC("CollectItem", RpcTarget.All);
        CollectItemLocal();
    }
    protected virtual void CollectItemLocal()
    {
        if (InventoryManager.Instance == null || itemData == null) return;

        GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        if (playerObj == null)
        {
            Debug.LogWarning("Player TagObject is null!");
            return;
        }

        inventory = playerObj.GetComponentInChildren<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("PlayerInventory not found on player!");
            return;
        }
        switch (itemData.itemType)
        {
            case ClueItemType.Vinyl:
                if (inventory.IsHoldingVinyl())
                {
                    DialogueManager.Instance?.ShowDialogue("You're already holding a vinyl!");
                    return;
                }

                InventoryManager.Instance.AddVinyl(itemData);
                inventory.PickupVinyl(this.gameObject, itemData, DialogueManager.Instance, GetComponent<DialogueMessage>());
                break;
        }
    }

    [PunRPC]
    protected void CallVinylDeactivateObject()
    {
        this.gameObject.SetActive(false);
        onPickupVinyl.Invoke();
    }
    [PunRPC]
    protected virtual void CollectItem()
    {
        if (InventoryManager.Instance == null || itemData == null) return;

        interactableObject.CallForceActivation(itemData);

        switch (itemData.itemType)
        {
            case ClueItemType.Flashlight:
                if (playerData != null) playerData.setFlashlight(true);
                break;

            case ClueItemType.Candle:
                if (playerData != null && CandlePuzzle.Instance != null)
                    CandlePuzzle.Instance.AddCandle();
                break;

            case ClueItemType.Artifact:
                InventoryManager.Instance.AddArtifact(itemData);
                break;

            case ClueItemType.Key:
                InventoryManager.Instance.AddKey(itemData);
                break;
            //case ClueItemType.Vinyl:
            //    InventoryManager.Instance.AddVinyl(itemData);
            //    GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            //    Debug.Log(playerObj + " player object tag");
            //    if (playerObj == null) return;

            //    inventory = playerObj.GetComponentInChildren<PlayerInventory>();
            //    if (inventory != null)
            //    {
            //        inventory.PickupVinyl(this.gameObject,itemData, DialogueManager.Instance,gameObject.GetComponent<DialogueMessage>());
            //    }
            //    break;
            case ClueItemType.FakeVinyl:

            default: // General or any unspecified type
                InventoryManager.Instance.AddItem(itemData);
                break;
        }

        onPickup.Invoke();
    }

    [PunRPC]
    protected void CollectTrapperPickup()
    {
        gameObject.SetActive(false);
    }

}
