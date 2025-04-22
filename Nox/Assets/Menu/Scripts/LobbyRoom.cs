using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
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
    public Image[] playerImages;
    public Sprite[] spriteList;
    public Sprite noneSprite;

    [SerializeField] private string[] protectorDetails = { "Munimen Sanitas - Protector has more Health and Stamina.", "(1) Salus Brevis - Give nearby characters temporary health.", "(2) Audacia Mortalis - Taunts nearby enemy." };
    [SerializeField] private string[] occultistDetails = { "Spectaculum Tenebris - Occultist can see the demon through walls at regular intervals.", "(1) Auget Agilitas - Increases Movement Speed of Allies.", "(2) Sanatio Tenebris - Restores Health To Allies." };
    [SerializeField] private string[] drifterDetails = { "Fuga Velox - Drifter has higher movement speed.", "(1) Umbra Fugitiva - Become invisible.", "(2) Aethereus Vinculum - Bind yourself and the enemy for a brief period." };
    [SerializeField] private string[] trapperDetails = { "Munera Venandi - Trapper can interact with unique collectibles to charge his abilities.", "(1) Hibernus Impedimentum - Throw a slowing grenade.", "(2) Gelu Immobilis - Place a trap that freezes the enemy for a few seconds." };

    [SerializeField] private GameObject[] classDetails = {};

    private bool isStartingGame = false;
    void Start()
    {
        UpdatePlayerUI();
    }

    private void getButtonList()
    {

        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform playerTransform = lobbyPlayersHeading.GetChild(playerIndex);

        for (int i = 0; i < 4; i++)
        {
            buttonGameObjects[i] = playerTransform.GetChild(i + 1).gameObject;
        }

        AddButtonListeners();
    }

    public void AddButtonListeners()
    {
        buttonGameObjects[0].GetComponent<Button>().onClick.AddListener(ClassSelectionProtector);
        buttonGameObjects[1].GetComponent<Button>().onClick.AddListener(ClassSelectionOccultist);
        buttonGameObjects[2].GetComponent<Button>().onClick.AddListener(ClassSelectionDrifter);
        buttonGameObjects[3].GetComponent<Button>().onClick.AddListener(ClassSelectionTrapper);

        foreach (GameObject obj in buttonGameObjects)
        {
            obj.SetActive(true);
        }
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

        classDetails[0].SetActive(true);
        classDetails[1].GetComponent<TextMeshProUGUI>().text = selectedClass;
        classDetails[2].GetComponent<TextMeshProUGUI>().text = protectorDetails[0];
        classDetails[3].GetComponent<TextMeshProUGUI>().text = protectorDetails[1];
        classDetails[4].GetComponent<TextMeshProUGUI>().text = protectorDetails[2];
    }

    public void ClassSelectionOccultist()
    {
        string selectedClass = "Occultist";
        playerData.Setclass(selectedClass);
        ReportClassSelection(selectedClass);

        classDetails[0].SetActive(true);
        classDetails[1].GetComponent<TextMeshProUGUI>().text = selectedClass;
        classDetails[2].GetComponent<TextMeshProUGUI>().text = occultistDetails[0];
        classDetails[3].GetComponent<TextMeshProUGUI>().text = occultistDetails[1];
        classDetails[4].GetComponent<TextMeshProUGUI>().text = occultistDetails[2];
    }

    public void ClassSelectionDrifter()
    {
        string selectedClass = "Drifter";
        playerData.Setclass(selectedClass);
        ReportClassSelection(selectedClass);

        classDetails[0].SetActive(true);
        classDetails[1].GetComponent<TextMeshProUGUI>().text = selectedClass;
        classDetails[2].GetComponent<TextMeshProUGUI>().text = drifterDetails[0];
        classDetails[3].GetComponent<TextMeshProUGUI>().text = drifterDetails[1];
        classDetails[4].GetComponent<TextMeshProUGUI>().text = drifterDetails[2];
    }

    public void ClassSelectionTrapper()
    {
        string selectedClass = "Trapper";
        playerData.Setclass(selectedClass);
        ReportClassSelection(selectedClass);

        classDetails[0].SetActive(true);
        classDetails[1].GetComponent<TextMeshProUGUI>().text = selectedClass;
        classDetails[2].GetComponent<TextMeshProUGUI>().text = trapperDetails[0];
        classDetails[3].GetComponent<TextMeshProUGUI>().text = trapperDetails[1];
        classDetails[4].GetComponent<TextMeshProUGUI>().text = trapperDetails[2];
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
        for(int i =0; i< selectedClasses.Length; i++)
        {
            if (playerImages.Length > i)  // Prevent index out of range
            {
                // Set the sprite based on the class
                switch (selectedClasses[i])
                {
                    case "Protector":
                        playerImages[i].sprite = spriteList[0];
                        SetAlpha(playerImages[i], 1f);
                        break;

                    case "Occultist":
                        playerImages[i].sprite = spriteList[1];
                        SetAlpha(playerImages[i], 1f);
                        break;

                    case "Drifter":
                        playerImages[i].sprite = spriteList[2];
                        SetAlpha(playerImages[i], 1f);
                        break;

                    case "Trapper":
                        playerImages[i].sprite = spriteList[3];
                        SetAlpha(playerImages[i], 1f);
                        break;

                    default:
                        playerImages[i].sprite = noneSprite;
                        SetAlpha(playerImages[i], 0f);
                        break;
                }
            }
        }
    }

    private void SetAlpha(Image img, float alpha)
    {
        Color color = img.color;
        color.a = alpha;
        img.color = color;
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

        if (isStartingGame)
            return; 

        isStartingGame = true;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel(1);
            
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncSelectedClasses", newPlayer, selectedClasses);
        }
    }

}
