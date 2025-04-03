using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviourPun
{
    public GameObject playerObject;
    public PlayerData playerData;
    public float totalHealth = 1;
    public float currentHealth = 1;
    public bool invulnerability = false;

    public Transform purgatoryLocation;

    void Start()
    {
        if (playerData != null)
        {
            SetPlayerHealth();
        }
    }

    private void SetPlayerHealth()
    {
        totalHealth = playerData.GetTotalHealth();
        currentHealth = totalHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (invulnerability) return;

        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damageAmount);
    }

    public void RestoreHealing(float value)
    {
        photonView.RPC("RPC_RestoreHealthPercent", RpcTarget.All, value);
    }

    [PunRPC]
    private void RPC_TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) GoToPurgatory();
    }

    [PunRPC]
    public void RPC_RestoreHealthPercent(float value)
    {
        currentHealth += totalHealth * value;
        currentHealth = Mathf.Min(currentHealth, totalHealth);
    }

    [PunRPC]
    private void GoToPurgatory()
    {
        if (purgatoryLocation == null)
            purgatoryLocation = GameObject.FindGameObjectWithTag("Purgatory")?.transform;

        if (purgatoryLocation != null)
            transform.position = purgatoryLocation.position;
    }
}