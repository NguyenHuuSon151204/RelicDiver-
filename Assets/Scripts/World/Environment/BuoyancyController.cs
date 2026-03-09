using UnityEngine;

public class BuoyancyController : MonoBehaviour
{
    private SinusoidalWaveController waveManager;
    private Rigidbody2D rb;

    [Header("Cấu hình nổi")]
    public float buoyancyFactor = 5.0f; // Lực đẩy nổi
    public float floatHeightOffset = 0.3f; // Để người chơi nhoi đầu lên mặt nước (số âm là chìm sâu hơn)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Tìm cái dải sóng trước/sau gần nhất
        waveManager = FindObjectOfType<SinusoidalWaveController>();
    }

    void FixedUpdate()
    {
        if (waveManager == null) return;

        // 1. Lấy độ cao của sóng tại chính xác tọa độ X của người chơi
        float currentWaveHeight = waveManager.GetWaveHeightAtX(transform.position.x);

        // 2. Nếu người chơi đang ở gần mặt nước
        if (transform.position.y < currentWaveHeight + 1f)
        {
            // Tính toán lực đẩy để người chơi nổi đúng mặt sóng
            float difference = (currentWaveHeight + floatHeightOffset) - transform.position.y;
            
            // Áp dụng lực đẩy nổi
            if (difference > 0)
            {
                rb.AddForce(Vector2.up * difference * buoyancyFactor, ForceMode2D.Force);
            }
        }
    }
}
