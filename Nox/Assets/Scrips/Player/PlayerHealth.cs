using System;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public GameObject playerObject;
    public PlayerData playerData;
    public float totalHealth = 1;
    public float currentHealth = 1;
    public bool invulnerability = false;
    
    public Transform purgatoryLocation;
    public PhotonView photonView;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if(playerData != null)
        {
            if (playerData.getClassSelected() == "Protector")
            {
                totalHealth = 4;
                currentHealth = 4;
            }
            else if(playerData.getClassSelected() == "Occultist")
            {
                totalHealth = 2;
                currentHealth = 2;
            }
            else if (playerData.getClassSelected() == "Drifter")
            {
                totalHealth = 2;
                currentHealth = 2;
            }
            else if (playerData.getClassSelected() == "Trapper")
            {
                totalHealth = 2;
                currentHealth = 2;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) 
        {
            goToPurgatory();
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void goToPurgatory()
    {

        photonView.RPC("GoToPurgatory", RpcTarget.All);
    }

    [PunRPC]
    public void GoToPurgatory()
    {
        purgatoryLocation = GameObject.FindGameObjectWithTag("Purgatory").transform;
        transform.position = purgatoryLocation.position;
    }

}
