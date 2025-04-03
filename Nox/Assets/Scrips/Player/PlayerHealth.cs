using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviourPun, IPunObservable
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
        // Only let the owning client trigger damage updates.
        if (!photonView.IsMine || invulnerability) return;

        photonView.RPC("RPC_TakeDamage", RpcTarget.MasterClient, damageAmount);
    }

    [PunRPC]
    private void RPC_TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            GoToPurgatory();
        }
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

    // This method is called by Photon to sync variables.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // The owner sends the current health to others.
            stream.SendNext(currentHealth);
            stream.SendNext(totalHealth);
        }
        else
        {
            // Non-owners receive the updated health values.
            currentHealth = (float)stream.ReceiveNext();
            totalHealth = (float)stream.ReceiveNext();
        }
    }
}
