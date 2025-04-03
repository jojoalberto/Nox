using System.Collections;
using Photon.Pun;
using StarterAssets;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public class Drifter : MonoBehaviour
{
    public PhotonView photonView;
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
    private Renderer myRenderer;
    private Color originalColor;

    [Header("Ability 2 (Aethereus Vinculum)")]
    public string drifterAbility2Name = "Aethereus Vinculum";
    public string drifterAbility2Description = "Bind yourself and the enemy for a brief period";
    public float drifterAbility2CD = 15f;
    public float drifterAbility2Duration = 2f;
    public bool Ability2Ready = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine )
        {
            DemonTargetAI1 targetDemon = GetDemonInSight();
            HandleDemonHighlight(targetDemon);

            if (Input.GetKeyDown(KeyCode.Alpha1) && Ability1Ready)
            {
                StartCoroutine(ResetCooldownAbility1());
                StartCoroutine(ActivateAbility1());
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && Ability2Ready && targetDemon != null)
            {
                StartCoroutine(ResetCooldownAbility2());
                StartCoroutine(ActivateAbility2(targetDemon));
            }
        }
        
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
        isInvisible = true;
        yield return new WaitForSeconds(drifterAbility1Duration);
        isInvisible = false;
    }

    IEnumerator ActivateAbility2(DemonTargetAI1 targetDemon)
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }
        targetDemon.StartCoroutine(targetDemon.BindEffect(drifterAbility2Duration));

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
