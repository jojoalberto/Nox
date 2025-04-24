using Photon.Pun;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonsterTrapAlert : MonoBehaviourPun
{
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

    private readonly HashSet<int> viewers = new(); 
    private bool isVisible = false;

    private void Awake()
    {
        if (trapRenderer == null)
            trapRenderer = GetComponentInChildren<Renderer>();
        if (trapRenderer != null)
            trapRenderer.enabled = isVisible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isSoundTrap)
        {
            var enemy = GameObject.FindGameObjectWithTag("Enemy");
            var demonAI = enemy?.GetComponent<DemonTargetAI1>();
            demonAI?.RequestSoundAlert(gameObject);
            onSoundTrapTrigger?.Invoke();
        }

        if (isDamageTrap && other.TryGetComponent<PlayerHealth>(out var playerHealth))
            playerHealth.TakeDamage(damage);

        if (isSlowTrap && other.TryGetComponent<ThirdPersonController>(out var controller))
        {
            controller.ApplySlow(slowMultiplier, slowDuration);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void RegisterViewer(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (viewers.Add(actorNumber) && viewers.Count == 1)
        {
            photonView.RPC(nameof(RPC_SetTrapVisibility), RpcTarget.All, true);
        }
    }

    public void UnregisterViewer(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (viewers.Remove(actorNumber) && viewers.Count == 0)
        {
            photonView.RPC(nameof(RPC_SetTrapVisibility), RpcTarget.All, false);
        }
    }


    [PunRPC]
    private void RPC_SetTrapVisibility(bool visible)
    {
        isVisible = visible;
        if (trapRenderer != null)
            trapRenderer.enabled = visible;
    }

    [PunRPC]
    private void RPC_RequestRegisterViewer(int actorNumber)
    {
        RegisterViewer(actorNumber);
    }

    [PunRPC]
    private void RPC_RequestUnregisterViewer(int actorNumber)
    {
        UnregisterViewer(actorNumber);
    }
}
