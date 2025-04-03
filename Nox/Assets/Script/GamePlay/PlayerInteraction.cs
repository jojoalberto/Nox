using Photon.Pun;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance;
    public Camera playerCamera;
    public LayerMask interactableLayer;
    public LayerMask pickableLayer;
    public GameObject interactionUI;

    private PickableObject currentItem;
    private int playerLayerMask; // Stores the player's layer to exclude from raycasts

    public PlayerData playerData;
    public string classSelected;

    public Trapper trapper;
    public LayerMask trapperPickableLayer;

    private LayerMask currentPickableMask;

    public PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        playerLayerMask = 1 << gameObject.layer; 

        if (playerData != null)
        {
            classSelected = playerData.classSelected;

            if (classSelected == "Trapper")
            {
                currentPickableMask = pickableLayer | trapperPickableLayer;
                if (trapper != null) trapper.enabled = true;
            }
            else
            {
                currentPickableMask = pickableLayer;
                if (trapper != null) trapper.enabled = false;
            }
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        CheckForInteractable();
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green, 0.5f);

        int interactableMask = interactableLayer.value & ~playerLayerMask;
        int pickableMask = currentPickableMask.value & ~playerLayerMask;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableMask))
        {
            if (hit.collider.TryGetComponent(out InteractableObject interactable))
            {
                interactable.Interact();
            }
        }
        else if (Physics.Raycast(ray, out hit, interactDistance, pickableMask))
        {
            if (hit.collider.TryGetComponent(out PickableObject pickable))
            {
                if (pickable.gameObject.layer == 10 && trapper != null)
                {
                    trapper.IncrementPickup();
                }
                pickable.Interact();
            }
        }
    }

    public void Interact()
    {
        if (playerCamera != null)
        {
            TryInteract();
        }
    }

    private void CheckForInteractable()
    {

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        int interactableMask = interactableLayer.value & ~playerLayerMask;
        int pickableMask = currentPickableMask.value & ~playerLayerMask;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableMask))
        {
            if (hit.collider.TryGetComponent(out InteractableObject interactable))
            {
                interactionUI.SetActive(true);
                return;
            }
        }

        if (Physics.Raycast(ray, out hit, interactDistance, pickableMask))
        {
            if (hit.collider.TryGetComponent(out PickableObject pickable))
            {
                currentItem = pickable;
                interactionUI.SetActive(true);
                return;
            }
        }

        interactionUI.SetActive(false);
        currentItem = null;
    }
}