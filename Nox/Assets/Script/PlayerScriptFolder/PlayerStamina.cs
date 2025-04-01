using StarterAssets;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{

    public PlayerData playerData;
    public float totalStamina = 1;
    public float currentStamina = 1;
    public float staminaRegen = 15f;
    public float staminaDelay = 2.5f;
    public bool infiniteStamina = false;

    public float runConsumption = 15f;
    private float RegenTimer = 0f;


    public ThirdPersonController thirdPersonController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(thirdPersonController == null)
        {
            try
            {
                thirdPersonController = gameObject.GetComponent<ThirdPersonController>();
            }
            catch
            {
                Debug.Log("No player controller found");
            }
            
        }
        if (playerData != null)
        {
            setPlayerStamina();       
        }

        RegenTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (infiniteStamina) return;

        // Check if the player is currently sprinting and has stamina.
        if (thirdPersonController != null && thirdPersonController.IsSprinting && currentStamina > 0)
        {
            // Consume stamina at 1 per second.
            currentStamina -= runConsumption * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);

            // Reset the regeneration delay timer.
            RegenTimer = staminaDelay;
        }
        else
        {
            // If not sprinting, count down the delay timer.
            if (RegenTimer > 0)
            {
                RegenTimer -= Time.deltaTime;
            }
            else
            {
                // Regenerate stamina at the specified rate.
                currentStamina += staminaRegen * Time.deltaTime;
                if (currentStamina > totalStamina)
                    currentStamina = totalStamina;
            }
        }

        // Prevent sprinting if stamina is depleted.
        if (thirdPersonController != null)
        {
            thirdPersonController.SprintAllowed = currentStamina > 0;
        }
    }

    private void setPlayerStamina()
    {
        totalStamina = playerData.GetTotalStamina();
        currentStamina = totalStamina;

        staminaRegen = playerData.GetStaminaRegen();
        staminaDelay = playerData.GetStaminaDelay();
    }
}
