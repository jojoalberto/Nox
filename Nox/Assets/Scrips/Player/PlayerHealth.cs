using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviourPun
{
    public GameObject playerObject;
    public PlayerData playerData;
    public float totalHealth = 1;
    public float currentHealth = 1;
    public float temporaryHealth = 0;
    public float temporaryHealthDecay = 1f;
    public bool invulnerability = false;

    public Transform respawnLocation;

    private Coroutine tempHealthCoroutine;

    public PlayerScriptBehaviour playerScriptBehaviour;
    public HealthBar healthBar;

    [SerializeField] public Volume postProcessingVolume;

    private Vignette vignette;
    private Coroutine vignetteCoroutine;
    [ColorUsage(true, true)] public Color damageColor = Color.red;
    [ColorUsage(true, true)] public Color healColor = Color.green;

    private Coroutine saturationCoroutine;
    private ColorAdjustments colorAdjustments;
    private bool isBlackAndWhite = false;

    public float respawnDelay = 10f;
    private bool isDead = false;

    public CinemachineVirtualCamera virtualCam;
    private List<GameObject> spectateTargets = new List<GameObject>();
    private int currentSpectateIndex = 0;
    private bool isSpectating = false;

    private void Awake()
    {
        
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            SetPlayerHealth(); // ONLY the owner uses PlayerData
            StartCoroutine(WaitForHUDAndSetHealth());
        }

        if (respawnLocation == null)
        {
            respawnLocation = GameObject.FindGameObjectWithTag("Respawn")?.transform;
        }

        if (postProcessingVolume != null)
        {
            if (postProcessingVolume.profile.TryGet(out Vignette v))
            {
                vignette = v;
                vignette.intensity.value = 0f;
                vignette.color.value = damageColor;
            }
            if (postProcessingVolume.profile.TryGet(out ColorAdjustments ca))
            {
                colorAdjustments = ca;
                colorAdjustments.saturation.value = 0f;
            }
        }

        
    }

    private void Update()
    {
        if (isSpectating && Input.GetMouseButtonDown(0)) // Left click
        {
            currentSpectateIndex = (currentSpectateIndex + 1) % spectateTargets.Count;
            SetSpectateView(currentSpectateIndex);
        }
    }


    void UpdateSpectateList()
    {
        spectateTargets.Clear();
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        spectateTargets.AddRange(playerObjects);
    }

    private IEnumerator WaitForHUDAndSetHealth()
    {
        if (playerData != null)
        {
            SetPlayerHealth();
        }
        // Wait until playerScriptBehaviour and hudInstance are both valid
        while (playerScriptBehaviour == null || playerScriptBehaviour.hudInstance == null)
        {
            yield return null;
        }
        healthBar = playerScriptBehaviour.hudInstance.GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.playerHealth = this;
            healthBar.SetMaxHealth();
        }
    }


    private void SetPlayerHealth()
    {
        totalHealth = playerData.GetTotalHealth();
        currentHealth = totalHealth;
        SyncHealthWithOthers();
    }

    public void TakeDamage(int damageAmount)
    {
        if (invulnerability) return;

        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damageAmount);
    }

    public void RestoreHealing(float value)
    {
        photonView.RPC("RPC_RestoreHealthPercent", RpcTarget.All, value);
    }

    [PunRPC]
    private void RPC_TakeDamage(int damageAmount)
    {
        if (temporaryHealth > 0)
        {
            int tempAbsorbed = Mathf.Min(damageAmount, (int)temporaryHealth);
            temporaryHealth -= tempAbsorbed;
            damageAmount -= tempAbsorbed;
        }

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);


        if (photonView.IsMine)
        {
            healthBar.UpdateHealth();
            FlashVignette(damageColor);
            CheckLowHealthEffect();
            SyncHealthWithOthers();

            if (currentHealth <= 0) PlayerDies();

        }
    }




    [PunRPC]
    public void RPC_RestoreHealthPercent(float value)
    {
        currentHealth += totalHealth * value;
        currentHealth = Mathf.Min(currentHealth, totalHealth);

        if (photonView.IsMine)
        {
            healthBar.UpdateHealth();
            FlashVignette(healColor);
            CheckLowHealthEffect();

            SyncHealthWithOthers();
        }
    }

    [PunRPC]
    public void RPC_AddTemporaryHealth(float value)
    {
        temporaryHealth += value;

        if (tempHealthCoroutine != null)
            StopCoroutine(tempHealthCoroutine);

        if (photonView.IsMine)
        {
            healthBar.UpdateHealth();
            FlashVignette(healColor);
            CheckLowHealthEffect();

            SyncHealthWithOthers();
        }

        tempHealthCoroutine = StartCoroutine(DecayTemporaryHealth());
    }


    private IEnumerator DecayTemporaryHealth()
    {
        while (temporaryHealth > 0)
        {
            yield return new WaitForSeconds(1f);
            temporaryHealth -= temporaryHealthDecay;
            healthBar.UpdateHealth();
        }
        temporaryHealth = 0;
    }

    private void PlayerDies()
    {
        if (!photonView.IsMine) return;

        Debug.Log(gameObject.name + " DIED LOL");
        
        StartCoroutine(SpectateAndRespawn());

    }

    private IEnumerator SpectateAndRespawn()
    {
        photonView.RPC("RPC_DisablePlayer", RpcTarget.All);

        Spectate();

        yield return new WaitForSeconds(respawnDelay);

        RespawnPlayer();

    }

    private void Spectate()
    {
        UpdateSpectateList();
        if (spectateTargets.Count > 0)
        {
            isSpectating = true;
            SetSpectateView(currentSpectateIndex);
        }


    }

    private void SetSpectateView(int index)
    {
        if (index < 0 || index >= spectateTargets.Count || virtualCam == null) return;
        Transform target = spectateTargets[index].transform.GetChild(0);
        virtualCam.Follow = target;
    }


    private void RespawnPlayer()
    {
        if (respawnLocation == null)
        {
            respawnLocation = GameObject.FindGameObjectWithTag("Respawn")?.transform;
        }

        if (respawnLocation != null)
        {
            Vector3 pos = respawnLocation.position;
            photonView.RPC("RPC_RespawnPosition", RpcTarget.All, pos);
        }
        else
        {
            Debug.LogWarning("Respawn location not found!");
        }

        photonView.RPC("RPC_EnablePlayer", RpcTarget.All);

        if (photonView.IsMine)
        {
            SetPlayerHealth(); // Resets health
            currentHealth = totalHealth;
            

            healthBar.UpdateHealth();
        }


        FlashVignette(healColor);
        CheckLowHealthEffect();

        Debug.Log($"{gameObject.name} respawned.");

        GameObject demon = GameObject.FindWithTag("Enemy");
        if (demon != null)
        {
            PhotonView demonView = demon.GetComponent<PhotonView>();
            if (demonView != null)
            {
                demonView.RPC("RPC_AddPlayer", RpcTarget.MasterClient, photonView.ViewID);
            }
        }

        if (virtualCam != null)
        {
            Transform myCameraRoot = transform.GetChild(0);
            if (myCameraRoot != null)
            {
                virtualCam.Follow = myCameraRoot;
                isSpectating = false;
            }
        }

    }

    [PunRPC]
    private void RPC_DisablePlayer()
    {
        playerScriptBehaviour.DisablePlayer();
        isDead = true;
    }

    [PunRPC]
    private void RPC_EnablePlayer()
    {
        playerScriptBehaviour.EnablePlayer();
        isDead = false;
    }

    [PunRPC]
    void RPC_RespawnPosition(Vector3 newPos)
    {
        //transform.position = newPos;

        var photonTransform = GetComponent<PhotonTransformView>();
        if (photonTransform != null)
            photonTransform.enabled = false;

        transform.position = newPos;

        // Reactivate sync shortly after
        StartCoroutine(ReenableTransformSync());
    }

    IEnumerator ReenableTransformSync()
    {
        yield return new WaitForSeconds(0.1f); // wait 1 frame or so
        var photonTransform = GetComponent<PhotonTransformView>();
        if (photonTransform != null)
            photonTransform.enabled = true;
    }

    private void FlashVignette(Color color)
    {
        if (vignette == null) return;

        if (vignetteCoroutine != null)
            StopCoroutine(vignetteCoroutine);

        vignetteCoroutine = StartCoroutine(FadeVignette(color));
    }

    private IEnumerator FadeVignette(Color color)
    {
        vignette.color.value = color;
        vignette.intensity.value = 0.4f;

        float duration = 2f;
        float t = 0f;
        float startIntensity = 0.4f;

        while (t < duration)
        {
            t += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(startIntensity, 0f, t / duration);
            yield return null;
        }

        vignette.intensity.value = 0f;
    }

    private void CheckLowHealthEffect()
    {
        if (colorAdjustments == null) return;

        float healthPercent = currentHealth / totalHealth;

        if (healthPercent <= 0.2f && !isBlackAndWhite)
        {
            SmoothSaturation(-100f);
            isBlackAndWhite = true;
        }
        else if (healthPercent > 0.2f && isBlackAndWhite)
        {
            SmoothSaturation(0f);
            isBlackAndWhite = false;
        }
    }


    private void SmoothSaturation(float targetSaturation, float duration = 1f)
    {
        if (saturationCoroutine != null)
            StopCoroutine(saturationCoroutine);

        saturationCoroutine = StartCoroutine(FadeSaturation(targetSaturation, duration));
    }

    private IEnumerator FadeSaturation(float target, float duration)
    {
        float start = colorAdjustments.saturation.value;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            colorAdjustments.saturation.value = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        colorAdjustments.saturation.value = target;
    }

    [PunRPC]
    private void RPC_SyncHealth(float totalHealth, float currentHealth)
    {
        this.totalHealth = totalHealth;
        this.currentHealth = currentHealth;

        Debug.Log($"{photonView.Owner.NickName} synced health: {currentHealth}/{totalHealth}");

        if (photonView.IsMine && healthBar != null)
        {
            healthBar.SetMaxHealth();
            healthBar.UpdateHealth();
        }
    }

    public void SyncHealthWithOthers()
    {
        photonView.RPC("RPC_SyncHealth", RpcTarget.All, totalHealth, currentHealth);
    }
}