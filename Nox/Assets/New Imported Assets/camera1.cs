using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 角色目标
    public float distance = 2.0f; // ✅ 让相机更靠近
    public float heightOffset = 1.7f; // ✅ 让相机对准角色肩膀
    public float sideOffset = 0.5f; // ✅ 让相机稍微偏向一侧，类似肩部视角

    public float rotationSpeed = 2.0f;
    public float smoothing = 5.0f;
    public float minYAngle = -10f;
    public float maxYAngle = 80f;

    private float yaw = 0.0f;
    private float pitch = 10.0f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: 目标未分配！");
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = 10f;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1)) // 右键按住旋转
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 targetPosition = target.position + Vector3.up * heightOffset + target.right * sideOffset; // ✅ 偏向肩膀
        Vector3 newPosition = targetPosition - (rotation * Vector3.forward * distance);

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * smoothing);
        transform.LookAt(targetPosition); // ✅ 让相机对准角色肩膀
    }
}
