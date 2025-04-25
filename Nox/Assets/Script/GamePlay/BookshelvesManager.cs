using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BookshelvesManager : MonoBehaviourPun
{
    public static BookshelvesManager Instance;
    [SerializeField]
    private ClueItemSO itemPrize;
    public UnityEvent onFinishQuest;
    public UnityEvent onIncorrectAnswer;
    [SerializeField]private int bookShelf;
    public List<GameObject> players;
    private HashSet<string> interactedBookshelves = new HashSet<string>();
    private int totalBookshelves;
    public UnityEvent onFinalQuest;
    private bool puzzleSolved = false;
    [SerializeField]
    private GameObject QuizUI;
    private Coroutine repeatQuizCoroutine;

    void Awake()
    {
        Instance = this;
        totalBookshelves = GameObject.FindGameObjectsWithTag("Bookshelf").Length;
    }
    public void CorrectChoice()
    {
        if (repeatQuizCoroutine != null)
        {
            StopCoroutine(repeatQuizCoroutine);
            repeatQuizCoroutine = null;
        }
        onFinishQuest.Invoke();
        puzzleSolved = true;
    }

    public void IncorrectChoice()
    {
        onIncorrectAnswer.Invoke();
        photonView.RPC("RepeatQuizRPC", RpcTarget.All);
    }
    [PunRPC]
    public void RepeatQuizRPC()
    {
        if (repeatQuizCoroutine == null)
        {
            repeatQuizCoroutine = StartCoroutine(RepeatWrongAnswerDialogue());
        }
    }
    public void RegisterInteraction(string bookshelfID)
    {
        if (!interactedBookshelves.Contains(bookshelfID))
        {
            interactedBookshelves.Add(bookshelfID);
            Debug.Log($"Bookshelf {bookshelfID} interacted with ({interactedBookshelves.Count}/{totalBookshelves})");

            if (interactedBookshelves.Count == totalBookshelves)
            {
                photonView.RPC("TriggerFinalEventRPC", RpcTarget.All);
            }
        }
    }
    [PunRPC]
    private void TriggerFinalEventRPC()
    {
        onFinalQuest.Invoke();
    }

    private IEnumerator RepeatWrongAnswerDialogue()
    {
        while (!puzzleSolved)
        {
            yield return new WaitForSeconds(10);

            if (!puzzleSolved)
            {
                QuizUI.SetActive(true);
            }
        }
    }
}
