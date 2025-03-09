using System;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class LobbyRoom : MonoBehaviourPunCallbacks
{
    public string[] playerNames = { "Open", "Open", "Open", "Open" };
    public TextMeshProUGUI[] playerGUIs;

    public PlayerData playerData;

   

    void Start()
    {
        UpdatePlayerUI();
    }

    public void UpdatePlayerNamesOnJoin()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        int playerIndex = actorNumber - 1;
        UpdatePlayerName(playerIndex, playerData.GetNickname());
    }

    [PunRPC]
    public void SetPlayerNames(int index, string name)
    {
        if (playerNames[index] == name)
        {
            return;
        }

        playerNames[index] = name;
        UpdatePlayerUI();

        if (photonView.IsMine)
        {
            UpdatePlayerName(index, name);
        }
    }

    private void UpdatePlayerUI()
    {
        for (int i = 0; i < playerNames.Length; i++)
        {
            playerGUIs[i].text = playerNames[i];
        }
    }

    public string GetPlayerNamesList()
    {
        return string.Join(", ", playerNames);
    }

   
    public void UpdatePlayerName(int index, string name)
    {
        photonView.RPC("SetPlayerNames", RpcTarget.AllBuffered, index, name);
    }

}
