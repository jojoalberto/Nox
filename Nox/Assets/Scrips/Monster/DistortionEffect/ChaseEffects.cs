using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Unity.Burst.Intrinsics.Arm;

public class ChaseEffects : MonoBehaviour
{

    public Transform demonTransform;

    [Header("Chromatic Aberration")]
    public float chromaticFadeSpeed = 2f;
    public float targetChromatic = 0.6f;

    private ChromaticAberration chromaticAberration;

    private bool isChasing = false;

    [SerializeField] private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Volume volume = GetComponent<Volume>();
        if (volume && volume.profile)
        {
            volume.profile.TryGet(out chromaticAberration);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (chromaticAberration == null)
            return;

        // Chromatic Aberration  during chase
        float targetCA = isChasing ? targetChromatic : 0f;
        chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, targetCA, Time.deltaTime * chromaticFadeSpeed);
    }
    public void StartChaseEffect()
    {
        photonView.RPC("RPC_StartChaseEffect", RpcTarget.All);
    }

    public void StopChaseEffect()
    {
        photonView.RPC("RPC_StopChaseEffect", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartChaseEffect()
    {
        isChasing = true;
    }

    [PunRPC]
    private void RPC_StopChaseEffect()
    {
        isChasing = false;
    }


}
