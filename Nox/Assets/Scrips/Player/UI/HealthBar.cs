using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Slider healthBar;
    public Gradient colorGradient;
    public Image fill;

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

        fill.color = colorGradient.Evaluate(1f);
    }

    public void UpdateHealth()
    {
        healthBar.value = playerHealth.currentHealth;

        fill.color = colorGradient.Evaluate(healthBar.normalizedValue);
    }
}
