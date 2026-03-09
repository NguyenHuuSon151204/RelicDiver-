using UnityEngine;

public class DiverAnimationHandler : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    
    [Header("Cấu hình")]
    public float minAnimSpeed = 0.5f; // Tốc độ quạt chân khi đứng yên
    public float maxAnimSpeed = 2.5f; // Tốc độ quạt chân khi bơi nhanh
    public float speedMultiplier = 0.3f;

    void Start()
    {
        // Thử tìm Animator ở chính nó hoặc con cái
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        // Thử tìm Rigidbody ở chính nó hoặc cha
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();

        if (anim == null) Debug.LogWarning("Không tìm thấy Animator cho Diver!");
        if (rb == null) Debug.LogWarning("Không tìm thấy Rigidbody cho Diver!");
    }

    void Update()
    {
        if (anim == null || rb == null) return;
        
        // Nếu đối tượng chứa Animator đang bị ẩn (đang ở trong tàu) thì không làm gì cả
        if (!anim.gameObject.activeInHierarchy) return;

        // Lấy vận tốc hiện tại của thợ lặn
        float currentSpeed = rb.velocity.magnitude;

        // Tính toán tốc độ Animation dựa trên vận tốc bơi
        float targetAnimSpeed = minAnimSpeed + (currentSpeed * speedMultiplier);
        targetAnimSpeed = Mathf.Clamp(targetAnimSpeed, minAnimSpeed, maxAnimSpeed);

        // Áp dụng tốc độ vào Animator
        anim.speed = targetAnimSpeed;
        
        // Đảm bảo Animator luôn ở trạng thái Update
        if (anim.speed > 0 && !anim.enabled) anim.enabled = true;
    }
}
