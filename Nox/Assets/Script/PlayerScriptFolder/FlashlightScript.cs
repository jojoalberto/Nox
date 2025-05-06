using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightScript : MonoBehaviourPun
{
    public PlayerData playerData;
    public GameObject flashlight;
    public Transform flashlightPosition;
    public float flashlightRange = 10f;
    public LayerMask trapLayer;
    [Range(1f, 180f)] public float flashlightAngle = 60f;

    [SerializeField] private PhotonView photonView;
    private HashSet<MonsterTrapAlert> trapsInView = new();
    public PlayerHealth playerHealth;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerData.hasFlashlight = false;
        if (!playerData.hasFlashlight)
            flashlight.SetActive(false);
    }

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        HandleFlashlightToggle();
        if (flashlight.activeSelf)
            UpdateTrapVisibility();
    }

    private void HandleFlashlightToggle()
    {
        if (Input.GetKeyDown(KeyCode.F) && !playerHealth.isDead && playerData.hasFlashlight)
        {
            bool newState = !flashlight.activeSelf;
            flashlight.SetActive(newState);
            photonView.RPC("SyncFlashlightState", RpcTarget.Others, newState);

            if (!newState)
            {
                foreach (var trap in trapsInView)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        trap.UnregisterViewer(photonView.OwnerActorNr);
                    }
                    else
                    {
                        trap.photonView.RPC("RPC_RequestUnregisterViewer", RpcTarget.MasterClient, photonView.OwnerActorNr);
                    }
                }
                trapsInView.Clear();
            }

        }
    }


    [PunRPC]
    private void SyncFlashlightState(bool state)
    {
        flashlight.SetActive(state);
    }

    private void UpdateTrapVisibility()
    {
        Collider[] hits = Physics.OverlapSphere(flashlightPosition.position, flashlightRange, trapLayer);
        HashSet<MonsterTrapAlert> trapsNowVisible = new();

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out MonsterTrapAlert trap))
            {
                Vector3 dirToTrap = hit.transform.position - flashlightPosition.position;
                float angle = Vector3.Angle(flashlight.transform.forward, dirToTrap);

                if (angle <= flashlightAngle / 2f)
                {
                    // Line-of-sight check
                    if (Physics.Raycast(flashlightPosition.position, dirToTrap.normalized, out RaycastHit rayHit, flashlightRange))
                    {
                        if (rayHit.collider.gameObject != hit.gameObject)
                            continue; 

                        // It's visible and not blocked
                        trapsNowVisible.Add(trap);
                        if (!trapsInView.Contains(trap))
                        {
                            if (PhotonNetwork.IsMasterClient)
                            {
                                trap.RegisterViewer(photonView.OwnerActorNr);
                            }
                            else
                            {
                                trap.photonView.RPC("RPC_RequestRegisterViewer", RpcTarget.MasterClient, photonView.OwnerActorNr);
                            }
                        }
                    }
                }
            }
        }

        // Unregister no longer visible traps
        foreach (var trap in trapsInView)
        {
            if (!trapsNowVisible.Contains(trap))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    trap.UnregisterViewer(photonView.OwnerActorNr);
                }
                else
                {
                    trap.photonView.RPC("RPC_RequestUnregisterViewer", RpcTarget.MasterClient, photonView.OwnerActorNr);
                }
            }
        }

        trapsInView = trapsNowVisible;
    }

}
