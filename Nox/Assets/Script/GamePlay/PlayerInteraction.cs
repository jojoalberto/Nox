using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance;
    public Camera playerCamera;
    public LayerMask interactableLayer;
    public LayerMask pickableLayer;
    public GameObject interactionUI;

    private PickableObject currentItem;

    void Update()
    {
        CheckForInteractable();
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("player trying to pick up the item");
            Interact();
        }

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
        else if (Physics.Raycast(ray, out hit, interactDistance, pickableLayer) && currentItem != null)
        {

            if (hit.collider.TryGetComponent(out currentItem))
            {
                Debug.Log("player pickup " + hit.collider.name);
                currentItem.Interact();
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
            Debug.Log("player picking up the item");
            TryInteract();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent(out InteractableObject interactable))
            {
                interactionUI.SetActive(true); 
                return;
            }

        }
        else if(Physics.Raycast(ray, out hit, interactDistance, pickableLayer))
        {
            if (hit.collider.TryGetComponent(out PickableObject pickableObject))
            {
                currentItem = pickableObject;
                interactionUI.SetActive(true);
                return;
            }
        }
            interactionUI.SetActive(false);
        currentItem = null;
    }

}
