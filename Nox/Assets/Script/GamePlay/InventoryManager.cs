using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; 

    public List<ClueItemSO> items = new List<ClueItemSO>(); 
    //public Transform inventoryUIParent; 
    //public GameObject inventorySlotPrefab; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void AddItem(ClueItemSO newItem)
    {
        items.Add(newItem);
        //UpdateInventoryUI();
    }

    //void UpdateInventoryUI()
    //{
    //    foreach (Transform child in inventoryUIParent)
    //    {
    //        Destroy(child.gameObject); // Clear old UI
    //    }

    //    foreach (ClueItemSO item in items)
    //    {
    //        GameObject slot = Instantiate(inventorySlotPrefab, inventoryUIParent);
    //        slot.GetComponentInChildren<Image>().sprite = item.icon;
    //    }
    //}
}
