using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class PickableObject : MonoBehaviourPun
{
    public UnityEvent onPickup;
    public ClueItemSO itemData;
    [SerializeField]
    private PlayerData playerData;
    private InteractableObject interactableObject;
    [SerializeField]
    private bool isArtifact;

    private void Start()
    {
        interactableObject = GetComponent<InteractableObject>();
    }

    public void Interact()
    {
        Debug.Log("GameObject in layer :" + gameObject.layer);
        if (gameObject.layer == 10)
        {
            CollectTrapperPickup();
        }
        Debug.Log("collectiong Item");
        photonView.RPC("CollectItem", RpcTarget.All);
    }

    [PunRPC]
    void CollectItem()
    {
        Debug.Log("collecting Item before if statement ");
        if (InventoryManager.Instance != null)
        {
            interactableObject.CallForceActivation(itemData);
            Debug.Log("collecting Item ");
            if(playerData != null && itemData.isFlashlight)
            {
                playerData.setFlashlight(true);
            }
            if (playerData != null && itemData.isCandle && CandlePuzzle.Instance != null)
            {
                Debug.Log("adding candle ");
                CandlePuzzle.Instance.AddCandle();
            }
            if (isArtifact)
            {
                InventoryManager.Instance.AddArtifact(itemData);
            }
            else
            {
                InventoryManager.Instance.AddItem(itemData);
            }
                
            onPickup.Invoke();
            //Destroy(gameObject); 
        }
    }

    void CollectTrapperPickup()
    {
        Destroy(gameObject);
    }
}
