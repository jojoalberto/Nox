using System;
using Photon.Pun;
using UnityEngine;

public class FlashlightScript : MonoBehaviourPun
{
    public PlayerData playerData;
    public GameObject flashlight;

    public Transform flashlightPosition;
    public float flashlightRange = 10f;
    public LayerMask trapLayer;
    [Range(1f, 180f)] public float flashlightAngle = 60f;

    private PhotonView PhotonView;

    private void Start()
    {
        if (!playerData.hasFlashlight)
        {
            flashlight.SetActive(false);
        }
    }

    private void Update()
    {
        UseFlashlight();
        CheckFlashlightHitTrap();
    }

    public void UseFlashlight()
    {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            if (playerData.hasFlashlight && Input.GetKeyDown(KeyCode.F))
            {
                flashlight.SetActive(!flashlight.activeSelf);
                photonView.RPC("SyncFlashlightState", RpcTarget.Others, flashlight.activeSelf);
            }
        }
    }

    [PunRPC]
    void SyncFlashlightState(bool state)
    {
        flashlight.SetActive(state);
    }

    private void CheckFlashlightHitTrap()
    {
        if (flashlight.activeSelf)
        {
            // Calculate the forward direction of the flashlight
            Vector3 forwardDirection = flashlight.transform.forward;

            // Raycast in the direction of the flashlight's beam to find traps within the cone of light
            Collider[] hits = Physics.OverlapSphere(flashlightPosition.position, flashlightRange, trapLayer);

            foreach (Collider hit in hits)
            {
                // Calculate the angle between the flashlight's forward direction and the trap position
                Vector3 directionToTrap = hit.transform.position - flashlightPosition.position;
                float angle = Vector3.Angle(forwardDirection, directionToTrap);

                // Check if the trap is within the flashlight's cone angle
                if (angle <= flashlightAngle / 2f)
                {
                    MonsterTrapAlert trap = hit.GetComponent<MonsterTrapAlert>();
                    if (trap != null)
                    {
                        // Set trap visibility to true
                        trap.SetTrapVisibility(true);
                    }
                }
                else
                {
                    // If the trap is outside the flashlight's cone, hide it
                    MonsterTrapAlert trap = hit.GetComponent<MonsterTrapAlert>();
                    if (trap != null)
                    {
                        trap.SetTrapVisibility(false);
                    }
                }
            }
        }
        else
        {
            // If the flashlight is off, make sure all visible traps are hidden
            HideAllTraps();
        }
    }

    private void HideAllTraps()
    {
        // Find all traps and set their visibility to false
        MonsterTrapAlert[] traps = FindObjectsOfType<MonsterTrapAlert>();
        foreach (MonsterTrapAlert trap in traps)
        {
            trap.SetTrapVisibility(false);
        }
    }
}