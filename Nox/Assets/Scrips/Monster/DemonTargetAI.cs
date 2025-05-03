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
    private enum SpeedState { Idle, ChasingFresh, ChasingTired }
    private SpeedState currentSpeedState = SpeedState.Idle;

    private Transform pendingAlertTarget = null;
    private bool investigatingAlert = false;

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
    public string[] debugMessages = { "", "", "", "" };

    [Header("States")]
    public bool isChasingPlayer = false;
    public bool isAttacking = false;

    [Header("Attack Settings")]
    public float attackRange = 5f;
    public int damageAmount = 1;
    public float postAttackIdleTime = 4f;

    private PhotonView photonView;

    public bool isBound = false;
    private bool isSlowed = false;
    private bool isFrozen = false;

    private Coroutine slowCoroutine;
    private float slowMultiplier = 1f;

    [Header("Aggression Settings")]
    [SerializeField] private float rate = 120f;
    [SerializeField] private float damageIncrement = 10f;
    [SerializeField] private float speedIncrement = 0.3f;
    [SerializeField] private float maximumDamage = 50f;
    [SerializeField] private float maximumSpeedIncrement = 2f;

    private float originaldefaultSpeed = 3f;
    private float originaltiredSpeed = 5f;
    private float originalchaseSpeed = 5.5f;

    [SerializeField] private Aggression aggressionUI;

    [SerializeField] ChaseEffects chaseEffects;

    [SerializeField] private AudioManager globalAudioManager;
    [SerializeField] private AudioSource breathingAudioSource;
    [SerializeField] private AudioSource generalAudioSource;
    [SerializeField] private AudioClip[] demonSfx;
    private Coroutine breathingCoroutine;

    [Header("Spawn Settings")]
    public bool isSpawnWaiting = true;
    [SerializeField] private float spawnWaitTime = 30f;
    [SerializeField] SkinnedMeshRenderer[] meshRenderer;

    private int lastAttackID = -1; // Tracks the last attack seen on each client
    private int attackCounter = 0; // Only used by the master/client who controls the demon


    void Start()
    {
        photonView = GetComponent<PhotonView>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        targetWeights = Enumerable.Repeat(1f, targetLocations.Length).ToArray();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(GhoulStartWait());
            StartCoroutine(WaitForPlayersToInstantiate());
        }

        originaldefaultSpeed = defaultSpeed;
        originaltiredSpeed = tiredSpeed;
        originalchaseSpeed = chaseSpeed;


        
        StartCoroutine(GetAggressionUI());
        StartCoroutine(Aggression());

    }

    IEnumerator GhoulStartWait()
    {
        foreach (SkinnedMeshRenderer mesh in meshRenderer)
        {
            mesh.enabled = false;
        }

        WaitForSeconds wait = new WaitForSeconds(spawnWaitTime);
        yield return wait;

        foreach(SkinnedMeshRenderer mesh in meshRenderer)
        {
            mesh.enabled = true;
        }
        isSpawnWaiting = false;
        PickNewTarget();
    }

    IEnumerator Aggression()
    {
        WaitForSeconds wait = new WaitForSeconds(rate);


        while (true)
        {
            yield return wait;
            PlaySoundAudioFromManager("MonsterAggression");
            if (PhotonNetwork.IsMasterClient)
            {
                // Increase damage
                if (damageAmount < maximumDamage)
                {
                    damageAmount += (int)damageIncrement;
                    damageAmount = Mathf.Min(damageAmount, (int)maximumDamage);

                    // Increase speeds
                    defaultSpeed += speedIncrement;
                    tiredSpeed += speedIncrement;
                    chaseSpeed += speedIncrement;

                    defaultSpeed = Mathf.Min(defaultSpeed, originaldefaultSpeed + maximumSpeedIncrement);
                    tiredSpeed = Mathf.Min(tiredSpeed, originaltiredSpeed + maximumSpeedIncrement);
                    chaseSpeed = Mathf.Min(chaseSpeed, originalchaseSpeed + maximumSpeedIncrement);
                }

                

                UpdateNavMeshSpeed();
                if (aggressionUI != null)
                {
                    if (damageAmount == maximumDamage)
                    {
                        photonView.RPC("RPC_ShowAggressionClaw", RpcTarget.All, 1);
                    }
                    else
                    {
                        photonView.RPC("RPC_ShowAggressionClaw", RpcTarget.All, 0);
                    }
                }


                debugMessages[3] = $"Aggression Increased: DMG={damageAmount}, SPD={defaultSpeed:F2}";
            }
        }
    }

    [PunRPC]
    public void RPC_ShowAggressionClaw(int tier)
    {
        if (tier == 0)
        {
            aggressionUI.SetVisibility();
        }
        else
        {
            aggressionUI.maxAggression();
        }
    }


    IEnumerator WaitForPlayersToInstantiate()
    {
        while (players.Count < PhotonNetwork.PlayerList.Length)
        {
            UpdatePlayerList();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator GetAggressionUI()
    {
        GameObject uiObj = null;

        while (uiObj == null)
        {
            uiObj = GameObject.FindGameObjectWithTag("AggressionUI");
            yield return null;
        }

        aggressionUI = uiObj.GetComponent<Aggression>();

        if (aggressionUI == null)
        {
            Debug.LogError("Found 'AggressionUI' object, but it has no Aggression component.");
        }
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
        if(isSpawnWaiting)
            { return; }
        UpdateNavMeshSpeed();

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
            if (investigatingAlert)
            {
                investigatingAlert = false;
                pendingAlertTarget = null;
                debugMessages[1] = "Finished alert investigation. Picking new target.";
                PickNewTarget();
            }
            else
            {
                ReduceTargetWeight(currentTargetIndex);
                PickNewTarget();
            }
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
        if (isSpawnWaiting)
        { yield break; }

        isChasingPlayer = true;


        BeginChaseMusic("ChaseA");

        if (chaseEffects != null)
        {
            chaseEffects.StartChaseEffect();
        }

        currentSpeedState = SpeedState.ChasingFresh;
        UpdateNavMeshSpeed();

        float chaseTimer = interestDuration;
        float currentGracePeriod = gracePeriod;

        while (chaseTimer > 0f)
        {
            SetClosestPlayer();
            if (currentTarget == null) break;

            debugMessages[1] = "Chasing: " + currentTarget.name;
            navMeshAgent.SetDestination(currentTarget.position);

            chaseTimer -= Time.deltaTime;
            yield return null;
        }

        if (currentTarget != null)
        {
            currentSpeedState = SpeedState.ChasingTired;
            UpdateNavMeshSpeed();

            chaseTimer = extendedChaseDuration;

            while (chaseTimer > 0f || currentGracePeriod > 0f)
            {
                SetClosestPlayer();
                if (currentTarget == null) break;

                if (HasLineOfSightToPlayer(currentTarget))
                {
                    currentGracePeriod = gracePeriod;
                    chaseTimer -= Time.deltaTime;
                }
                else
                {
                    if (currentGracePeriod > 0f)
                        currentGracePeriod -= Time.deltaTime;

                    chaseTimer -= Time.deltaTime;
                }

                navMeshAgent.SetDestination(currentTarget.position);
                yield return null;
            }
        }

        isChasingPlayer = false;
        EndChaseMusic();

        if (chaseEffects != null)
        {
            chaseEffects.StopChaseEffect();
        }

        currentSpeedState = SpeedState.Idle;
        UpdateNavMeshSpeed();

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
        if (isSpawnWaiting || isFrozen || isBound)
            yield break;

        isAttacking = true;

        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
        }

        UpdateNavMeshSpeed();
        int originalDamage = damageAmount;

        // Create a unique attack ID
        int currentAttackID = attackCounter++; // Only increases on the attacker (master client)

        // Play animation and sound for all clients immediately
        photonView.RPC("RPC_PlayAudioClip", RpcTarget.All, 0);
        photonView.RPC("RPC_PlayAttackAnimation", RpcTarget.All, currentAttackID);

        // Apply damage
        if (currentTarget != null)
        {
            PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (isFrozen || isBound)
                {
                    isAttacking = false;
                    UpdateNavMeshSpeed();
                    yield break;
                }

                playerHealth.TakeDamage(damageAmount);
                playerHealth.PlayStaggerAnimation();

                if (playerHealth.currentHealth <= 0)
                {
                    players.Remove(currentTarget);
                    currentTarget = null;
                }
            }
        }

        damageAmount = 0;

        yield return new WaitForSeconds(2.8f);

        photonView.RPC("RPC_PlayPostAttackAnimation", RpcTarget.All, currentAttackID);

        yield return new WaitForSeconds(2.33f);

        isAttacking = false;
        UpdateNavMeshSpeed();
        damageAmount = originalDamage;

        // Resume chasing or idle
        if (currentTarget != null && !currentTarget.GetComponent<PlayerHealth>().isDead)
        {
            currentSpeedState = SpeedState.ChasingTired;
            isChasingPlayer = true;

            chaseCoroutine = StartCoroutine(ChasePlayer());
        }
        else
        {
            currentSpeedState = SpeedState.Idle;
            isChasingPlayer = false;

            if (chaseEffects != null)
            {
                chaseEffects.StopChaseEffect();
            }

            PickNewTarget();
        }

        UpdateNavMeshSpeed();
    }

    [PunRPC]
    void RPC_PlayAttackAnimation(int attackID)
    {
        if (attackID <= lastAttackID)
        {
            Debug.Log($"[AttackAnimation] Skipping duplicate ID {attackID} on {PhotonNetwork.LocalPlayer.ActorNumber}");
            return;
        }

        lastAttackID = attackID;
        Debug.Log($"[AttackAnimation] Playing attack ID {attackID} on {PhotonNetwork.LocalPlayer.ActorNumber}");
        animator.SetTrigger("Attack");
    }

    [PunRPC]
    void RPC_PlayPostAttackAnimation(int attackID)
    {
        if (attackID != lastAttackID)
        {
            Debug.Log($"[PostAttack] Ignored for mismatched ID {attackID} on {PhotonNetwork.LocalPlayer.ActorNumber}");
            return;
        }

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

        if (isSpawnWaiting)
        { return ; }

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
        if (!isSpawnWaiting)
            yield break;

        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
        }

        isChasingPlayer = true;
        UpdateNavMeshSpeed();
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

    public void RequestSlow(float slowAmount, float duration)
    {
        photonView.RPC("RPC_ApplySlow", RpcTarget.MasterClient, slowAmount, duration);
    }

    [PunRPC]
    public void RPC_ApplySlow(float slowAmount, float duration)
    {
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowEffect(slowAmount, duration));
    }

    private IEnumerator SlowEffect(float slowAmount, float duration)
    {
        isSlowed = true;
        slowMultiplier = slowAmount;

        UpdateNavMeshSpeed();

        yield return new WaitForSeconds(duration);

        isSlowed = false;
        slowMultiplier = 1f;

        UpdateNavMeshSpeed();

        slowCoroutine = null;
    }

    public void RequestFreeze(float duration)
    {
        photonView.RPC("RPC_ApplyFreeze", RpcTarget.MasterClient, duration);
    }

    [PunRPC]
    public void RPC_ApplyFreeze(float duration)
    {
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(FreezeEffect(duration));
    }

    private IEnumerator FreezeEffect(float duration)
    {
        isFrozen = true;
        UpdateNavMeshSpeed();
        yield return new WaitForSeconds(duration);
        isFrozen = false;
        UpdateNavMeshSpeed(); 
    }



    private void UpdateNavMeshSpeed()
    {
        if (isBound || isAttacking || isFrozen)
        {
            navMeshAgent.speed = 0;
            return;
        }

        float baseSpeed = defaultSpeed;

        switch (currentSpeedState)
        {
            case SpeedState.ChasingFresh:
                baseSpeed = chaseSpeed;
                break;
            case SpeedState.ChasingTired:
                baseSpeed = tiredSpeed;
                break;
            case SpeedState.Idle:
                baseSpeed = defaultSpeed;
                break;
            default:
                baseSpeed = defaultSpeed;
                break;
        }

        if (isSlowed)
        {
            navMeshAgent.speed = Mathf.Max(0.01f, baseSpeed * slowMultiplier);
        }
        else
        {
            navMeshAgent.speed = baseSpeed;
        }
    }

    [PunRPC]
    public void RPC_SoundAlert(GameObject soundOrigin)
    {
        if (soundOrigin == null || targetLocations.Length == 0) return;

        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Transform location in targetLocations)
        {
            if (location == null) continue;

            float distance = Vector3.Distance(soundOrigin.transform.position, location.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = location;
            }
        }

        if (nearestTarget == null) return;

        // If chasing, remember it for later
        if (isChasingPlayer)
        {
            pendingAlertTarget = nearestTarget;
            debugMessages[1] = "Heard sound, but still chasing.";
        }
        else
        {
            InvestigateAlertLocation(nearestTarget);
        }
    }

    private void InvestigateAlertLocation(Transform target)
    {
        if (target == null) return;

        currentTargetIndex = -1; // make it clear it's not a standard patrol point
        navMeshAgent.SetDestination(target.position);
        investigatingAlert = true;
        debugMessages[1] = "Investigating alert at: " + target.name;
    }

    public void RequestSoundAlert(GameObject soundOrigin)
    {
        photonView.RPC("RPC_SoundAlert", RpcTarget.MasterClient, soundOrigin);
    }

    public void RequestStartChasing(Transform target)
    {
        Debug.Log("Recognizer " + target.name);
        PhotonView targetView = target.GetComponent<PhotonView>();
        if (targetView != null)
        {
            photonView.RPC("RPC_StartChasing", RpcTarget.MasterClient, targetView.ViewID);
        }
    }

    [PunRPC]
    public void RPC_StartChasing(int targetViewID)
    {
        if (isSpawnWaiting)
        {
            return;
        }

        PhotonView targetView = PhotonView.Find(targetViewID);
        if (currentTarget == targetView.transform && isChasingPlayer)
            return;

        if (targetView == null) return;

        currentTarget = targetView.transform;
        isChasingPlayer = true;
        currentSpeedState = SpeedState.ChasingFresh;
        UpdateNavMeshSpeed();

        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
        }

        chaseCoroutine = StartCoroutine(ChasePlayer());

        Debug.Log("Demon has started chasing the player!");
    }

    [PunRPC]
    public void RPC_AddPlayer(int playerViewID)
    {
        PhotonView view = PhotonView.Find(playerViewID);
        if (view != null && view.transform != null)
        {
            AddPlayer(view.transform);
        }
    }

    public void AddPlayer(Transform player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            debugMessages[2] = $"Added player {player.name} to tracking list.";
        }
    }

    
    private void BeginChaseMusic(string name)
    {
        globalAudioManager.RequestPlayMusicClipByName(name);
    }

    private void EndChaseMusic()
    {
        globalAudioManager.RequestPlayMusicClipByName("General");
    }

    private void PlaySoundAudioFromManager(string name)
    {
        globalAudioManager.RPCPlayAudioByName(name);
    }


    public void UpdateBreathing(float distance)
    {
        if(isSpawnWaiting)
            { return; }
        if (distance > 25f)
        {
            if (breathingCoroutine != null)
            {
                StopCoroutine(breathingCoroutine);
                breathingCoroutine = null;
            }

            StartCoroutine(FadeOutBreathing());
            return;
        }

        if (!breathingAudioSource.isPlaying)
            breathingAudioSource.Play();

        float t = Mathf.InverseLerp(25f, 8f, distance);
        float targetVolume = Mathf.Lerp(0.1f, 1f, t);
        float targetPitch = Mathf.Lerp(0.8f, 1.5f, t);

        if (breathingCoroutine != null)
            StopCoroutine(breathingCoroutine);

        breathingCoroutine = StartCoroutine(FadeInBreathing(targetVolume, targetPitch));
    }

    IEnumerator FadeInBreathing(float targetVolume, float targetPitch)
    {
        float duration = 0.5f;
        float timer = 0f;

        float startVolume = breathingAudioSource.volume;
        float startPitch = breathingAudioSource.pitch;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            breathingAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            breathingAudioSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);

            yield return null;
        }

        breathingAudioSource.volume = targetVolume;
        breathingAudioSource.pitch = targetPitch;
    }

    IEnumerator FadeOutBreathing()
    {
        float duration = 0.5f;
        float timer = 0f;

        float startVolume = breathingAudioSource.volume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            breathingAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        breathingAudioSource.volume = 0f;
        breathingAudioSource.Stop();
    }

    [PunRPC]
    public void RPC_PlayAudioClip(int clipIndex)
    {
        generalAudioSource.PlayOneShot(demonSfx[clipIndex]);
    }

}