using UnityEngine;
using Photon.Pun;
using TMPro;


public class PlayerIGN : MonoBehaviourPun
{

    public GameObject textObject;
    public TextMeshProUGUI iGNText;
    public GameObject playerCamera;
    public Camera mainCamera;



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
            GameObject.Destroy(playerCamera);
        }
    }

    private void Update()
    {
        mainCamera = (Camera) FindAnyObjectByType(typeof(Camera));
        if(mainCamera)
        {
            transform.LookAt(mainCamera.transform.position);   
        }

    }
}
