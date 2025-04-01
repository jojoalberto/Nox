using UnityEngine;
using Photon.Pun;

public class PickableObject : MonoBehaviourPun
{
    public ClueItemSO itemData;
    [SerializeField]
    private PlayerData playerData;
    private InteractableObject interactableObject;

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
        //if (photonView.IsMine || !PhotonNetwork.IsConnected)
        //{
        Debug.Log("collectiong Item");
        photonView.RPC("CollectItem", RpcTarget.All);
        //}
    }

    [PunRPC]
    void CollectItem()
    {
        Debug.Log("collecting Item before if statement ");
        if (InventoryManager.Instance != null)
        {
            interactableObject.CallForceActivation();
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
            InventoryManager.Instance.AddItem(itemData);
            //Destroy(gameObject); 
        }
    }

    void CollectTrapperPickup()
    {
        Destroy(gameObject);
    }
}
