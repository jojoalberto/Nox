using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string userId;
    public string nickname;

    public string GetUserId()
    { 
        return userId; 
    }
    public string GetNickname() 
    { 
        return nickname; 
    }

}