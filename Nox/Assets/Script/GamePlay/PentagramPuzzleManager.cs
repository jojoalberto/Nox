using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PentagramPuzzleManager : MonoBehaviourPunCallbacks
{
    public static PentagramPuzzleManager Instance;
    public UnityEvent onPuzzleComplete;

    public PentagramMark[] marks; // Assign all marks in Inspector
    private bool puzzleCompleted = false;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckPuzzleStatus()
    {
        if (puzzleCompleted) return;

        foreach (var mark in marks)
        {
            if (!mark.isCorrectlyOccupied)
            {
                return; // Puzzle not solved yet
            }
        }

        PuzzleCompleted();
    }

    private void PuzzleCompleted()
    {

        puzzleCompleted = true;
        Debug.Log("Puzzle Completed!");
        photonView.RPC("OnPuzzleCompleted", RpcTarget.All);
    }

    [PunRPC]
    private void OnPuzzleCompleted()
    {
        onPuzzleComplete.Invoke();
        // Do whatever you want here (play animation, open portal, etc.)
        Debug.Log("Puzzle Completion Synced Across All Players!");
    }
}

