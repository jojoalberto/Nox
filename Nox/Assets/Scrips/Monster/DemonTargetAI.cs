using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Photon.Realtime;
using Photon.Pun;

public class DemonTargetAI1 : MonoBehaviour
{
    [Header("General Settings")]
    public Transform[] targetLocations;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    public float[] targetWeights;
    public int currentTargetIndex = -1;
    public List<Transform> players = new List<Transform>();

    [Header("Chase Settings")]
    public float defaultSpeed = 3f;
    public float tiredSpeed = 5f;
    public float chaseSpeed = 5.5f;
    private float interestDuration = 10f;
    private float extendedChaseDuration = 10f;
    public float gracePeriod = 5f;
    public float chaseDistance = 10f;
    private Coroutine chaseCoroutine;
    [SerializeField] private LayerMask lineOfSightLayers;
    public Transform currentTarget;

    [Header("Debug Settings")]
    public string[] debugMessages = { "", "", "" };

    [Header("States")]
    public bool isChasingPlayer = false;
    public bool isAttacking = false;

    [Header("Attack Settings")]
    public float attackRange = 5f;
    public int damageAmount = 1;
    public float postAttackIdleTime = 4f;

    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        targetWeights = Enumerable.Repeat(1f, targetLocations.Length).ToArray();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitForPlayersToInstantiate());
        }
    }

    IEnumerator WaitForPlayersToInstantiate()
    {
        while (players.Count < PhotonNetwork.PlayerList.Length)
        {
            UpdatePlayerList();
            yield return new WaitForSeconds(0.5f);
        }

        PickNewTarget();
    }

    void UpdatePlayerList()
    {
        players.Clear();
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players.AddRange(playerObjects.Select(go => go.transform));
        SetClosestPlayer();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        SetClosestPlayer();
        debugMessages[0] = "Closest: " + (currentTarget != null ? currentTarget.name : "None");

        if (!isChasingPlayer && currentTarget != null && HasLineOfSightToPlayer(currentTarget))
        {
            if (chaseCoroutine == null)
            {
                chaseCoroutine = StartCoroutine(ChasePlayer());
            }
        }

        if (isChasingPlayer && !isAttacking && currentTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToPlayer <= attackRange)
            {
                // Stop chasing when attacking starts
                if (chaseCoroutine != null)
                {
                    StopCoroutine(chaseCoroutine);
                    chaseCoroutine = null;
                }
                StartCoroutine(AttackPlayer());
            }
        }

        if (!isChasingPlayer && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            ReduceTargetWeight(currentTargetIndex);
            PickNewTarget();
        }

        UpdateAnimation();

    }

    void PickNewTarget()
    {
        if (targetLocations.Length == 0) return;

        int newIndex = GetWeightedRandomIndex();
        currentTargetIndex = newIndex;
        navMeshAgent.SetDestination(targetLocations[newIndex].position);
        navMeshAgent.speed = defaultSpeed;

        debugMessages[1] = targetLocations[newIndex].ToString();
    }

    int GetWeightedRandomIndex()
    {
        float totalWeight = targetWeights.Sum();
        float randomPoint = Random.value * totalWeight;

        for (int i = 0; i < targetWeights.Length; i++)
        {
            if (randomPoint < targetWeights[i]) return i;
            randomPoint -= targetWeights[i];
        }
        return 0;
    }

    void ReduceTargetWeight(int index)
    {
        if (index >= 0 && index < targetWeights.Length)
        {
            targetWeights[index] *= 0.5f;
        }

        ResetWeightsIfNeeded();
    }

    void ResetWeightsIfNeeded()
    {
        if (targetWeights.All(w => w < 0.2f))
        {
            for (int i = 0; i < targetWeights.Length; i++)
            {
                targetWeights[i] = 1f;
            }
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            float speed = navMeshAgent.velocity.magnitude;

            // Only MasterClient should send the RPC
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SyncAnimation", RpcTarget.All, speed);
            }
        }
    }

    [PunRPC]
    void SyncAnimation(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    IEnumerator ChasePlayer()
    {
        isChasingPlayer = true;
        navMeshAgent.speed = chaseSpeed;
        float chaseTimer = interestDuration;
        float currentGracePeriod = gracePeriod;

        while (chaseTimer > 0f)
        {
            SetClosestPlayer();
            if (currentTarget == null)
            {
                break; // Exit chase if no target
            }
            debugMessages[1] = "Chasing: " + currentTarget.name;
            navMeshAgent.SetDestination(currentTarget.position);

            chaseTimer -= Time.deltaTime;
            yield return null;
        }

        if (currentTarget != null)
        {
            navMeshAgent.speed = tiredSpeed;
            chaseTimer = extendedChaseDuration;

            while (chaseTimer > 0f || currentGracePeriod > 0f)
            {
                SetClosestPlayer();
                if (currentTarget == null)
                {
                    break; // Exit if no target
                }

                if (HasLineOfSightToPlayer(currentTarget))
                {
                    currentGracePeriod = gracePeriod;
                    chaseTimer -= Time.deltaTime;
                }
                else
                {
                    if (currentGracePeriod > 0f)
                    {
                        currentGracePeriod -= Time.deltaTime;
                    }
                    chaseTimer -= Time.deltaTime;
                }

                navMeshAgent.SetDestination(currentTarget.position);
                yield return null;
            }
        }

        // Resume normal behavior
        isChasingPlayer = false;
        navMeshAgent.speed = defaultSpeed;
        chaseCoroutine = null;
        PickNewTarget();
    }

    bool HasLineOfSightToPlayer(Transform player)
    {
        if (player == null) return false;

        Vector3 aiEyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 playerEyePosition = player.position + Vector3.up * 1.5f;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (playerEyePosition - aiEyePosition).normalized;

        bool isInFront = Vector3.Angle(transform.forward, directionToPlayer) <= 90f;
        bool isCloseEnough = distanceToPlayer <= chaseDistance;

        if (!isCloseEnough && !isInFront) return false;

        Debug.DrawLine(aiEyePosition, playerEyePosition, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Linecast(aiEyePosition, playerEyePosition, out hit, lineOfSightLayers))
        {
            debugMessages[2] = hit.collider.name;
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    public void SetClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        currentTarget = null; // Reset current target

        foreach (Transform player in players)
        {
            if (player == null) continue;

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && health.currentHealth <= 0) continue;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = player;
            }
        }
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;

        // Stop any active chasing when attacking starts
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
        }

        float originalSpeed = navMeshAgent.speed;
        int originalDamage = damageAmount;

        if (currentTarget != null)
        {
            PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);

                // Check if target died after attack
                if (playerHealth.currentHealth <= 0)
                {
                    players.Remove(currentTarget);
                    currentTarget = null;
                }
            }
        }

        // Post-attack idle state
        navMeshAgent.speed = 0;
        damageAmount = 0;

        // Play attack animation
        // Stop movement and play attack animation on all clients
        photonView.RPC("PlayAttackAnimation", RpcTarget.All);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Play post-attack animation
        photonView.RPC("PlayAttackAnimation", RpcTarget.All);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("PostAttack"));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length - 0.5f);

        // Reset animation to movement
        photonView.RPC("ResetAnimation", RpcTarget.All);

        // Restore movement and damage
        navMeshAgent.speed = originalSpeed;
        damageAmount = originalDamage;

        isAttacking = false;

        // Update target status after animations complete
        SetClosestPlayer();

        if (currentTarget != null && HasLineOfSightToPlayer(currentTarget))
        {
            // Start new chase if valid target exists
            chaseCoroutine = StartCoroutine(ChasePlayer());
        }
        else
        {
            // Return to patrolling if no valid target
            isChasingPlayer = false;
            PickNewTarget();
        }
    }

    [PunRPC]
    void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    [PunRPC]
    void PlayPostAttackAnimation()
    {
        animator.SetTrigger("PostAttack");
    }

    [PunRPC]
    void ResetAnimation()
    {
        animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
    }

}