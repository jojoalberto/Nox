using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CandlePuzzle : MonoBehaviourPun
{
    public static CandlePuzzle Instance;
    [SerializeField]
    private List<GameObject> candle;
    private int candleCount = 0;
    private int requiredCandles = 4;
    public UnityEvent onQuestComplete;
    [SerializeField]
    private DialogueMessage dialogueMessage;
    [SerializeField]
    private DialogueManager dialogueManager;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {

    }

    void Update()
    {
        
    }
    [PunRPC]
    public void AddCandle()
    {
        candleCount++;
        Debug.Log("candle count " + candleCount);
        photonView.RPC("CheckCandleWinCondition", RpcTarget.All);

    }
    [PunRPC]
    private void CheckCandleWinCondition()
    {
        if (candleCount >= requiredCandles)
        {
            for (int i = 0; i < candle.Count; i++)
            {
                Debug.Log("chest unlock ");
                candle[i].SetActive(true);
                StartCoroutine(ShowFinalDialogueWithDelay());
                onQuestComplete.Invoke();
            }
        }
    }


    private IEnumerator ShowFinalDialogueWithDelay()
    {
        yield return new WaitForSeconds(3.5f);
        photonView.RPC("ShowChestDialogueRPC", RpcTarget.All, dialogueMessage.GetDialogueMessage(0));

    }


    [PunRPC]
    void ShowChestDialogueRPC(string message)
    {

        dialogueManager.ShowDialogue(message);

    }
        
}
