using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BookshelvesManager : MonoBehaviourPun
{
    public static BookshelvesManager Instance;
    public UnityEvent onFinishQuest;
    public UnityEvent onIncorrectAnswer;
    public UnityEvent onQuizRepeat;
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
        
    }
    private void Start()
    {

    }
    private void Update()
    {

    }
    public void ScanBookShelves()
    {
        GameObject[] foundBookshelves = GameObject.FindGameObjectsWithTag("Bookshelf");
        totalBookshelves = foundBookshelves.Length;
        Debug.Log("Total Bookshelves Found: " + totalBookshelves);
        foreach (var shelf in foundBookshelves)
        {
            Debug.Log("bookshelf name is: " + shelf.name);
        }
    }
 
    public void CorrectChoice()
    {
        photonView.RPC("CorrectChoiceRPC", RpcTarget.All);
    }
    [PunRPC]
    public void CorrectChoiceRPC()
    {
        if (repeatQuizCoroutine != null)
        {
            StopCoroutine(repeatQuizCoroutine);
            repeatQuizCoroutine = null;
        }
        QuizUI.SetActive(false);
        onFinishQuest.Invoke();
        puzzleSolved = true;
    }
    [PunRPC]
    public void IncorrectChoiceRPC()
    {
        QuizUI.SetActive(false);
        onIncorrectAnswer.Invoke();
    }
    public void IncorrectChoice()
    {
        photonView.RPC("IncorrectChoiceRPC", RpcTarget.All);
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

            //if (interactedBookshelves.Count >= totalBookshelves)
            //{
            //    photonView.RPC("TriggerFinalEventRPC", RpcTarget.All);
            //}
        }
        if (interactedBookshelves.Count >= totalBookshelves)
        {
            Debug.Log("triggering final Quiz part RPC");
            photonView.RPC("TriggerFinalEventRPC", RpcTarget.All);
        }
    }
    [PunRPC]
    public void TriggerFinalEventRPC()
    {
        Debug.Log("triggering final Quiz part");
        onFinalQuest.Invoke();
        StartCoroutine(StartQuiz());
    }
    private IEnumerator StartQuiz()
    {
        yield return new WaitForSeconds(8);
        Debug.Log("setting QUiz ui true");
        QuizUI.SetActive(true);
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
