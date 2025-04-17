using UnityEngine;

[CreateAssetMenu(fileName = "NewClueItem", menuName = "Inventory/Clue Item")]
public class ClueItemSO : ScriptableObject
{
    public string itemID;
    public string itemName;
    public string description;
    public Sprite image;
    public bool isFlashlight;
    public bool isCandle;
}