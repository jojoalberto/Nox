using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using StarterAssets;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public class Drifter : MonoBehaviour
{
    public PhotonView photonView;
    public bool isDrifter = false;

    public List<SkinnedMeshMaterials> skinnedMeshMaterials;


    private ThirdPersonController thirdPersonController;
    public Camera playerCamera;
    public float maxRaycastDistance = 20f;
    private DemonTargetAI1 lastHighlightedDemon;

    [Header("Ability 1 (Umbra Fugitiva)")]
    public string drifterAbility1Name = "Umbra Fugitiva";
    public string drifterAbility1Description = "Become invisible";
    public float drifterAbility1Value = 1.5f;
    public float drifterAbility1CD = 30f;
    public float drifterAbility1Duration = 10f;
    public bool Ability1Ready = true;
    public bool isInvisible = false;


    [Header("Ability 2 (Aethereus Vinculum)")]
    public string drifterAbility2Name = "Aethereus Vinculum";
    public string drifterAbility2Description = "Bind yourself and the enemy for a brief period";
    public float drifterAbility2CD = 15f;
    public float drifterAbility2Duration = 2f;
    public bool Ability2Ready = true;

    public PlayerScriptBehaviour playerScriptBehaviour;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerScriptBehaviour = GetComponent<PlayerScriptBehaviour>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine && isDrifter)
        {
            DemonTargetAI1 targetDemon = GetDemonInSight();
            HandleDemonHighlight(targetDemon);

            if (Input.GetKeyDown(KeyCode.Alpha1) && Ability1Ready)
            {
                photonView.RPC("RPC_ResetCooldownDrifterAbility1", RpcTarget.All);
                StartCoroutine(ActivateAbility1());
                updateAbility1UI();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && Ability2Ready && targetDemon != null)
            {
                photonView.RPC("RPC_ResetCooldownDrifterAbility2", RpcTarget.All);
                StartCoroutine(ActivateAbility2(targetDemon));
                updateAbility2UI();
            }
        }
        
    }

    private void updateAbility1UI()
    {
        playerScriptBehaviour.SetAbility1UICD(drifterAbility1CD);
    }

    private void updateAbility2UI()
    {
        playerScriptBehaviour.SetAbility2UICD(drifterAbility2CD);
    }

    [PunRPC]
    void RPC_ResetCooldownDrifterAbility1()
    {
        StartCoroutine(ResetCooldownAbility1());
    }

    [PunRPC]
    void RPC_ResetCooldownDrifterAbility2()
    {
        StartCoroutine(ResetCooldownAbility2());
    }

    IEnumerator ResetCooldownAbility1()
    {
        Ability1Ready = false;
        yield return new WaitForSeconds(drifterAbility1CD);
        Ability1Ready = true;
    }

    IEnumerator ResetCooldownAbility2()
    {
        Ability2Ready = false;
        yield return new WaitForSeconds(drifterAbility2CD);
        Ability2Ready = true;
    }

    IEnumerator ActivateAbility1()
    {
        photonView.RPC("RPC_ToggleInvisibility", RpcTarget.All, true);

        yield return new WaitForSeconds(drifterAbility1Duration);

        photonView.RPC("RPC_ToggleInvisibility", RpcTarget.All, false);
    }

    [PunRPC]
    void RPC_ToggleInvisibility(bool invisibile)
    {
        isInvisible = invisibile;

        for (int i = 0; i < skinnedMeshMaterials.Count; i++)
        {
            if (skinnedMeshMaterials[i].renderer != null)
            {
                // Switch between invisibility materials and original materials
                skinnedMeshMaterials[i].renderer.materials = isInvisible ? skinnedMeshMaterials[i].invisMaterials : skinnedMeshMaterials[i].originalMaterials;
            }
        }
    }

    IEnumerator ActivateAbility2(DemonTargetAI1 targetDemon)
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }

        targetDemon.BindDemon(drifterAbility2Duration);

        yield return new WaitForSeconds(drifterAbility2Duration);

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }

    }

    private DemonTargetAI1 GetDemonInSight()
    {
        if (!photonView.IsMine) return null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRaycastDistance);

        foreach (RaycastHit hit in hits)
        {
            DemonTargetAI1 targetDemon = hit.collider.GetComponent<DemonTargetAI1>();
            if (targetDemon != null)
            {
                return targetDemon;
            }
        }

        return null;
    }

    void HandleDemonHighlight(DemonTargetAI1 targetDemon)
    {
        if (targetDemon != lastHighlightedDemon)
        {
            if (lastHighlightedDemon != null)
            {
                lastHighlightedDemon.GetComponent<Outline>().enabled = false;
            }
            
        }
        if(targetDemon != null)
        {
            targetDemon.GetComponent<Outline>().enabled = true;
        }
        lastHighlightedDemon = targetDemon;
    }
}
