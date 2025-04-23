using System.Collections;
using Photon.Pun;
using UnityEngine;

public class TrapperGrenade : MonoBehaviour
{
    public PhotonView photonView;
    private Rigidbody rb;

    public float explosionRadius = 15f;
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;
    public LayerMask enemyLayer;

    private bool hasExploded = false;

    public float pitchMultiplier = 8f; 
    public float rotationSmoothness = 10f;
    public float rollSpeed = 360f; 

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (rb == null) return;

        float verticalVelocity = rb.linearVelocity.y;
        float forwardSpeed = rb.linearVelocity.magnitude;

        // Make pitch more dramatic and snappy
        float targetPitch = Mathf.Clamp(-verticalVelocity * pitchMultiplier, -70f, 70f);

        // Optionally add a constant spin around Z (forward roll)
        float roll = Time.time * rollSpeed;

        // Create a target rotation
        Quaternion targetRotation = Quaternion.Euler(targetPitch, targetPitch, roll);

        // Smoothly interpolate to that rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

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
