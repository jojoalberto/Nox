using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Slider healthBar;
    public Slider tempHealthBar;
    public Gradient colorGradient;
    public Gradient tempColorGradient;
    public Image fill;
    public Image tempFill;

    public PlayerHealth playerHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetMaxHealth()
    {
        healthBar.maxValue = playerHealth.totalHealth;
        healthBar.value = playerHealth.totalHealth;

        tempHealthBar.maxValue = playerHealth.totalHealth;
        tempHealthBar.value = 0f;

        fill.color = colorGradient.Evaluate(1f);
        tempFill.color = tempColorGradient.Evaluate(1f);
        UpdateTempFill();
    }

    public void UpdateHealth()
    {
        healthBar.value = playerHealth.currentHealth;

        fill.color = colorGradient.Evaluate(healthBar.normalizedValue);
        UpdateTempFill();
    }

    private void UpdateTempFill()
    {
        tempHealthBar.value = playerHealth.temporaryHealth;
        tempFill.color = tempColorGradient.Evaluate(tempHealthBar.normalizedValue);
    }

}