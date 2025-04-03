using Photon.Pun;
using UnityEngine;

public class PlayerScriptBehaviour : MonoBehaviour
{

    public PlayerData playerData;
    public Trapper trapper;
    public Occultist occultist;
    public Drifter drifter;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       


        if(playerData == null)
        {
            Debug.Log("No player data script found");
        }
        else
        {
            string classSelected = playerData.getClassSelected();
            if (classSelected != null)
            {
                if(classSelected == "Drifter")
                {
                    drifter.enabled = true;
                }
                if(classSelected == "Occultist")
                {
                    occultist.enabled = true;
                }
                else if(classSelected == "Trapper")
                {
                    trapper.enabled = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
