using System;
using System.Collections;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Trapper : MonoBehaviour
{
    public PhotonView photonView;
    public bool isTrapper = false;

    [SerializeField] private int pickups = 0;

    private Camera mainCamera;
    private int originalCullingMask;
    private int trapperLayer;

    [Header("Ability 1 (Hibernus Impedimentum)")]
    public string trapperAbility1Name = "Hibernus Impedimentum";
    public string trapperAbility1Description = "Throw a slowing grenade";
    public float trapperAbility1SlowValue = 0.5f;
    public float trapperAbility1RadiusValue = 15f;
    public float trapperAbility1DurationValue = 2f;
    public float trapperAbility1CD = 10f;
    public int trapperAbility1Cost = 1;
    public bool Ability1Ready = true;

    [Header("References For Grenade")]
    public Transform cam;
    public Transform grenadeSpawnPosition;
    public GameObject grenade;

    [Header("Throwing For Grenade")]
    public float throwForce;
    public float throwUpwardForce;


    [Header("Ability 2 (Gelu Immobilis)")]
    public string trapperAbility2Name = "Gelu Immobilis";
    public string trapperAbility2Description = "Place a trap that freezes the enemy for a few seconds";
    public float trapperAbility2CD = 10f;
    public float trapperAbility2Duration = 2f;
    public int trapperAbility2Cost = 3;
    public bool Ability2Ready = true;

    [Header("References For Trap")]
    public GameObject freezeTrap;

    public PlayerScriptBehaviour playerScriptBehaviour;
    public TMP_Text passiveText;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] abilityAduioClips;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerScriptBehaviour = GetComponent<PlayerScriptBehaviour>();
        if (isTrapper)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCullingMask = mainCamera.cullingMask;
                trapperLayer = LayerMask.NameToLayer("TrapperPickup");
            }
        }
    }

    void Update()
    {
        if (photonView.IsMine && isTrapper && (pickups >= trapperAbility1Cost))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && Ability1Ready)
            {
                photonView.RPC("RPC_ResetCooldownTrapperAbility1", RpcTarget.All);
                ActivateAbility1();
                updateAbility1UI();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && Ability2Ready && (pickups >= trapperAbility2Cost))
            {
                photonView.RPC("RPC_ResetCooldownTrapperAbility2", RpcTarget.All);
                ActivateAbility2();
                updateAbility2UI();
            }
        }
    }

    private void updateAbility1UI()
    {
        playerScriptBehaviour.SetAbility1UICD(trapperAbility1CD);
    }

    private void updateAbility2UI()
    {
        playerScriptBehaviour.SetAbility2UICD(trapperAbility2CD);
    }

    void OnEnable()
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask |= (1 << trapperLayer);
        }
    }

    void OnDisable()
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask = originalCullingMask;
        }
    }

    public void IncrementPickup()
    {
        photonView.RPC("RPC_IncrementPickup", RpcTarget.All);
    }

    [PunRPC]
    void RPC_IncrementPickup()
    {
        pickups++;
        if (photonView.IsMine)
        {
            passiveText.text = "x" + pickups.ToString();
        }
    }

    [PunRPC]
    void RPC_DecrementPickup(int amount)
    {
        pickups = pickups - amount;
        if (photonView.IsMine)
        {
            passiveText.text = "x" + pickups.ToString();
        }
    }

    [PunRPC]
    void RPC_ResetCooldownTrapperAbility1()
    {
        StartCoroutine(ResetCooldownAbility1());
    }

    [PunRPC]
    void RPC_ResetCooldownTrapperAbility2()
    {
        StartCoroutine(ResetCooldownAbility2());
    }

    IEnumerator ResetCooldownAbility1()
    {
        Ability1Ready = false;
        yield return new WaitForSeconds(trapperAbility1CD);
        Ability1Ready = true;
    }

    IEnumerator ResetCooldownAbility2()
    {
        Ability2Ready = false;
        yield return new WaitForSeconds(trapperAbility2CD);
        Ability2Ready = true;
    }

    private void ActivateAbility1()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayTrapperAbilityAudio", RpcTarget.All, 0);
        }

        photonView.RPC("RPC_DecrementPickup", RpcTarget.All, trapperAbility1Cost);
        GameObject instantiatedGrenade = PhotonNetwork.Instantiate(grenade.name, grenadeSpawnPosition.position, cam.rotation);

        Rigidbody rb = instantiatedGrenade.GetComponent<Rigidbody>();

        Vector3 forceDirection = cam.transform.forward;
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 500f))
        {
            forceDirection = (hit.point - grenadeSpawnPosition.position).normalized;
        }

        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
        rb.AddForce(forceToAdd, ForceMode.Impulse);

        TrapperGrenade trapperGrenade = instantiatedGrenade.GetComponent<TrapperGrenade>();
        trapperGrenade.explosionRadius = trapperAbility1RadiusValue;
        trapperGrenade.slowAmount = trapperAbility1SlowValue;
        trapperGrenade.slowDuration = trapperAbility1DurationValue;
    }

    private void ActivateAbility2()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayTrapperAbilityAudio", RpcTarget.All, 1);
        }

        photonView.RPC("RPC_DecrementPickup", RpcTarget.All, trapperAbility2Cost);


        float trapDistance = 1f;
        Vector3 forwardDirection = transform.forward;
        Vector3 tentativePosition = transform.position + forwardDirection * trapDistance;

        Vector3 spawnPosition;
        RaycastHit hit;
        if (Physics.Raycast(tentativePosition + Vector3.up, Vector3.down, out hit, 10f, LayerMask.GetMask("Default")))
        {
            spawnPosition = hit.point;
        }
        else
        {
            spawnPosition = tentativePosition;
        }
 
        GameObject trapInstance = PhotonNetwork.Instantiate(freezeTrap.name, spawnPosition, Quaternion.identity);

        TrapperTrap trapperTrap = trapInstance.GetComponent<TrapperTrap>();
        if (trapperTrap != null)
        {
            trapperTrap.duration = trapperAbility2Duration;
        }

    }

    [PunRPC]
    public void RPC_PlayTrapperAbilityAudio(int clipIndex)
    {
        if (audioSource != null && abilityAduioClips[clipIndex] != null)
        {
            audioSource.PlayOneShot(abilityAduioClips[clipIndex]);
        }
    }

}
