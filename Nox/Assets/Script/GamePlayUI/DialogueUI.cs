using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

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
        dialoguePanel.SetActive(false);
        StopAllCoroutines();
    }

}