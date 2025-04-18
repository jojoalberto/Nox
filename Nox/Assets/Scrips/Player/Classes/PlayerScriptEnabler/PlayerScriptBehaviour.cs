using System.Collections;
using Photon.Pun;
using StarterAssets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    public bool isAbility1CD;
    public bool isAbility2CD;
    public float ability1CD;
    public float ability2CD;

    public UIBob UIHolder;

    [SerializeField] private GameObject usernameCanvas;

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
        }

    }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {

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

        GetComponent<ThirdPersonController>().enabled = false;
        GetComponent<CharacterController>().enabled = false;

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
        GetComponent<ThirdPersonController>().enabled = true;
        GetComponent<CharacterController>().enabled = true;

        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.enabled = true;
        }

        if (usernameCanvas != null)
        {
            usernameCanvas.SetActive(true);
        }
    }

}
