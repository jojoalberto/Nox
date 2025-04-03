using UnityEngine;

public class PlayerMats : MonoBehaviour
{
    SkinnedMeshRenderer[] originalSkinnedMeshRenderers;
    Material[] originalMaterial;
    Material[] originalMaterialOverride;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeMaterial()
    {
        for (int i = 0; i < originalSkinnedMeshRenderers.Length; i++)
        {

        }
    }
}
