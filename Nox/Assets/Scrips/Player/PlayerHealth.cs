using System.Collections;
using Photon.Pun;
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

    public Transform purgatoryLocation;

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

    void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(WaitForHUDAndSetHealth());
        }
        else
        {
            SetPlayerHealth();
        }

        if (postProcessingVolume != null )
        {
            if(postProcessingVolume.profile.TryGet(out Vignette v))
            {
                vignette = v;
                vignette.intensity.value = 0f;
                vignette.color.value = damageColor;
            }
            if(postProcessingVolume.profile.TryGet(out ColorAdjustments ca))
            {
                colorAdjustments = ca;
                colorAdjustments.saturation.value = 0f;
            }
        }
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

        currentHealth -= damageAmount;
        

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

        tempHealthCoroutine = StartCoroutine(DecayTemporaryHealth());
    }


    private IEnumerator DecayTemporaryHealth()
    {
        while (temporaryHealth > 0)
        {
            yield return new WaitForSeconds(1f);
            temporaryHealth -= temporaryHealthDecay;
        }

        temporaryHealth = 0;
    }

    private void PlayerDies()
    {
        //if (purgatoryLocation == null)
        //    purgatoryLocation = GameObject.FindGameObjectWithTag("Purgatory")?.transform;

        //if (purgatoryLocation != null)
        //    transform.position = purgatoryLocation.position;

        Debug.Log(gameObject.name + " DIED LOL");
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

        // Update health bar and effects
        if (photonView.IsMine)
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