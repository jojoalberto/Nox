using TMPro;
using UnityEngine;

public class Flicker : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private float baseAlpha;
    private Color baseColor;

    private bool hovering = false;

    void Start()
    {
        tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        baseAlpha = tmp.color.a;
        baseColor = tmp.color;
    }

    void Update()
    {
        if(hovering)
        {
            float flicker = Mathf.PingPong(Time.time * 3.5f, 0.5f) + 0.5f;
            Color c = tmp.color;
            c.a = flicker * baseAlpha;
            tmp.color = c;
        }
    }

    public void OnHover()
    {
        hovering = true;
    }

    public void ExitHover()
    {
        hovering = false;
        tmp.color = baseColor;
    }
}
