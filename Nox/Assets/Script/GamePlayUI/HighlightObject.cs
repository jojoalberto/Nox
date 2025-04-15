using UnityEngine;
using System.Collections.Generic;

public class HighlightObject : PlayerInteraction
{
    public LayerMask interactLayers;
    public Material highlightMaterial;

    private GameObject currentTarget;
    private Dictionary<Renderer, Material> originalMaterials = new();
    private List<Outline> activeOutlines = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HighlightObjects();
    }

    public void HighlightObjects()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayers))
        {
            GameObject hitObj = hit.collider.gameObject;
            GameObject rootObj = hitObj;

            if (rootObj != currentTarget)
            {
                ClearHighlight();

                Renderer[] renderers = rootObj.GetComponentsInChildren<Renderer>();
                foreach (var rend in renderers)
                {
                    // Store original material
                    if (!originalMaterials.ContainsKey(rend))
                    {
                        originalMaterials[rend] = rend.material;
                        rend.material = highlightMaterial;
                    }

                    // Enable or add Outline component (Chris Nolet's version)
                    Outline outline = rend.GetComponent<Outline>();
                    if (outline == null)
                        outline = rend.gameObject.AddComponent<Outline>();

                    outline.enabled = true;
                    activeOutlines.Add(outline);
                }

                currentTarget = rootObj;
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    public void ClearHighlight()
    {
        foreach (var pair in originalMaterials)
        {
            if (pair.Key != null)
                pair.Key.material = pair.Value;
        }

        foreach (var outline in activeOutlines)
        {
            if (outline != null)
                outline.enabled = false;
        }

        originalMaterials.Clear();
        activeOutlines.Clear();
        currentTarget = null;
    }

}
