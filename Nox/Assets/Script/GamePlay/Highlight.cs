using UnityEngine;
using System.Collections.Generic;

public class Highlight : PlayerInteraction
{
    public LayerMask interActablelayers;
    public Material highlightMaterial;

    private GameObject currentTarget;
    private Dictionary<Renderer, Material> originalMaterials = new();
    private List<Outline> activeOutlines = new();
    private Camera localCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Only run this script for the local player
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }

        localCamera = Camera.main;
    }

    void Update()
    {
        HighlightObjects();
    }

    public void HighlightObjects()
    {
        if (localCamera == null) return;

        Ray ray = new Ray(localCamera.transform.position, localCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interActablelayers))
        {
            GameObject hitObj = hit.collider.gameObject;
            GameObject rootObj = hitObj;

            if (rootObj != currentTarget)
            {
                ClearHighlight();

                Renderer[] renderers = rootObj.GetComponentsInChildren<Renderer>();
                foreach (var rend in renderers)
                {
                    if (!originalMaterials.ContainsKey(rend))
                    {
                        originalMaterials[rend] = rend.material; // Store instance material
                        Material instancedHighlightMat = new Material(highlightMaterial);
                        rend.material = instancedHighlightMat;
                    }

                    Outline outline = rend.GetComponent<Outline>();
                    if (outline == null)
                        outline = rend.gameObject.AddComponent<Outline>();

                    outline.enabled = true;
                    outline.OutlineWidth = 4;
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
