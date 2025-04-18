using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory/InventorySO")]
public class InventorySO : ScriptableObject
{
    public List<ClueItemSO> clueItems = new List<ClueItemSO>();
    public List<string> keyIDs = new List<string>();  // List of collected key IDs

    public void AddItem(ClueItemSO item)
    {
        if (!clueItems.Contains(item))
        {
            clueItems.Add(item);
        }
    }

    public void AddKey(string keyID)
    {
        if (!keyIDs.Contains(keyID))
        {
            keyIDs.Add(keyID);
        }
    }

    public bool HasKey(string keyID)
    {
        return keyIDs.Contains(keyID);  // Check if a particular key is in the list
    }
}
