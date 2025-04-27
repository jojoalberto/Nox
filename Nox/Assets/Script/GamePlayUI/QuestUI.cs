using Photon.Pun;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questText;
    private string tempText;
    [SerializeField]
    private Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void QuestTextUpdate(string text)
    {
        tempText = text;
        questText.text = tempText;
    }

    public void ShowImage(string itemID)
    {
        var data = ClueDatabase.GetClueByID(itemID);
        if (data != null)
        {
            image.gameObject.SetActive(true);
            image.sprite = data.image;
        }
        else
        {
            Debug.LogWarning($"ClueItemSO with ID '{itemID}' not found.");
        }

    }
    public void HideImage()
    {
        image.gameObject.SetActive(false);
    }
}

