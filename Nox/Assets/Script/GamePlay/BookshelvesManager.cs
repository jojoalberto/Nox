using UnityEngine;
using UnityEngine.Events;

public class BookshelvesManager : MonoBehaviour
{
    [SerializeField]
    private ClueItemSO itemPrize;
    public UnityEvent onFinishQuest;
    public UnityEvent onIncorrectAnswer;

    public void CorrectChoice()
    {
        onFinishQuest.Invoke();
    }

    public void IncorrectChoice()
    {
        onIncorrectAnswer.Invoke();
    }

}
