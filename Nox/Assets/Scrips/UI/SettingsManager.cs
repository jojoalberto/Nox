using System.Runtime.CompilerServices;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject lastMenu;
    [SerializeField] private GameObject settingsMenus;
    [SerializeField] private GameObject[] menus;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveLastMenu()
    {
        foreach (GameObject obj in menus)
        {
            if (obj.activeSelf)
            {
                lastMenu = obj;
            }
        }
    }



    public void SettingsButton()
    {
        if(settingsMenus.activeSelf)
        {
            settingsMenus.SetActive(false);
            lastMenu.SetActive(true);
        }
        else
        {
            SaveLastMenu();
            lastMenu.SetActive(false);
            settingsMenus.SetActive(true);
        }
        
    }


}
