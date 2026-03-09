using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CausticAnimation : MonoBehaviour
{
    [Header("Cấu hình Lưới sáng")]
    [SerializeField] private Light2D causticLight;
    [SerializeField] private float pulseSpeed = 1.5f; // Tốc độ nhấp nháy
    [SerializeField] private float driftSpeed = 0.5f; // Tốc độ trôi dạt
    [SerializeField] private float driftAmount = 1.5f; // Độ lệch khi trôi

    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.position;
        if (causticLight == null) causticLight = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (causticLight == null) return;

        // 1. Làm lưới nắng nhấp nháy mượt mà (Pulsing)
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
        causticLight.intensity = 0.5f + pulse;

        // 2. Làm lưới nắng trôi dạt (Wandering)
        // Khi cái đèn di chuyển nhẹ vòng tròn, cái bóng Cookie trên mặt đất sẽ trôi rất thật
        float x = Mathf.Sin(Time.time * driftSpeed) * driftAmount;
        float y = Mathf.Cos(Time.time * driftSpeed * 0.8f) * driftAmount;
        
        transform.position = initialPos + new Vector3(x, y, 0);
    }
}
