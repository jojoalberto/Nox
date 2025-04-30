using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class GramophoneScript : InteractableObject
{
    public UnityEvent onCorrectVinyl;
    public UnityEvent onFinishVinylQuest;
    public UnityEvent onWrongVinyl;
    public string requiredVinylID;
    private int vinylCount = 0;
    private bool hasInteract =false;
    private PlayerInventory inventory;
    private GameObject playerObj;



    private 

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
        if (!hasInteract)
        {
            base.Interact();
            hasInteract = true;
        }
        else
        {
            playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            inventory = playerObj.GetComponent<PlayerInventory>();
            photonView.RPC("InteractGramaphone", RpcTarget.All);
            inventory.DropVinyl();
        }

        
    }
    [PunRPC]
    public void InteractGramaphone()
    {
        //GameObject playerObj = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        if (playerObj == null) return;   

        if (inventory == null)
        {
            Debug.LogWarning("No PlayerInventory found!");
            return;
        }
        Debug.Log(inventory.IsHoldingVinyl() + "player inventory has");
        if (inventory.IsHoldingVinyl())
        {
            hasInteract = true;
            ClueItemSO vinyl = inventory.GetHeldVinyl();
            if (vinyl.itemID == requiredVinylID) // Match by ID
            {
                vinylCount++;
                onCorrectVinyl.Invoke();
                if (vinylCount >= 2)
                {
                    Debug.Log("Correct vinyl played!");
                    if (onFinishVinylQuest != null)
                        onFinishVinylQuest.Invoke();
                }
                else
                {
                    Debug.Log("Correct vinyl played!");
                }

            }
            else
            {
                Debug.Log("Wrong vinyl.");
                FakeVinyl();
            }
        }
    }

    public void FakeVinyl()
    {
        float r = Random.Range(1, 10);

        if(r < 3)
        {
            Debug.Log("Attempting Sound Alert");
            if(onWrongVinyl != null)
                onWrongVinyl.Invoke();
        }
        
    }
}
