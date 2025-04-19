using UnityEngine;
public enum ClueItemType
{
    General,
    Key,
    Candle,
    Flashlight,
    Artifact,
    Vinyl
}

[CreateAssetMenu(fileName = "NewClueItem", menuName = "Inventory/Clue Item")]
public class ClueItemSO : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite image;
    public ClueItemType itemType;
    [Tooltip("Only used if item is a Key")]
    public string keyID;
}