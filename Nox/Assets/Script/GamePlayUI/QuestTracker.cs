using Photon.Pun;
using UnityEngine;
public class QuestTracker : MonoBehaviourPun
{
    public QuestUI UI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void CallUpdateQuestRPC(string Quest)
    {
        photonView.RPC("UpdateQuest", RpcTarget.All, Quest);
    }
    [PunRPC]
    public void UpdateQuest(string Quest)
    {
        UI.QuestTextUpdate(Quest);
    }
}
