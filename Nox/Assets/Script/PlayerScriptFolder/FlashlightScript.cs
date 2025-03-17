using Photon.Pun;
using UnityEngine;

public class FlashlightScript : MonoBehaviourPun
{
    public PlayerData playerData; 
    public GameObject flashlight;   

    private void Start()
    {
        if (!playerData.hasFlashlight)
        {
            flashlight.SetActive(false); 
        }
    }

    private void Update()
    {
        if (playerData.hasFlashlight && Input.GetKeyDown(KeyCode.F))
        {
            flashlight.SetActive(!flashlight.activeSelf); 
            photonView.RPC("SyncFlashlightState", RpcTarget.Others, flashlight.activeSelf);
        }
    }

    [PunRPC]
    void SyncFlashlightState(bool state)
    {
        flashlight.SetActive(state); 
    }
}
