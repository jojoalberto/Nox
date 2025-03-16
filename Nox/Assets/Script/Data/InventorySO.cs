using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventory", menuName = "Inventory/Clue Inventory")]
public class InventorySO : ScriptableObject
{
    public List<ClueItemSO> clueItems = new List<ClueItemSO>();

    public void AddItem(ClueItemSO newItem)
    {
        if (!clueItems.Contains(newItem))
        {
            clueItems.Add(newItem);
        }   
    }
}