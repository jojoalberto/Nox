using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public void DealDamageToAll(int damageAmount)
    {
        PlayerHealth[] allPlayers = Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        foreach (PlayerHealth player in allPlayers)
        {
            // Only let the owner call the RPC to avoid duplicate calls
            if (player.photonView.IsMine)
            {
                player.photonView.RPC("RPC_TakeDamage", RpcTarget.All, damageAmount);
            }
        }
    }
}
