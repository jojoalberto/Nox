using System.Collections.Generic;
using UnityEngine;

public class ClueDatabase : MonoBehaviour
{
    public List<ClueItemSO> clues;
    public static Dictionary<string, ClueItemSO> ClueLookup;

    void Awake()
    {
        ClueLookup = new Dictionary<string, ClueItemSO>();
        foreach (var clue in clues)
        {
            if (!ClueLookup.ContainsKey(clue.itemID))
            {
                ClueLookup.Add(clue.itemID, clue);
            }
        }
    }

    public static ClueItemSO GetClueByID(string id)
    {
        return ClueLookup.TryGetValue(id, out var clue) ? clue : null;
    }
}
