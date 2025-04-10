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
    private Coroutine tauntCoroutine;


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

    public bool isBound = false;

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
        if (isBound)
        {
            // Optionally update idle animation here.
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        SetClosestPlayer();
        debugMessages[0] = "Closest: " + (currentTarget != null ? currentTarget.name : "None");

        //Check if current target is drifter in invisibility state
        if (currentTarget != null)
        {
            Drifter drifter = currentTarget.GetComponent<Drifter>();
            if (drifter != null && drifter.isInvisible)
            {
                currentTarget = null;
                SetClosestPlayer();
            }
        }

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
        Transform bestTargetInLos = null;
        Transform closestTarget = null;

        foreach (Transform player in players)
        {
            if (player == null) continue;

            Drifter drifter = player.GetComponent<Drifter>();
            if (drifter != null && drifter.isInvisible)
                continue; // Ignore invisible players

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && health.currentHealth <= 0) continue;

            float distance = Vector3.Distance(transform.position, player.position);

            if (HasLineOfSightToPlayer(player))
            {
                // Prioritize the closest visible player
                if (bestTargetInLos == null || distance < Vector3.Distance(transform.position, bestTargetInLos.position))
                {
                    bestTargetInLos = player;
                }
            }

            // Keep track of the closest target in case no one is in LoS
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = player;
            }
        }

        // Prefer a target in LoS, otherwise fallback to the closest one
        if (bestTargetInLos != null)
        {
            currentTarget = bestTargetInLos;
        }
        else
        {
            currentTarget = closestTarget;
        }
    }



    IEnumerator AttackPlayer()
    {
        isAttacking = true;

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
                if (playerHealth.currentHealth <= 0)
                {
                    players.Remove(currentTarget);
                    currentTarget = null;
                }
            }
        }

        navMeshAgent.speed = 0;
        damageAmount = 0;

        photonView.RPC("PlayAttackAnimation", RpcTarget.All);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        photonView.RPC("PlayPostAttackAnimation", RpcTarget.All);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("PostAttack"));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length - 0.75f);



        navMeshAgent.speed = originalSpeed;
        damageAmount = originalDamage;

        isAttacking = false;

        isChasingPlayer = false;
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

    public void BindDemon(float duration)
    {
        photonView.RPC("RPC_BindDemon", RpcTarget.MasterClient, duration);
    }

    [PunRPC]
    void RPC_BindDemon(float duration)
    {
        StartCoroutine(BindEffect(duration));
    }

    public IEnumerator BindEffect(float duration)
    {
        // Set the demon as bound.
        isBound = true;

        navMeshAgent.isStopped = true;


        yield return new WaitForSeconds(duration);


        isBound = false;
        navMeshAgent.isStopped = false;

        SetClosestPlayer();

        StartCoroutine(ChasePlayer());
        yield return null;
    }

    [PunRPC]
    public void RPC_Taunt(int protectorViewID, float tauntDuration)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        GameObject protectorObj = PhotonView.Find(protectorViewID)?.gameObject;
        if (protectorObj == null) return;

        if (tauntCoroutine != null)
        {
            StopCoroutine(tauntCoroutine);
        }

        tauntCoroutine = StartCoroutine(ForcedChaseProtector(protectorObj.transform, tauntDuration));
    }

    IEnumerator ForcedChaseProtector(Transform protector, float duration)
    {
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
        }

        isChasingPlayer = true;
        navMeshAgent.speed = chaseSpeed;
        currentTarget = protector;

        float timer = duration;
        while (timer > 0f)
        {
            if (protector == null) break;

            navMeshAgent.SetDestination(protector.position);
            timer -= Time.deltaTime;
            yield return null;
        }

        isChasingPlayer = false;
        tauntCoroutine = null;

        SetClosestPlayer();
        chaseCoroutine = StartCoroutine(ChasePlayer());
    }
}