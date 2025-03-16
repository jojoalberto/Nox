using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine.Rendering;

public class DemonTargetAI1 : MonoBehaviour
{
    public Transform[] targetLocations;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    public float[] targetWeights;
    public int currentTargetIndex = -1;
    public Transform player;

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

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        targetWeights = Enumerable.Repeat(1f, targetLocations.Length).ToArray();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        PickNewTarget();
    }

    void Update()
    {
        debugMessages[0] = Vector3.Distance(transform.position, player.position).ToString();

        if (!isChasingPlayer && HasLineOfSight())
        {
            if (chaseCoroutine == null)
            {
                chaseCoroutine = StartCoroutine(ChasePlayer());
            }
        }
        else if (isChasingPlayer)
        {
            navMeshAgent.SetDestination(player.position);
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
        debugMessages[1] = "Player";
        isChasingPlayer = true;
        navMeshAgent.speed = chaseSpeed;
        float chaseTime = interestDuration;

        while (chaseTime > 0f)
        {

            navMeshAgent.SetDestination(player.position);
            chaseTime -= Time.deltaTime;
            yield return null;
        }

        // Check if player is still visible for extended chase
        if (HasLineOfSight())
        {
            navMeshAgent.speed = tiredSpeed;
            chaseTime = extendedChaseDuration;
            float grace = gracePeriod;

            // Extended chase phase with grace period
            while (chaseTime > 0f || grace > 0f)
            {
                bool hasSight = HasLineOfSight();
                if (hasSight)
                {
                    grace = 5f;
                    chaseTime -= Time.deltaTime;
                }
                else
                {
                    if (grace > 0f)
                    {
                        grace -= Time.deltaTime;
                    }
                    chaseTime -= Time.deltaTime;
                }

                navMeshAgent.SetDestination(player.position);
                yield return null;
            }
        }

        // Resume normal behavior
        isChasingPlayer = false;
        navMeshAgent.speed = defaultSpeed;
        chaseCoroutine = null;
        PickNewTarget();
    }




    bool HasLineOfSight()
    {
        if (player == null) return false;

        // Adjust for eye level to avoid ground hits
        Vector3 aiEyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 playerEyePosition = player.position + Vector3.up * 1.5f;

        // Calculate distance and direction
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (playerEyePosition - aiEyePosition).normalized;

        // Check if player is within chase distance or in front
        bool isInFront = Vector3.Angle(transform.forward, directionToPlayer) <= 90f;
        bool isCloseEnough = distanceToPlayer <= chaseDistance;

        // Demon can only see behind if player is within 10 units
        if (!isCloseEnough && !isInFront) return false;

        // Visual debug line
        Debug.DrawLine(aiEyePosition, playerEyePosition, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Linecast(aiEyePosition, playerEyePosition, out hit, lineOfSightLayers))
        {
            debugMessages[2] = hit.collider.name;
            return hit.collider.CompareTag("Player");
        }
        return false;
    }
}
