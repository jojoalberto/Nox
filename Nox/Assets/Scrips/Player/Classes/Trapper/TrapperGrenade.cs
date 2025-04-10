using System.Collections;
using Photon.Pun;
using UnityEngine;

public class TrapperGrenade : MonoBehaviour
{
    public PhotonView photonView;

    public float explosionRadius = 15f;
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;
    public LayerMask enemyLayer;

    private bool hasExploded = false;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || hasExploded) return;

        hasExploded = true;

        Explode();
    }

    void Explode()
    {
        // Find nearby objects to slow
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);

        foreach (var enemy in enemies)
        {
            DemonTargetAI1 demonTargetAI1 = enemy.GetComponent<DemonTargetAI1>();
            if (demonTargetAI1 != null)
            {
                demonTargetAI1.RequestSlow(slowAmount, slowDuration);
                Debug.Log("ENEMY HIT AND SLOWED");
            }
        }

        photonView.RPC("DestroySelf", RpcTarget.All);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    [PunRPC]
    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
