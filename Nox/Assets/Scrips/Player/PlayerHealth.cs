using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviourPun
{
    public GameObject playerObject;
    public PlayerData playerData;
    public float totalHealth = 1;
    public float currentHealth = 1;
    public float temporaryHealth = 0;
    public float temporaryHealthDecay = 1f;
    public bool invulnerability = false;

    public Transform purgatoryLocation;

    private Coroutine tempHealthCoroutine;

    public PlayerScriptBehaviour playerScriptBehaviour;
    public HealthBar healthBar;

    void Start()
    {
        StartCoroutine(WaitForHUDAndSetHealth());
    }

    private IEnumerator WaitForHUDAndSetHealth()
    {
        // Wait until playerScriptBehaviour and hudInstance are both valid
        while (playerScriptBehaviour == null || playerScriptBehaviour.hudInstance == null)
        {
            yield return null;
        }

        if (playerData != null)
        {
            SetPlayerHealth();
        }
    }


    private void SetPlayerHealth()
    {
        totalHealth = playerData.GetTotalHealth();
        currentHealth = totalHealth;

        
        healthBar = playerScriptBehaviour.hudInstance.GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.playerHealth = this;
            healthBar.SetMaxHealth();
        }
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
        if (temporaryHealth > 0)
        {
            int tempAbsorbed = Mathf.Min(damageAmount, (int)temporaryHealth);
            temporaryHealth -= tempAbsorbed;
            damageAmount -= tempAbsorbed;
        }

        currentHealth -= damageAmount;
        if (currentHealth <= 0) GoToPurgatory();

        if (photonView.IsMine)
        {
            healthBar.UpdateHealth();
        }
    }

    [PunRPC]
    public void RPC_RestoreHealthPercent(float value)
    {
        currentHealth += totalHealth * value;
        currentHealth = Mathf.Min(currentHealth, totalHealth);

        if (photonView.IsMine)
        {
            healthBar.UpdateHealth();
        }
    }

    [PunRPC]
    public void RPC_AddTemporaryHealth(float value)
    {
        temporaryHealth += value;

        if (tempHealthCoroutine != null)
            StopCoroutine(tempHealthCoroutine);

        tempHealthCoroutine = StartCoroutine(DecayTemporaryHealth());
    }


    private IEnumerator DecayTemporaryHealth()
    {
        while (temporaryHealth > 0)
        {
            yield return new WaitForSeconds(1f);
            temporaryHealth -= temporaryHealthDecay;
        }

        temporaryHealth = 0;
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