using System;
using System.Collections.Generic;
using NUnit.Framework;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoom : MonoBehaviourPunCallbacks
{
    public string[] playerNames = { "Open", "Open", "Open", "Open" };
    public TextMeshProUGUI[] playerGUIs;

    public PlayerData playerData;

    public Transform lobbyPlayersHeading;
    public GameObject[] buttonGameObjects = new GameObject[4];
    public Button startButton;

    public string[] selectedClasses = { "None", "None", "None", "None" };
   

    void Start()
    {
        UpdatePlayerUI();
    }

    private void getButtonList()
    {
        
        int currentActor = PhotonNetwork.LocalPlayer.ActorNumber;
        
        if (currentActor == 1)
        {
            buttonGameObjects[0] = lobbyPlayersHeading.GetChild(0).GetChild(1).gameObject;
            buttonGameObjects[1] = lobbyPlayersHeading.GetChild(0).GetChild(2).gameObject;
            buttonGameObjects[2] = lobbyPlayersHeading.GetChild(0).GetChild(3).gameObject;
            buttonGameObjects[3] = lobbyPlayersHeading.GetChild(0).GetChild(4).gameObject;
        }
        else if (currentActor == 2)
        {
            buttonGameObjects[0] = lobbyPlayersHeading.GetChild(1).GetChild(1).gameObject;
            buttonGameObjects[1] = lobbyPlayersHeading.GetChild(1).GetChild(2).gameObject;
            buttonGameObjects[2] = lobbyPlayersHeading.GetChild(1).GetChild(3).gameObject;
            buttonGameObjects[3] = lobbyPlayersHeading.GetChild(1).GetChild(4).gameObject;


        }
        else if(currentActor == 3)
        {
            buttonGameObjects[0] = lobbyPlayersHeading.GetChild(2).GetChild(1).gameObject;
            buttonGameObjects[1] = lobbyPlayersHeading.GetChild(2).GetChild(2).gameObject;
            buttonGameObjects[2] = lobbyPlayersHeading.GetChild(2).GetChild(3).gameObject;
            buttonGameObjects[3] = lobbyPlayersHeading.GetChild(2).GetChild(4).gameObject;
        }
        else if(currentActor == 4)
        {
            buttonGameObjects[0] = lobbyPlayersHeading.GetChild(3).GetChild(1).gameObject;
            buttonGameObjects[1] = lobbyPlayersHeading.GetChild(3).GetChild(2).gameObject;
            buttonGameObjects[2] = lobbyPlayersHeading.GetChild(3).GetChild(3).gameObject;
            buttonGameObjects[3] = lobbyPlayersHeading.GetChild(3).GetChild(4).gameObject;
        }

        AddButtonListeners();
    }

    public void AddButtonListeners()
    {
        buttonGameObjects[0].GetComponent<Button>().onClick.AddListener(ClassSelectionProtector);
        buttonGameObjects[1].GetComponent<Button>().onClick.AddListener(ClassSelectionOccultist);
        buttonGameObjects[2].GetComponent<Button>().onClick.AddListener(ClassSelectionDrifter);
        buttonGameObjects[3].GetComponent<Button>().onClick.AddListener(ClassSelectionTrapper);
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
            getButtonList();
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

    public void ClassSelectionProtector()
    {

        playerData.Setclass("Protector");
        

        int currentActor = PhotonNetwork.LocalPlayer.ActorNumber;
        selectedClasses[currentActor - 1] = playerData.getClassSelected();
        UpdateSelectedClasses(selectedClasses);
    }

    public void ClassSelectionOccultist()
    {
        playerData.Setclass("Occultist");

        int currentActor = PhotonNetwork.LocalPlayer.ActorNumber;
        selectedClasses[currentActor - 1] = playerData.getClassSelected();
        UpdateSelectedClasses(selectedClasses);
    }

    public void ClassSelectionDrifter()
    {
        playerData.Setclass("Drifter");

        int currentActor = PhotonNetwork.LocalPlayer.ActorNumber;
        selectedClasses[currentActor - 1] = playerData.getClassSelected();
        UpdateSelectedClasses(selectedClasses);
    }

    public void ClassSelectionTrapper()
    {
        playerData.Setclass("Trapper");

        int currentActor = PhotonNetwork.LocalPlayer.ActorNumber;
        selectedClasses[currentActor - 1] = playerData.getClassSelected();
        UpdateSelectedClasses(selectedClasses);
    }

    public void UpdateSelectedClasses(string[] receivedSelectedClasses)
    {
        

        photonView.RPC("SetPlayerClasses", RpcTarget.Others, receivedSelectedClasses);

        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            if(CheckSelectedClasses())
            {
                startButton.interactable = false;
            }
            else
            {
                startButton.interactable = true;
            }
        }
    }

    [PunRPC]
    public void SetPlayerClasses(string[] receivedSelectedClasses)
    {
        selectedClasses = receivedSelectedClasses;
    }

    private bool CheckSelectedClasses()
    {
        HashSet<string> seen = new HashSet<string>();
        foreach (string classSelected in selectedClasses)
        {
            if (classSelected == "None")
                return true;
            else if (!seen.Add(classSelected))
                return true;
        }
        return false;
    }
}
