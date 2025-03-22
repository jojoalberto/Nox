using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string userId;
    public string nickname;
    public string classSelected;
    public bool hasFlashlight;

    public float totalHealth = 1;

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
        SetPlaayerHealth();
    }

    public void setFlashlight(bool flashlight)
    {
        hasFlashlight = flashlight;
    }

    public bool getFlashlight()
    {
        return hasFlashlight;
    }

    public void SetPlaayerHealth()
    {
        if (getClassSelected() == "Protector")
        {
            totalHealth = 4;
        }
        else if (getClassSelected() == "Occultist")
        {
            totalHealth = 2;
        }
        else if (getClassSelected() == "Drifter")
        {
            totalHealth = 2;
        }
        else if (getClassSelected() == "Trapper")
        {
            totalHealth = 2;
        }
    }

    internal float GetTotalHealth()
    {
        return totalHealth; 
    }
}