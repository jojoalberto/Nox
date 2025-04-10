using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CandlePuzzle : MonoBehaviour
{
    public static CandlePuzzle Instance;
    [SerializeField]
    private List<GameObject> candle;
    private int candleCount = 0;
    private int requiredCandles = 4;
    public UnityEvent onQuestComplete;
    [SerializeField]
    private DialogueMessage dialogueMessage;

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
    public void AddCandle()
    {
        candleCount++;
        Debug.Log("candle count + " + candleCount);
        CheckCandleWinCondition();

    }

    private void CheckCandleWinCondition()
    {
        if (candleCount >= requiredCandles)
        {
            CandleChest.Instance.UnlockChest();
            for (int i = 0; i < candle.Count; i++)
            {
                candle[i].SetActive(true);
                StartCoroutine(ShowFinalDialogueWithDelay());
                onQuestComplete.Invoke();
            }
        }
    }
    private IEnumerator ShowFinalDialogueWithDelay()
    {
        yield return new WaitForSeconds(3.5f); 
        DialogueManager.Instance.ShowDialogue(dialogueMessage.GetDialogueMessage(1));
    }
}
