using UnityEngine;

public class Trapper : MonoBehaviour
{
    [SerializeField] private int pickups = 0;

    private Camera mainCamera;
    private int originalCullingMask;
    private int trapperLayer;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCullingMask = mainCamera.cullingMask;
            trapperLayer = LayerMask.NameToLayer("TrapperPickup");
        }
    }

    void Update()
    {
        
    }

    void OnEnable()
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask |= (1 << trapperLayer);
        }
    }

    void OnDisable()
    {
        if (mainCamera != null)
        {
            mainCamera.cullingMask = originalCullingMask;
        }
    }


    public void IncrementPickup()
    {
        pickups++;
    }
}
