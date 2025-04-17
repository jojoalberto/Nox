using Photon.Pun;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;

public class MonsterTrapAlert : MonoBehaviourPun
{
    private PhotonView photonView;

    [Header("Trap Kind")]
    [SerializeField] private bool isSoundTrap;
    [SerializeField] private bool isDamageTrap;
    [SerializeField] private bool isSlowTrap;

    [Header("Slow Trap Settings")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 5f;

    [Header("Damage Trap Settings")]
    [SerializeField] private int damage = 10;

    [Header("Visibility Settings")]
    [SerializeField] private Renderer trapRenderer;
    [Tooltip("Angle within which the trap is considered visible (in degrees)")]
    public float visibilityAngle = 60f;

    [SerializeField] private UnityEvent onSoundTrapTrigger;
    private bool isCurrentlyVisible = false;

    private void Start()
    {
        
    }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (trapRenderer == null)
            trapRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isSoundTrap)
        {
            var enemy = GameObject.FindGameObjectWithTag("Enemy");
            var demonAI = enemy?.GetComponent<DemonTargetAI1>();
            if (demonAI != null)
            {
                demonAI.RequestSoundAlert(gameObject);
                onSoundTrapTrigger?.Invoke();
            }
        }

        if (isDamageTrap && other.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            playerHealth.TakeDamage(damage);
        }

        if (isSlowTrap && other.TryGetComponent<ThirdPersonController>(out var controller))
        {
            controller.ApplySlow(slowMultiplier, slowDuration);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void SetTrapVisibility(bool isVisible)
    {
        if (isVisible == isCurrentlyVisible) return;
        isCurrentlyVisible = isVisible;
        photonView.RPC("RPC_SetTrapVisibility", RpcTarget.All, isVisible);
       
    }

    [PunRPC]
    private void RPC_SetTrapVisibility(bool isVisible)
    {
        if (trapRenderer != null)
            trapRenderer.enabled = isVisible;
    }
}