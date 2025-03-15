using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance;
    public Camera playerCamera;
    public LayerMask interactableLayer;

    void Update()
    {
        Interact();
    }

    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green, 0.5f);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            

            if (hit.collider.TryGetComponent(out InteractableObject interactable))
            {
                Debug.Log("player interacts " + hit.collider.name);
                interactable.Interact();
            }
        }
    }

    public void Interact()
    {
        if (playerCamera == null)
        {
            return;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryInteract();
            }
        }
    }
}
