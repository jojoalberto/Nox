using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BookshelvesManager : MonoBehaviour
{
    [SerializeField]
    private ClueItemSO itemPrize;
    public UnityEvent onFinishQuest;
    public UnityEvent onIncorrectAnswer;
    [SerializeField]private DemonTargetAI1 demon;
    public List<GameObject> players;

    public void CorrectChoice()
    {
        onFinishQuest.Invoke();
    }

    public void IncorrectChoice()
    {
        onIncorrectAnswer.Invoke();
    }
}
