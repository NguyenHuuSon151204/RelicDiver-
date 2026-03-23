using UnityEngine;

public class HookHeadController : MonoBehaviour
{
    [Header("--- Trạng thái ---")]
    public string status = "Đang tìm tàu ngầm..."; 

    [Header("--- Kết nối thủ công ---")]
    [SerializeField] private SubmarineHook mainHook; 

    void Start()
    {
        // Tự động tìm cha có chứa script SubmarineHook (Dự phòng nếu bạn quên kéo)
        if (mainHook == null) mainHook = GetComponentInParent<SubmarineHook>();
        if (mainHook == null) mainHook = FindObjectOfType<SubmarineHook>();

        if (mainHook != null) status = "✅ Đã kết nối với Tàu Ngầm";
        else status = "❌ LỖI: Không tìm thấy Tàu Ngầm!";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem vật phẩm này có "Tag" là Relic hoặc Kho báu không
        // Bạn có thể tùy chỉnh Tag theo ý thích (ví dụ: "Relic")
        if (collision.CompareTag("Relic") || collision.CompareTag("Item"))
        {
            if (mainHook != null)
            {
                // Thông báo cho Script chính là quắp được đồ rồi, kéo lên ngay!
                mainHook.CatchItem(collision.transform);
            }
        }
    }
}
