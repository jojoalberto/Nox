using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.VisualScripting;
using Photon.Realtime;

public class DemonTargetAI1 : MonoBehaviour
{
    public Transform[] targetLocations;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    public float[] targetWeights;
    public int currentTargetIndex = -1;
    public List<Transform> players = new List<Transform>();

    public bool isChasingPlayer = false;
    public float defaultSpeed = 3f;
    public float tiredSpeed = 5f;
    public float chaseSpeed = 5.5f;
    private float interestDuration = 10f;
    private float extendedChaseDuration = 10f;
    public float gracePeriod = 5f;
    public float chaseDistance = 10f;
    private Coroutine chaseCoroutine;

    public string[] debugMessages = { "", "", "" };
    [SerializeField] private LayerMask lineOfSightLayers;

    public Transform currentTarget;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        targetWeights = Enumerable.Repeat(1f, targetLocations.Length).ToArray();

        // Find all players with the "Player" tag
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players.AddRange(playerObjects.Select(go => go.transform));

        PickNewTarget();
    }

    void Update()
    {
        SetClosestPlayer();
        debugMessages[0] = "Closest: " + currentTarget.name;

        if (!isChasingPlayer && HasLineOfSightToPlayer(currentTarget))
        {
            if (currentTarget != null)
            {
                if (chaseCoroutine == null)
                {
                    chaseCoroutine = StartCoroutine(ChasePlayer());
                }
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
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
        }
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
                if (HasLineOfSightToPlayer(currentTarget))
                {
                    currentGracePeriod = 5f;
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

    Transform GetClosestVisiblePlayer()
    {
        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform player in players)
        {
            if (player == null) continue;

            if (HasLineOfSightToPlayer(player))
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer;
    }

    public void SetClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;

        foreach (Transform player in players)
        {
            if (player == null) continue;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = player;
            }
        }
    }
}