using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class OccultistCam : MonoBehaviour
{


    public ScriptableRendererFeature enemyHighlightFeature;
    public float passiveSeeThroughCD = 30f;
    public float passiveSeeThroughDuration= 5f;

    private bool isSeeThrough = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isSeeThrough)
        {
            isSeeThrough = true;
            StartCoroutine(ToggleSeeThrough());
        }
    }

    IEnumerator ToggleSeeThrough()
    {
        if (enemyHighlightFeature != null)
        {
            enemyHighlightFeature.SetActive(!enemyHighlightFeature.isActive);
            yield return new WaitForSeconds(passiveSeeThroughDuration);
            enemyHighlightFeature.SetActive(!enemyHighlightFeature.isActive);
            yield return new WaitForSeconds(passiveSeeThroughCD);
            isSeeThrough = false;
        }
    }
}
