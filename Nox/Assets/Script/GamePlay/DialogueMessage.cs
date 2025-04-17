using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueMessage : MonoBehaviour
{
    public List<string> dialogueMessage;
    [SerializeField]
    private Image image;


    public string GetDialogueMessage(int message)
    {
        return dialogueMessage[message];
    }

    public Image GetImage()
    {
        return image;
    }

}
