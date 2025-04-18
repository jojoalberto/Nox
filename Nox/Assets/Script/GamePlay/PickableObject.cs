using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class PickableObject : MonoBehaviourPun
{
    public UnityEvent onPickup;
    public ClueItemSO itemData;  // The item data associated with this object
    [SerializeField] protected PlayerData playerData;  // Reference to PlayerData for flashlight, etc.
    protected InteractableObject interactableObject;

    protected virtual void Start()
    {
        interactableObject = GetComponent<InteractableObject>();
    }

    public virtual void Interact()
    {
        // If object is on a specific layer (like Trapper), collect it differently
        if (gameObject.layer == 10)  // Check for Trapper-specific layer
        {
            CollectTrapperPickup();
            return;
        }

        // Otherwise, call the standard CollectItem RPC
        photonView.RPC("CollectItem", RpcTarget.All);
    }

    [PunRPC]
    protected virtual void CollectItem()
    {
        // Check for null values to prevent errors
        if (InventoryManager.Instance == null || itemData == null) return;

        // Trigger any actions associated with the item interaction
        interactableObject.CallForceActivation(itemData);

        // Switch based on item type to handle different cases
        switch (itemData.itemType)
        {
            case ClueItemType.Flashlight:
                if (playerData != null) playerData.setFlashlight(true);  // Give flashlight to player
                break;

            case ClueItemType.Candle:
                if (playerData != null && CandlePuzzle.Instance != null)
                    CandlePuzzle.Instance.AddCandle();  // Add candle to puzzle if needed
                break;

            case ClueItemType.Artifact:
                InventoryManager.Instance.AddArtifact(itemData);  // Add artifact to inventory
                break;

            case ClueItemType.Key:
                InventoryManager.Instance.AddKey(itemData);
                HandleKeyPickup();  // Special handling for keys
                break;

            default: // General or any unspecified type
                InventoryManager.Instance.AddItem(itemData);  // Add general item
                break;
        }

        // Invoke any additional behavior like visual effects or sound
        onPickup.Invoke();
    }

    // Handle key-specific pickups (adding to player's inventory)
    protected void HandleKeyPickup()
    {
        // Retrieve the player's GameObject
        GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        if (playerObj == null) return;  // Ensure player object exists

        PlayerInventory inventory = playerObj.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            // Add the key to the player's inventory based on its keyID
            inventory.AddKey(itemData.keyID);
        }
    }

    // Specific collection logic for trapper objects
    protected void CollectTrapperPickup()
    {
        // Destroy the object (may not always be desirable, can be tweaked)
        Destroy(gameObject);
    }
}
