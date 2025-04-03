using Photon.Pun;
using UnityEngine;

public class PlayerScriptBehaviour : MonoBehaviour
{

    public PlayerData playerData;
    public Trapper trapper;
    public Occultist occultist;
    public Drifter drifter;

    public string globalClassSelected;
    public PhotonView photonView;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       photonView = GetComponent<PhotonView>();

        if(photonView.IsMine)
        {
            if (playerData == null)
            {
                Debug.Log("No player data script found");
            }
            else
            {
                globalClassSelected = playerData.getClassSelected();
                if (globalClassSelected != null)
                {
                    photonView.RPC("RPC_SetClassSelected", RpcTarget.All, globalClassSelected);
                }
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void RPC_SetClassSelected(string classSelected)
    {
        globalClassSelected = classSelected;

        SetBools();

    }

    private void SetBools()
    {
        if (globalClassSelected == "Protector")
        {
            return;
        }
        if (globalClassSelected == "Occultist") 
        {
            occultist.isOccultist = true;
        }
        else if (globalClassSelected == "Drifter")
        {
            drifter.isDrifter = true;
        }
        else if (globalClassSelected == "Trapper")
        {
            return;
        }
    }
}
