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
    public GameObject dialoguePanel;
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
        dialoguePanel.SetActive(false);
        StopAllCoroutines();
    }

}