using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string userId;
    public string nickname;
    public string classSelected;

    public string GetUserId()
    { 
        return userId; 
    }
    public string GetNickname() 
    { 
        return nickname; 
    }

    public string getClassSelected()
    {
        return classSelected;
    }

    public void Setclass(string className)
    {
        classSelected = className;
    }

}