using System.Collections;
using StarterAssets;
using UnityEngine;

public class Occultist : MonoBehaviour
{
    public OccultistCam occultistCam;

    public ThirdPersonController thirdPersonController;
    public Camera playerCamera;
    public float maxRaycastDistance = 20f;
    private ThirdPersonController lastHighlightedPlayer;

    [Header("Ability 1 (Auget Agilitas)")]
    public string occultistAbility1Name = "Auget Agilitas";
    public string occultistAbility1Description = "Increases Movement Speed of Allies";
    public float occultistAbility1Value = 1.5f;
    public float occultistAbility1CD = 10f;
    public float occultistAbility1Duration = 2f;
    public bool Ability1Ready = true;

    [Header("Ability 2 (Sanatio Tenebris)")]
    public string occultistAbility2Name = "Sanatio Tenebris";
    public string occultistAbility2Description = "Restores Health To Allies";
    public float occultistAbility2Value = 0.25f;
    public float occultistAbility2CD = 10f;
    public bool Ability2Ready = true;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        occultistCam.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        ThirdPersonController targetPlayer = GetPlayerInSight();
        HandlePlayerHighlight(targetPlayer);

        if (Input.GetKeyDown(KeyCode.Alpha1) && Ability1Ready)
        {
            StartCoroutine(ResetCooldownAbility1());
            if (targetPlayer != null)
            {
                StartCoroutine(ActivateAbility1(targetPlayer));
            }
            else
            {
                StartCoroutine(ActivateAbility1(thirdPersonController));
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && Ability1Ready)
        {
            StartCoroutine(ResetCooldownAbility2());
            if (targetPlayer != null)
            {
                ActivateAbility2(targetPlayer);
            }
            else
            {
                ActivateAbility2(thirdPersonController);
            }
        }
    }

    IEnumerator ResetCooldownAbility1()
    {
        Ability1Ready = false;
        yield return new WaitForSeconds(occultistAbility1CD);
        Ability1Ready = true;
    }

    IEnumerator ResetCooldownAbility2()
    {
        Ability2Ready = false;
        yield return new WaitForSeconds(occultistAbility2CD);
        Ability2Ready = true;
    }

    IEnumerator ActivateAbility1(ThirdPersonController target)
    {
        target.SetSpeedMultiplier(occultistAbility1Value);
        yield return new WaitForSeconds(occultistAbility1Duration);
        target.SetSpeedMultiplier(1f);
    }

    private void ActivateAbility2(ThirdPersonController target)
    {
        target.GetComponent<PlayerHealth>().RestoreHealthPercent(occultistAbility2Value);
    }

    ThirdPersonController GetPlayerInSight()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastDistance))
        {
            ThirdPersonController targetPlayer = hit.collider.GetComponent<ThirdPersonController>();
            if (targetPlayer != null && targetPlayer != thirdPersonController)
            {
                return targetPlayer;
            }
        }

        return null;
    }

    void HandlePlayerHighlight(ThirdPersonController targetPlayer)
    {
        if (targetPlayer != lastHighlightedPlayer)
        {
            if (lastHighlightedPlayer != null)
            {
                lastHighlightedPlayer.GetComponent<Outline>().enabled = false;
            }

            if (targetPlayer != null)
            {
                targetPlayer.GetComponent<Outline>().enabled = true;
            }

            lastHighlightedPlayer = targetPlayer;
        }
    }
}
