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
            
        }
        getButtonList();
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
        string selectedClass = "Protector";
        playerData.Setclass(selectedClass);
        ReportClassSelection(selectedClass);
    }

    public void ClassSelectionOccultist()
    {
        string selectedClass = "Occultist";
        playerData.Setclass(selectedClass);
        ReportClassSelection(selectedClass);
    }

    public void ClassSelectionDrifter()
    {
        string selectedClass = "Drifter";
        playerData.Setclass(selectedClass);
        ReportClassSelection(selectedClass);
    }

    public void ClassSelectionTrapper()
    {
        string selectedClass = "Trapper";
        playerData.Setclass(selectedClass);
        ReportClassSelection(selectedClass);
    }

    private void ReportClassSelection(string className)
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC("ReportClassToMaster", RpcTarget.MasterClient, actorNumber, className);
    }

    [PunRPC]
    private void ReportClassToMaster(int actorNumber, string className)
    {
        int index = actorNumber - 1;
        selectedClasses[index] = className;
        photonView.RPC("SyncSelectedClasses", RpcTarget.All, selectedClasses);
    }

    [PunRPC]
    private void SyncSelectedClasses(string[] updatedClasses)
    {
        selectedClasses = updatedClasses;
        UpdateClassSelectionUI();
        ValidateClassSelections();
    }
    private void UpdateClassSelectionUI()
    {
        // Add any UI update logic here if needed
    }

    private void ValidateClassSelections()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.interactable = !CheckSelectedClasses();
        }
    }

    public void UpdateSelectedClasses(string[] receivedSelectedClasses)
    {
        
        
        photonView.RPC("SetPlayerClasses", RpcTarget.Others, receivedSelectedClasses);

        
    }

    [PunRPC]
    public void SetPlayerClasses(string[] receivedSelectedClasses)
    {

        selectedClasses = receivedSelectedClasses;

        if (PhotonNetwork.IsMasterClient)
        {
            if (CheckSelectedClasses())
            {
                startButton.interactable = false;
            }
            else
            {
                startButton.interactable = true;
            }
        }
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

    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(1);
            
    }
}
