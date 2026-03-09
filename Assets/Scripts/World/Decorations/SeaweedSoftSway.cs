using UnityEngine;

public class SeaweedSoftSway : MonoBehaviour
{
    [Header("Cấu hình Uốn lượn")]
    [SerializeField] private float swayAmount = 5f;     // Độ nghiêng
    [SerializeField] private float swaySpeed = 1.5f;    // Tốc độ
    [SerializeField] private float stretchAmount = 0.05f; // Độ co giãn (tạo cảm giác mềm)
    
    private float initialZRotation;
    private Vector3 initialScale;
    private float timeOffset;

    void Start()
    {
        initialZRotation = transform.rotation.eulerAngles.z;
        initialScale = transform.localScale;
        timeOffset = Random.Range(0f, 10f); // Để các cây không lắc giống hệt nhau
    }

    void Update()
    {
        float time = Time.time * swaySpeed + timeOffset;

        // 1. Tạo độ nghiêng qua lại
        float sway = Mathf.Sin(time) * swayAmount;
        
        // 2. Tạo hiệu ứng co giãn (Làm cây nhìn như đang hít thở/mềm mại)
        // Khi nghiêng sang một bên thì hơi kéo dài ra một chút
        float stretch = Mathf.Sin(time * 2f) * stretchAmount;
        
        // Áp dụng vào vật thể
        transform.rotation = Quaternion.Euler(0, 0, initialZRotation + sway);
        transform.localScale = new Vector3(initialScale.x, initialScale.y + stretch, initialScale.z);
    }
}
