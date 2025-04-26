using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueMessage : MonoBehaviour
{
    public List<string> dialogueMessage;
    private Sprite image;


    public string GetDialogueMessage(int message)
    {
        return dialogueMessage[message];
    }

    public Sprite GetImage()
    {
        return image;
    }

}
