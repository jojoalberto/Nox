using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    PlayerHealth[] allPlayers;
    public void DealDamageToAll(int damageAmount)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        allPlayers = Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        foreach (PlayerHealth player in allPlayers)
        {
            player.photonView.RPC("RPC_RequestDamageAll", player.photonView.Owner, damageAmount);
        }
    }

    public void EnableCursor()
    {
            Debug.Log("showing Cursor");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        
    }

    public void DisableCursor()
    {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
    }
}
