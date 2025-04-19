using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class GramophoneScript : InteractableObject
{
    public UnityEvent onInteract;
    public UnityEvent onCorrectVinyl;
    public UnityEvent onWrongVinyl;
    public string requiredVinylID;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public override void Interact()
    {
        base.Interact();
        InteractGramaphone();
        
    }

    public void InteractGramaphone()
    {
        GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        if (playerObj == null) return;
        PlayerInventory inventory = playerObj.GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("No PlayerInventory found!");
            return;
        }
        Debug.Log(inventory.IsHoldingVinyl() + "player inventory has");
        if (inventory.IsHoldingVinyl())
        {
            ClueItemSO vinyl = inventory.GetHeldVinyl();
            if (vinyl.itemID == requiredVinylID) // Match by ID
            {
                Debug.Log("Correct vinyl played!");
                onCorrectVinyl.Invoke();

                // Optionally clear the vinyl
                inventory.DropVinyl();
            }
            else
            {
                Debug.Log("Wrong vinyl.");
                onWrongVinyl.Invoke();
                inventory.DropVinyl();
            }
        }
        else
        {
            Debug.Log("Player is not holding any vinyl.");
            onInteract.Invoke();
        }
    }
}
