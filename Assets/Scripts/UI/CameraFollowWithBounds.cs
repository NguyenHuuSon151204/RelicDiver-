using UnityEngine;
using TMPro;

public class CameraFollowWithBounds : MonoBehaviour
{
    [Header("Cấu hình theo dõi")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Thông báo UI")]
    public GameObject warningUI; // Kéo một TextMeshPro hoặc Image thông báo vào đây
    public float warningDuration = 0.5f;

    private Camera cam;
    private float camHeight, camWidth;
    private bool isTouchingBoundary = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (warningUI) warningUI.SetActive(false);
    }

    void LateUpdate()
    {
        if (target == null || WorldBoundary.Instance == null) return;

        // 1. Tính toán kích thước Camera trong thế giới
        camHeight = cam.orthographicSize;
        camWidth = camHeight * cam.aspect;

        // 2. Vị trí mục tiêu mong muốn
        Vector3 desiredPosition = target.position + offset;

        // 3. GIỚI HẠN (CLAMP) vị trí Camera dựa trên WorldBoundary
        Vector2 bounds = WorldBoundary.Instance.boundarySize;
        Vector2 center = WorldBoundary.Instance.transform.position;

        float minX = center.x - bounds.x / 2 + camWidth;
        float maxX = center.x + bounds.x / 2 - camWidth;
        float minY = center.y - bounds.y / 2 + camHeight;
        float maxY = center.y + bounds.y / 2 - camHeight;

        // Kiểm tra xem có đang chạm biên không để hiện thông báo
        bool currentlyTouching = false;
        if (desiredPosition.x <= minX || desiredPosition.x >= maxX || 
            desiredPosition.y <= minY || desiredPosition.y >= maxY)
        {
            currentlyTouching = true;
        }

        // Kẹp vị trí lại
        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

        // 4. Di chuyển mượt mà
        transform.position = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);

        // 5. Xử lý thông báo
        HandleWarningUI(currentlyTouching);
    }

    void HandleWarningUI(bool touch)
    {
        if (warningUI == null) return;

        if (touch)
        {
            warningUI.SetActive(true);
        }
        else
        {
            warningUI.SetActive(false);
        }
    }
}
