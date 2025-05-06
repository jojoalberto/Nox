using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string userId;
    public string nickname;
    public string classSelected;
    public bool hasFlashlight = false;

    public float totalHealth = 100;
    public float totalStamina = 100;
    public float staminaRegen = 2.5f;
    public float staminaDelay = 2.5f;

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
        SetPlayerHealth();
    }

    public void setFlashlight(bool flashlight)
    {
        hasFlashlight = flashlight;
    }

    public bool getFlashlight()
    {
        return hasFlashlight;
    }

    public void SetPlayerHealth()
    {
        if (getClassSelected() == "Protector")
        {
            totalHealth = 150;
            totalStamina = 150;
        }
        else if (getClassSelected() == "Occultist")
        {
            totalHealth = 100;
            totalStamina = 100;
        }
        else if (getClassSelected() == "Drifter")
        {
            totalHealth = 100;
            totalStamina = 100;
        }
        else if (getClassSelected() == "Trapper")
        {
            totalHealth = 100;
            totalStamina = 100;
        }
    }

    public float GetTotalHealth()
    {
        return totalHealth; 
    }
    public float GetTotalStamina()
    {
        return totalStamina;
    }

    public float GetStaminaRegen()
    {
        return staminaRegen;
    }

    public float GetStaminaDelay()
    {
        return staminaDelay;
    }
}