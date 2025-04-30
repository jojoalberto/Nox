using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class Protector : MonoBehaviour
{
    [SerializeField] private float interactDistance;
    public UnityEvent onDestroyObject;
    public LayerMask destroyableLayer;
    public PhotonView photonView;
    private GameObject targetObject;
    private PlayerInteraction playerInteraction;
    public bool isProtector = false;

    public PlayerScriptBehaviour playerScriptBehaviour;

    [Header("Ability 1 (Salus Brevis)")]
    public string protectorAbility1Name = "Salus Brevis";
    public string protectorAbility1Description = "Give nearby characters temporary health";
    public float ability1Radius = 20f;
    public float protectorAbility1Value = 10f;
    public float protectorAbility1CD = 15f;
    public float protectorAbility1Duration = 10f;
    public bool Ability1Ready = true;

    [Header("Ability 2 (Audacia Mortalis)")]
    public string protectorAbility2Name = "Audacia Mortalis";
    public string protectorAbility2Description = "Taunts nearby enemy";
    public float ability2Radius = 20f;
    public float protectorAbility2CD = 15f;
    public float protectorAbility2Duration = 10f;
    public bool Ability2Ready = true;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] abilityAduioClips;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerScriptBehaviour = GetComponent<PlayerScriptBehaviour>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && isProtector)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && Ability1Ready)
            {
                photonView.RPC("RPC_ResetCooldownProtectorAbility1", RpcTarget.All);
                StartCoroutine(ActivateAbility1());
                updateAbility1UI();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && Ability2Ready)
            {
                photonView.RPC("RPC_ResetCooldownProtectorAbility2", RpcTarget.All);
                StartCoroutine(ActivateAbility2());
                updateAbility2UI();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("pressing E");
                DestroyObject();
            }
        }
    }
    [PunRPC]
    public void DestroyObjectRPC(GameObject tarObj)
    {
        Debug.Log("protector is hitting deactivation");
        tarObj.SetActive(false);
        onDestroyObject.Invoke();
    }

    private void updateAbility1UI()
    {
        playerScriptBehaviour.SetAbility1UICD(protectorAbility1CD);
    }

    private void updateAbility2UI()
    {
        playerScriptBehaviour.SetAbility2UICD(protectorAbility2CD);
    }

    [PunRPC]
    void RPC_ResetCooldownProtectorAbility1()
    {
        StartCoroutine(ResetCooldownAbility1());
    }

    [PunRPC]
    void RPC_ResetCooldownProtectorAbility2()
    {
        StartCoroutine(ResetCooldownAbility2());
    }

    IEnumerator ResetCooldownAbility1()
    {
        Ability1Ready = false;
        yield return new WaitForSeconds(protectorAbility1CD);
        Ability1Ready = true;
    }

    IEnumerator ResetCooldownAbility2()
    {
        Ability2Ready = false;
        yield return new WaitForSeconds(protectorAbility2CD);
        Ability2Ready = true;
    }

    IEnumerator ActivateAbility1()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_PlayProtectorAbilityAudio", RpcTarget.All, 0);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ability1Radius);
        foreach (Collider hit in hitColliders)
        {
            GameObject target = hit.gameObject;
            if (target.CompareTag("Player"))
            {
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                PhotonView targetPhotonView = target.GetComponent<PhotonView>();

                if (playerHealth != null && targetPhotonView != null)
                {
                    targetPhotonView.RPC("RPC_AddTemporaryHealth", RpcTarget.All, protectorAbility1Value);
                }
            }
        }

        yield return null;
    }

    IEnumerator ActivateAbility2()
    {
        if(photonView.IsMine)
        {
            photonView.RPC("RPC_PlayProtectorAbilityAudio", RpcTarget.All, 1);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ability1Radius);
        foreach (Collider hit in hitColliders)
        {
            GameObject target = hit.gameObject;
            if (target.CompareTag("Enemy"))
            {
                PhotonView demonPV = target.GetComponent<PhotonView>();
                if (demonPV != null)
                {
                    demonPV.RPC("RPC_Taunt", RpcTarget.MasterClient, transform.GetComponent<PhotonView>().ViewID, protectorAbility2Duration);
                }
            }
        }
        yield return null;
    }
    public void DestroyObject()
    {
        if (photonView.IsMine && isProtector)
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactDistance, destroyableLayer))
            {
                GameObject hitObj = hit.collider.gameObject;
                targetObject = hitObj;
                Debug.Log("protector is hitting " + hitObj);
                photonView.RPC("DestroyObjectRPC", RpcTarget.All, targetObject);
            }
        }
    }

    [PunRPC]
    public void RPC_PlayProtectorAbilityAudio(int clipIndex)
    {
        if (audioSource != null && abilityAduioClips[clipIndex] != null)
        {
            audioSource.PlayOneShot(abilityAduioClips[clipIndex]);
        }
        
    }
}
