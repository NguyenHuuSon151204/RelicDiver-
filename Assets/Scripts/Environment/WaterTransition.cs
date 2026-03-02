using UnityEngine;

public class WaterTransition : MonoBehaviour
{
    [Header("Cấu hình khi vào nước")]
    [SerializeField] private float waterGravityScale = 0f;
    [SerializeField] private float waterDrag = 1.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = waterGravityScale;
                rb.drag = waterDrag;
            }

            // Kích hoạt hệ thống Oxy khi vào nước
            OxygenSystem oxygen = other.GetComponent<OxygenSystem>();
            if (oxygen != null) oxygen.enabled = true;

            Debug.Log("Đã vào nước! Hệ thống Oxy đã kích hoạt.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f; // Trả lại trọng lực khi lên bờ
                rb.drag = 0.5f;
            }

            // Tắt hệ thống Oxy khi lên bờ (tùy chọn)
            OxygenSystem oxygen = other.GetComponent<OxygenSystem>();
            if (oxygen != null) oxygen.enabled = false;

            Debug.Log("Lên bờ! Hệ thống Oxy đã tạm dừng.");
        }
    }
}
