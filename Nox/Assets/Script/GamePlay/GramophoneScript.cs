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
    private int tempCount;



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

            if (inventory != null && inventory.IsHoldingVinyl())
            {
                ClueItemSO vinyl = inventory.GetHeldVinyl();
                string vinylID = vinyl.itemID;

                // Pass the vinyl ID to the RPC so all clients process it the same way
                photonView.RPC("InteractGramaphone", RpcTarget.All, vinylID);

                inventory.DropVinyl();
            }
            else
            {
                Debug.Log("No vinyl held");
            }
        }

        
    }
    [PunRPC]
    public void InteractGramaphone(string vinylID)
    {
        hasInteract = true;
        if (string.IsNullOrEmpty(vinylID)) return;


        if (vinylID == requiredVinylID)
        {
            vinylCount++;
            onCorrectVinyl.Invoke();

            if (vinylCount >= 2)
            {
                photonView.RPC("PuzzleComplete", RpcTarget.All);
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
    [PunRPC]
    public void PuzzleComplete()
    {
        onFinishVinylQuest?.Invoke();
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
