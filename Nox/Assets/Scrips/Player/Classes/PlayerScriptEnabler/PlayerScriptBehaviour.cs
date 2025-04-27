using System.Collections;
using System.Linq;
using Photon.Pun;
using Photon.Voice.Unity;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;


public class PlayerScriptBehaviour : MonoBehaviour
{

    public PlayerData playerData;
    public Protector protector;
    public Occultist occultist;
    public Drifter drifter;
    public Trapper trapper;

    public string globalClassSelected;
    public PhotonView photonView;

    public GameObject playerUI;
    public GameObject hudInstance;

    public Sprite[] protectorSprites;
    public Sprite[] occultistSprites;
    public Sprite[] drifterSprites;
    public Sprite[] trapperSprites;

    public TMP_Text ability1CDText;
    public TMP_Text ability2CDText;

    public Image ability1CDFill;
    public Image ability2CDFill;
    public Image ability1Image;
    public Image ability2Image;

    public Image passiveImage;
    public TMP_Text passiveText;

    public bool isAbility1CD;
    public bool isAbility2CD;
    public float ability1CD;
    public float ability2CD;

    public UIBob UIHolder;

    [SerializeField] private GameObject usernameCanvas;

    [SerializeField] private Recorder recorder;

    public float whisperThreshold = 0.01f;
    public float talkThreshold = 0.03f;
    public float screamThreshold = 0.07f;

    [SerializeField] private DemonTargetAI1 demonTargetAI1;

    private float soundAlertCooldown = 2f;
    private float nextSoundAlertTime = 0f;

    private KeywordRecognizer keywordRecognizer;
    private string[] triggerWords = { "franklin", "jack", "amber" };

    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private AudioClip heartbeatClip;
    private Coroutine heartbeatRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

        if (photonView.IsMine)
        {
            if (playerData == null)
            {
                Debug.Log("No player data script found");
            }
            else
            {
                hudInstance = Instantiate(playerUI);
                globalClassSelected = playerData.getClassSelected();
                if (globalClassSelected != null)
                {
                    photonView.RPC("RPC_SetClassSelected", RpcTarget.All, globalClassSelected);
                }

            }
            recorder = GameObject.FindGameObjectWithTag("VCRecorder").GetComponent<Recorder>();
            demonTargetAI1 = GameObject.FindGameObjectWithTag("Enemy").GetComponent<DemonTargetAI1>();

            keywordRecognizer = new KeywordRecognizer(triggerWords);
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        }

        

    }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        //HeartBeat Settings
        if(heartbeatSource != null)
        {
            heartbeatSource.clip = heartbeatClip;
            heartbeatSource.loop = true;
            heartbeatSource.playOnAwake = false;
            heartbeatSource.volume = 0f; 
            heartbeatSource.pitch = 1f;
            heartbeatSource.spatialBlend = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.V))
            {
                recorder.TransmitEnabled = true;
                CheckVoiceLevels();
                if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
                {
                    keywordRecognizer.Start();
                    Debug.Log("Recognizer Started");
                }
            }
            else
            {
                recorder.TransmitEnabled = false;
                if (keywordRecognizer != null && keywordRecognizer.IsRunning)
                {
                    keywordRecognizer.Stop();
                    Debug.Log("Recognizer Stopped");
                }
            }



            float closestDistance = float.MaxValue;

            float distance = Vector3.Distance(transform.position, demonTargetAI1.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
            }
            UpdateHeartbeat(closestDistance);
        }



    }

    void CheckVoiceLevels()
    {
        if (!photonView.IsMine || !recorder.TransmitEnabled) return;
        if (Time.time < nextSoundAlertTime) return;
        float volume = recorder.LevelMeter.CurrentAvgAmp;

        if (volume >= screamThreshold)
        {
            AlertDemons("scream");
            Debug.Log("I AM SCREAMING");
        }
        else if (volume >= talkThreshold)
        {
            AlertDemons("talk");
            Debug.Log("I Am Talking");

        }
        else if (volume >= whisperThreshold)
        {
            AlertDemons("whisper");
            Debug.Log("i am whispering");
        }
        nextSoundAlertTime = Time.time + soundAlertCooldown;
    }

    void AlertDemons(string voiceLevel)
    {
        float range = 0f;

        switch (voiceLevel)
        {
            case "whisper":
                range = 8f; 
                break;
            case "talk":
                range = 15f;
                break;
            case "scream":
                range = 25f;
                break;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (Collider hit in hits)
        {
            DemonTargetAI1 tempDemonTargetAI1 = hit.GetComponent<DemonTargetAI1>();
            if (tempDemonTargetAI1 != null)
            {
                tempDemonTargetAI1.RequestSoundAlert(gameObject);
            }
        }
    }


    [PunRPC]
    void RPC_SetClassSelected(string classSelected)
    {
        globalClassSelected = classSelected;

        SetBools();

    }

    private void SetBools()
    {

        if (photonView.IsMine)
        {
            if (hudInstance != null)
            {
                Transform abilitiesPanel = hudInstance.transform.GetChild(0).GetChild(1);
                ability1Image = abilitiesPanel.GetChild(0).GetChild(0).GetComponent<Image>();
                ability2Image = abilitiesPanel.GetChild(1).GetChild(0).GetComponent<Image>();

                ability1CDFill = abilitiesPanel.GetChild(0).GetChild(1).GetComponent<Image>();
                ability2CDFill = abilitiesPanel.GetChild(1).GetChild(1).GetComponent<Image>();

                ability1CDText = abilitiesPanel.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                ability2CDText = abilitiesPanel.GetChild(1).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

                UIHolder = hudInstance.transform.GetChild(0).GetComponent<UIBob>();
                UIHolder.playerController = GetComponent<ThirdPersonController>();
                UIHolder.playerTransform = gameObject.transform;

                passiveImage = abilitiesPanel.GetChild(2).GetComponent<Image>();
                passiveText = abilitiesPanel.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();

                switch (globalClassSelected)
                {
                    case "Protector":
                        protector.isProtector = true;
                        ability1Image.sprite = protectorSprites[0];
                        ability2Image.sprite = protectorSprites[1];
                        break;
                    case "Occultist":
                        occultist.isOccultist = true;
                        ability1Image.sprite = occultistSprites[0];
                        ability2Image.sprite = occultistSprites[1];
                        break;
                    case "Drifter":
                        drifter.isDrifter = true;
                        ability1Image.sprite = drifterSprites[0];
                        ability2Image.sprite = drifterSprites[1];
                        break;
                    case "Trapper":
                        trapper.isTrapper = true;
                        ability1Image.sprite = trapperSprites[0];
                        ability2Image.sprite = trapperSprites[1];
                        passiveImage.sprite = trapperSprites[2];
                        trapper.passiveText = passiveText;
                        passiveImage.gameObject.SetActive(true);
                        break;
                }
            }
        }
        else
        {
            // Set class for non-local players
            switch (globalClassSelected)
            {
                case "Protector":
                    protector.isProtector = true;
                    break;
                case "Occultist":
                    occultist.isOccultist = true;
                    break;
                case "Drifter":
                    drifter.isDrifter = true;
                    break;
                case "Trapper":
                    trapper.isTrapper = true;
                    break;
            }
        }
    }

    public void SetAbility1UICD(float Cooldown)
    {
        isAbility1CD = true;
        ability1CDFill.fillAmount = 1.0f;
        ability1CDFill.gameObject.SetActive(true);
        ability1CDText.gameObject.SetActive(true);
        ability1CD = Cooldown;
        StartCoroutine(ApplyCD1());
    }

    IEnumerator ApplyCD1()
    {
        float cooldownTimer = ability1CD;

        while (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(cooldownTimer / ability1CD);
            ability1CDFill.fillAmount = ratio;

            int displayTime = Mathf.CeilToInt(cooldownTimer);
            ability1CDText.text = displayTime.ToString();

            yield return null;
        }

        ability1CDFill.fillAmount = 0f;
        ability1CDText.text = "0";
        ability1CDText.gameObject.SetActive(false);
        ability1CDFill.gameObject.SetActive(false);
        isAbility1CD = false;

        // Flash the ability icon when cooldown ends
        StartCoroutine(FlashIcon(ability1Image));
    }



    public void SetAbility2UICD(float cooldown)
    {
        isAbility2CD = true;
        ability2CDFill.fillAmount = 1.0f;
        ability2CDFill.gameObject.SetActive(true);
        ability2CDText.gameObject.SetActive(true);

        ability2CD = cooldown;
        StartCoroutine(ApplyCD2());
    }

    IEnumerator ApplyCD2()
    {
        float cooldownTimer = ability2CD;

        while (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(cooldownTimer / ability2CD);
            ability2CDFill.fillAmount = ratio;

            int displayTime = Mathf.CeilToInt(cooldownTimer);
            ability2CDText.text = displayTime.ToString();

            yield return null;
        }

        ability2CDFill.fillAmount = 0f;
        ability2CDText.text = "0";
        ability2CDText.gameObject.SetActive(false);
        ability2CDFill.gameObject.SetActive(false);
        isAbility2CD = false;

        // Flash the ability icon when cooldown ends
        StartCoroutine(FlashIcon(ability2Image));
    }


    IEnumerator FlashIcon(Image abilityIcon)
    {
        Color originalColor = abilityIcon.color;
        Color flashColor = Color.white;

        float flashDuration = 0.1f;
        int flashCount = 3;

        for (int i = 0; i < flashCount; i++)
        {
            abilityIcon.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            abilityIcon.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    public void DisablePlayer()
    {
        if (photonView.IsMine)
        {
            GetComponent<ThirdPersonController>().enabled = false;
            GetComponent<CharacterController>().enabled = false;
            hudInstance.SetActive(false);

            protector.enabled = false;
            occultist.enabled = false;
            drifter.enabled = false;
            trapper.enabled = false;
        }

        

        

        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.enabled = false;
        }

        if (usernameCanvas != null)
        {
            usernameCanvas.SetActive(false);
        }
    }

    public void EnablePlayer()
    {
        if (photonView.IsMine)
        {
            GetComponent<ThirdPersonController>().enabled = true;
            GetComponent<CharacterController>().enabled = true;
            hudInstance.SetActive(true);
            protector.enabled = true;
            occultist.enabled = true;
            drifter.enabled = true;
            trapper.enabled = true;
        }

        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.enabled = true;
        }

        if (usernameCanvas != null)
        {
            usernameCanvas.SetActive(true);
        }
    }

    private void UpdateHeartbeat(float distance)
    {
        if (distance > 25f)
        {
            if (heartbeatRoutine != null)
            {
                StopCoroutine(heartbeatRoutine);
                heartbeatRoutine = null;
            }

            StartCoroutine(FadeOutHeartbeat());
            return;
        }

        if (!heartbeatSource.isPlaying)
            heartbeatSource.Play();

        float t = Mathf.InverseLerp(25f, 8f, distance); // 0 = far, 1 = close
        float targetVolume = Mathf.Lerp(0.1f, 1f, t);
        float targetPitch = Mathf.Lerp(0.8f, 1.5f, t);

        if (heartbeatRoutine != null)
            StopCoroutine(heartbeatRoutine);

        heartbeatRoutine = StartCoroutine(FadeInHeartbeat(targetVolume, targetPitch));
    }

    IEnumerator FadeInHeartbeat(float targetVolume, float targetPitch)
    {
        float duration = 0.5f;
        float timer = 0f;

        float startVolume = heartbeatSource.volume;
        float startPitch = heartbeatSource.pitch;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            heartbeatSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            heartbeatSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);

            yield return null;
        }

        heartbeatSource.volume = targetVolume;
        heartbeatSource.pitch = targetPitch;
    }

    IEnumerator FadeOutHeartbeat()
    {
        float duration = 0.5f;
        float timer = 0f;

        float startVolume = heartbeatSource.volume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            heartbeatSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        heartbeatSource.volume = 0f;
        heartbeatSource.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        // Whisper range - Blue
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 8f);

        // Talk range - Green
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 15f);

        // Scream range - Red
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 25f);
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Recognizer Recognized phrase: " + args.text);

        if (triggerWords.Contains(args.text.ToLower()))
        {
            Debug.Log("Recognizer Trigger word spoken! Enraging demon!");
            demonTargetAI1.RequestStartChasing(transform);
        }
    }
}
