using UnityEngine;
using UnityEngine.UI;

public class Aggression : MonoBehaviour
{
    [SerializeField] private Image clawImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVisibility()
    {
        if (clawImage != null)
        {
            clawImage.enabled = true;
        }
    }

    public void maxAggression()
    {
        if (clawImage != null)
        {
            clawImage.color = new Color32(255, 0, 0, 255); // Fully red with full opacity
        }
    }

}
