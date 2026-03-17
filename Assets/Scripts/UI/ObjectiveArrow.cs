using UnityEngine;

public class ObjectiveArrow : MonoBehaviour
{
    [Header("Cấu hình Mũi tên")]
    [SerializeField] private GameObject arrowVisual; // Object hình mũi tên (sprite)
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float bounceAmount = 0.5f;

    [Header("Cạnh chỉnh vị trí")]
    [SerializeField] private float orbitDistance = 2.5f; // Khoảng cách từ Player đến mũi tên
    [SerializeField] private float rotationOffset = -90f; // Chỉnh cái này trong Inspector nếu mũi tên chỉ sai hướng

    [Header("Tham chiếu mục tiêu")]
    [SerializeField] private Transform targetPoint; // Thường là trạm nạp Oxy/về đích
    private Transform player;
    private bool isActive = false;
    private Vector3 initialScale;

    private void Start()
    {
        if (arrowVisual != null) initialScale = arrowVisual.transform.localScale;
        
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.transform;

        // Ban đầu ẩn mũi tên
        if (arrowVisual) arrowVisual.SetActive(false);

        // Đăng ký sự kiện từ LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnArtifactCollected += CheckShowArrow;
        }
    }

    private void CheckShowArrow(int collected, int target)
    {
        if (collected >= target)
        {
            isActive = true;
            if (arrowVisual) arrowVisual.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isActive || player == null || targetPoint == null || arrowVisual == null) return;

        // 1. Tính toán hướng từ Player đến Đích
        Vector3 directionToTarget = (targetPoint.position - player.position).normalized;

        // 2. Đặt vị trí mũi tên ở khoảng cách orbitDistance so với Player theo hướng đích
        transform.position = player.position + directionToTarget * orbitDistance;

        // 3. Xoay mũi tên để nó luôn nhìn về phía Đích
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // 4. Hiệu ứng nhấp nhô (Scale) cho sinh động
        float s = Mathf.Sin(Time.time * bounceSpeed) * bounceAmount;
        arrowVisual.transform.localScale = initialScale + new Vector3(s, s, s);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnArtifactCollected -= CheckShowArrow;
        }
    }

    // Gán mục tiêu thủ công nếu cần
    public void SetTarget(Transform newTarget) => targetPoint = newTarget;
}
