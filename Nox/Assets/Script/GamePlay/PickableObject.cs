using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class PickableObject : MonoBehaviourPun
{
    public UnityEvent onPickup;
    public ClueItemSO itemData; 
    [SerializeField] protected PlayerData playerData;  
    protected InteractableObject interactableObject;

    protected virtual void Start()
    {
        interactableObject = GetComponent<InteractableObject>();
    }

    public virtual void Interact()
    {
        if (gameObject.layer == 10)  
        {
            CollectTrapperPickup();
            return;
        }

        photonView.RPC("CollectItem", RpcTarget.All);
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
            case ClueItemType.Vinyl:
                InventoryManager.Instance.AddVinyl(itemData);
                GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
                Debug.Log(playerObj + " player object tag");
                if (playerObj == null) return;

                PlayerInventory inventory = playerObj.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.PickupVinyl(itemData, DialogueManager.Instance,gameObject.GetComponent<DialogueMessage>());
                }
                break;
            case ClueItemType.FakeVinyl:

            default: // General or any unspecified type
                InventoryManager.Instance.AddItem(itemData);
                break;
        }

        onPickup.Invoke();
    }

    protected void CollectTrapperPickup()
    {
        Destroy(gameObject);
    }
}
