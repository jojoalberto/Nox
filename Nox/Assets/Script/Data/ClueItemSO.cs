using UnityEngine;

[CreateAssetMenu(fileName = "NewClueItem", menuName = "Inventory/Clue Item")]
public class ClueItemSO : ScriptableObject
{
    public string itemName;
    public string description;
    public bool isFlashlight;
}