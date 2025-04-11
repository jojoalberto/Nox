using Photon.Pun;
using UnityEngine;

public class TrapperTrap : MonoBehaviour
{

    public float duration = 2f;
    private bool hasTriggered = false;
    public PhotonView photonView;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FreezeDemon(DemonTargetAI1 demonTargetAI1)
    {
        demonTargetAI1.RequestFreeze(duration);
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject + "Entered");
        if (!hasTriggered)
        {
            DemonTargetAI1 demon = other.gameObject.GetComponent<DemonTargetAI1>();
            if (demon != null)
            {
                hasTriggered = true;
                FreezeDemon(demon);
                photonView.RPC("DestroySelf", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
