using System.Collections;
using Photon.Pun;
using StarterAssets;
using UnityEngine;
using UnityEngine.Audio;

public class Occultist : MonoBehaviour
{
    public bool isOccultist = false;
    public OccultistCam occultistCam;

    public ThirdPersonController thirdPersonController;
    public Camera playerCamera;
    public float maxRaycastDistance = 8f;
    private ThirdPersonController lastHighlightedPlayer;

    public PhotonView photonView;

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

    public PlayerScriptBehaviour playerScriptBehaviour;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] abilityAduioClips;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerScriptBehaviour = GetComponent<PlayerScriptBehaviour>();

        if (photonView.IsMine && isOccultist)
        {
            occultistCam.enabled = true;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && isOccultist)
        {
            ThirdPersonController targetPlayer = GetPlayerInSight();
            HandlePlayerHighlight(targetPlayer);

            if (Input.GetKeyDown(KeyCode.Alpha1) && Ability1Ready)
            {
                photonView.RPC("RPC_ResetCooldownOccultistAbility1", RpcTarget.All);
                if (targetPlayer != null)
                {
                    StartCoroutine(ActivateAbility1(targetPlayer));
                }
                else
                {
                    StartCoroutine(ActivateAbility1(thirdPersonController));
                }
                updateAbility1UI();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && Ability2Ready)
            {
                photonView.RPC("RPC_ResetCooldownOccultistAbility2", RpcTarget.All);
                if (targetPlayer != null)
                {
                    ActivateAbility2(targetPlayer);
                }
                else
                {
                    ActivateAbility2(thirdPersonController);
                }
                updateAbility2UI();
            }
        }

        
    }

    private void updateAbility1UI()
    {
        playerScriptBehaviour.SetAbility1UICD(occultistAbility1CD);
    }

    private void updateAbility2UI()
    {
        playerScriptBehaviour.SetAbility2UICD(occultistAbility2CD);
    }

    [PunRPC]
    void RPC_ResetCooldownOccultistAbility1()
    {
        StartCoroutine(ResetCooldownAbility1());
    }

    [PunRPC]
    void RPC_ResetCooldownOccultistAbility2()
    {
        StartCoroutine(ResetCooldownAbility2());
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

    [PunRPC]
    void RPC_ActivateAbility1(int targetPlayerID)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayOccultistAbilityAudio", RpcTarget.All, 0);
        }

        GameObject targetObject = PhotonView.Find(targetPlayerID).gameObject;
        ThirdPersonController target = targetObject.GetComponent<ThirdPersonController>();

        StartCoroutine(ActivateAbility1(target));
    }

    IEnumerator ActivateAbility1(ThirdPersonController target)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayOccultistAbilityAudio", RpcTarget.All, 0);
        }

        target.SetSpeedMultiplier(occultistAbility1Value);
        yield return new WaitForSeconds(occultistAbility1Duration);
        target.SetSpeedMultiplier(1f);
    }

    void UseAbility1(ThirdPersonController target)
    {
        photonView.RPC("RPC_ActivateAbility1", RpcTarget.All, target.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void RPC_ActivateAbility2(int targetPlayerID)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayOccultistAbilityAudio", RpcTarget.All, 1);
        }

        GameObject targetObject = PhotonView.Find(targetPlayerID)?.gameObject;
        if (targetObject == null) return;

        PlayerHealth targetHealth = targetObject.GetComponent<PlayerHealth>();
        if (targetHealth != null)
        {
            targetHealth.photonView.RPC("RPC_RestoreHealthPercent", RpcTarget.MasterClient, occultistAbility2Value);
        }
    }

    void ActivateAbility2(ThirdPersonController target)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayOccultistAbilityAudio", RpcTarget.All, 1);
        }

        if (target == null) return;

        PlayerHealth playerhealth = target.GetComponent<PlayerHealth>();

        playerhealth.RestoreHealing(occultistAbility2Value);
    }


    ThirdPersonController GetPlayerInSight()
    {
        if (!photonView.IsMine) return null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRaycastDistance);

        foreach (RaycastHit hit in hits)
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

    [PunRPC]
    public void RPC_PlayOccultistAbilityAudio(int clipIndex)
    {
        if (audioSource != null && abilityAduioClips[clipIndex] != null)
        {
            audioSource.PlayOneShot(abilityAduioClips[clipIndex]);
        }

    }

}
