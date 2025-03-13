using UnityEngine;
using Photon.Pun;
using TMPro;


public class PlayerIGN : MonoBehaviourPun
{
    public TextMeshProUGUI iGNText;
    public GameObject playerCamera;

    [PunRPC]
    public void SetNickname(string nickname)
    {
        iGNText.text = nickname;
    }

    public string GetNickname()
    {
        return iGNText.text;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SetNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }
        else
        {
            playerCamera.SetActive(false);
        }
    }
}
