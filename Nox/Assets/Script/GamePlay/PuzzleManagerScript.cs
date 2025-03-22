using UnityEngine;

public class PuzzleManagerScript : MonoBehaviour
{
    [SerializeField]
    private int numberOfPuzzleTargetPoints;
    private int currentPoints;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPointsToPuzzle(int points)
    {
        numberOfPuzzleTargetPoints = points + currentPoints;
    }

    public int GetPuzzleTargetPoints()
    {
        return numberOfPuzzleTargetPoints;
    }
    public int GetPuzzleCurrentPoints()
    {
        return currentPoints;
    }
    
    public void ResetPoints()
    {
        currentPoints = 0;
    }

}
