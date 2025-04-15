using StarterAssets;
using UnityEngine;

public class UIBob : MonoBehaviour
{
    public Transform playerTransform;
    public ThirdPersonController playerController;
    public float bobAmount = 10f;
    public float followSpeed = 5f;

    private RectTransform rectTransform;
    private Vector2 initialPosition;
    private Vector2 targetOffset;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (playerController == null || playerTransform == null) return;

        Vector2 moveInput = playerController.GetComponent<StarterAssetsInputs>().move;

        // Invert input to simulate inertia (UI "pulls back" from movement)
        targetOffset = new Vector2(-moveInput.x, -moveInput.y) * bobAmount;

        // Smoothly move UI toward the target offset
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, initialPosition + targetOffset, Time.deltaTime * followSpeed);
    }
}
