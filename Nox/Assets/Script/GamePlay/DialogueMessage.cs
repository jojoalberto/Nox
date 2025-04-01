using UnityEngine;
using System.Collections.Generic;

public class DialogueMessage : MonoBehaviour
{
    public List<string> dialogueMessage;

    public string GetDialogueMessage(int message)
    {
        return dialogueMessage[message];
    }

}
