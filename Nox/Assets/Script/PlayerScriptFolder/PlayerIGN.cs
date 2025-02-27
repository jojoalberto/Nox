using UnityEngine;
using Photon.Pun;
using TMPro;


public class PlayerIGN : MonoBehaviourPun
{
    public TextMeshProUGUI iGNText;

    [PunRPC]
    public void SetNickname(string nickname)
    {
        iGNText.text = nickname;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SetNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }
    }
}
