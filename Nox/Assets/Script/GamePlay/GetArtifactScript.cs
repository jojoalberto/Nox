using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetArtifactScript : MonoBehaviourPun
{
    [SerializeField]
    private List<ClueItemSO> keys;
    [SerializeField]
    private List<ClueItemSO> artifact;
    [SerializeField]
    private InventoryManager inventoryManager;
    public List<ClueItemSO> Keys => keys;
    public List<ClueItemSO> Artifact => artifact;

    private void Start()
    {

    }
    public ClueItemSO FindKeyByItemID(string itemID)
    {
        Debug.Log("getting key by ID");
        return keys.FirstOrDefault(item => item.itemID == itemID);
    }

    public void CallGetKeyByItemID(string itemID)
    {
        Debug.Log("calling function to get key by ID");
        photonView.RPC("AddKey", RpcTarget.AllBuffered, itemID);
    }

    public ClueItemSO FindArtifactByItemID(string itemID)
    {
        Debug.Log("getting Artifact by ID");
        return artifact.FirstOrDefault(item => item.itemID == itemID);
    }

    public void CallGetArtifactByItemID(string itemID)
    {
        Debug.Log("calling function to get Artifact by ID");
        photonView.RPC("AddArtifact", RpcTarget.AllBuffered, itemID);
    }

    public ClueItemSO GetItemByType(ClueItemType type)// get form both list
    {
        return keys.Concat(artifact).FirstOrDefault(item => item.itemType == type);
    }

    [PunRPC]
    public void AddKey(string itemID)
    {
        inventoryManager.AddKey(FindKeyByItemID(itemID));
    }
    [PunRPC]
    public void AddArtifact(string itemID)
    {
        inventoryManager.AddArtifact(FindArtifactByItemID(itemID));
    }

}
