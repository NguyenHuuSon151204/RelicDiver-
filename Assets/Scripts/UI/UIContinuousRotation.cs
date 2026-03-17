using UnityEngine;

public class UIContinuousRotation : MonoBehaviour
{
    [Header("Cấu hình xoay")]
    public float rotationSpeed = 30f; // Tốc độ xoay (độ/giây)
    public bool clockwise = true;    // Xoay cùng chiều hay ngược chiều kim đồng hồ

    [Header("Hiệu ứng Phập phồng (Tùy chọn)")]
    public bool pulseEffect = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    
    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // 1. Xoay vòng tròn
        float direction = clockwise ? -1f : 1f;
        transform.Rotate(0, 0, direction * rotationSpeed * Time.unscaledDeltaTime);

        // 2. Hiệu ứng phập phồng (Scale)
        if (pulseEffect)
        {
            float s = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
            transform.localScale = initialScale * s;
        }
    }
}
