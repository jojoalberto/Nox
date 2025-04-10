using System;
using System.Collections;
using Photon.Pun;
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
    public bool Ability1Ready = true;

    [Header("References For Grenade")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    [Header("Throwing For Grenade")]
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;


    [Header("Ability 2 (Gelu Immobilis)")]
    public string trapperAbility2Name = "Gelu Immobilis";
    public string trapperAbility2Description = "Place a trap that freezes the enemy for a few seconds";
    public float trapperAbility2CD = 10f;
    public float trapperAbility2Duration = 2f;
    public bool Ability2Ready = true;



    void Start()
    {
        photonView = GetComponent<PhotonView>();
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
        if (photonView.IsMine && isTrapper && (pickups > 0))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && Ability1Ready)
            {
                photonView.RPC("RPC_ResetCooldownTrapperAbility1", RpcTarget.All);
                ActivateAbility1();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && Ability2Ready && (pickups > 2))
            {
                photonView.RPC("RPC_ResetCooldownTrapperAbility2", RpcTarget.All);
                StartCoroutine(ActivateAbility2());
            }
        }
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
        pickups++;
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
        pickups--;
        GameObject grenade = PhotonNetwork.Instantiate(objectToThrow.name, attackPoint.position, cam.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        Vector3 forceDirection = cam.transform.forward;
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
        rb.AddForce(forceToAdd, ForceMode.Impulse);

        TrapperGrenade trapperGrenade = grenade.GetComponent<TrapperGrenade>();
        trapperGrenade.explosionRadius = trapperAbility1RadiusValue;
        trapperGrenade.slowAmount = trapperAbility1SlowValue;
        trapperGrenade.slowDuration = trapperAbility1DurationValue;
    }


    [PunRPC]
    private void ToggleTrapperGrenadeActive(GameObject grenade)
    {
        grenade.SetActive(!grenade.activeSelf);
    }

    IEnumerator ActivateAbility2()
    {
        pickups = pickups - 3;
        yield return null;
    }
}
