using System.Collections.Generic;
using UnityEngine;

public class CandlePuzzle : MonoBehaviour
{
    public static CandlePuzzle Instance;
    [SerializeField]
    private List<GameObject> candle;
    private int candleCount = 0;
    private int requiredCandles;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        requiredCandles = candle.Count;
    }

    void Update()
    {
        
    }
    public void AddCandle()
    {
        candleCount++;
        Debug.Log("candle count + " + candleCount);
        if (candleCount >= requiredCandles)
        {
            CandleChest.Instance.UnlockChest();
            for(int i=0;i<candle.Count;i++)
            {
                candle[i].SetActive(true);
            }
        }
    }


}
