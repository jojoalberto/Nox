using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public TextMeshProUGUI dialogueText;
    public Image itemImage;
    public GameObject dialoguePanel;
    public GameObject imagePanel;
    public UnityEvent onDialogueEnds;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false); 
    }

    public void ShowDialogue(string message)
    {
        Debug.Log("showing Dialogue");
        dialoguePanel.SetActive(true);
        dialogueText.text = message;
        Canvas.ForceUpdateCanvases();
        StartCoroutine(StartCloseDialogue());
    }


    public void ShowImage(string clueID)
    {
        var data = ClueDatabase.GetClueByID(clueID);
        if (data != null)
        {
            imagePanel.SetActive(true);
            itemImage.sprite = data.image;
        }
        else
        {
            Debug.LogWarning($"ClueItemSO with ID '{clueID}' not found.");
        }
    }

    private void Update()
    {
        if(dialoguePanel.activeSelf)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("closing Dialogue");
                CloseDialogue();
            }
        }
    }

    IEnumerator StartCloseDialogue()
    {
        yield return new WaitForSeconds(3f); // Wait for 3 seconds
        CloseDialogue();
    }

    void CloseDialogue()
    {
        onDialogueEnds.Invoke();
        imagePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        StopAllCoroutines();
    }

}