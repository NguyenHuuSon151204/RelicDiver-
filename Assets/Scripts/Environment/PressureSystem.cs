using UnityEngine;

public class PressureSystem : MonoBehaviour
{
    [Header("Cấu hình Áp suất")]
    [SerializeField] private float surfaceY = 0f; // Điểm bắt đầu tính áp suất
    [SerializeField] private float safeDepth = 10f; // Độ sâu an toàn (không tốn máu)
    [SerializeField] private float damageMultiplier = 0.5f; // Tỉ lệ sát thương
    [SerializeField] private float checkInterval = 1f; // Tần suất kiểm tra

    private HealthSystem health;
    private HUDController hud;
    private float timer;

    private void Start()
    {
        health = GetComponent<HealthSystem>();
        hud = FindObjectOfType<HUDController>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            float currentDepth = surfaceY - transform.position.y;
            
            if (currentDepth > safeDepth)
            {
                float damage = (currentDepth - safeDepth) * damageMultiplier;
                ApplyPressureDamage(damage);
            }
            timer = 0;
        }
    }

    private void ApplyPressureDamage(float damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"<color=red>CẢNH BÁO:</color> Áp suất quá lớn! Sát thương: {damage}");
        }

        // Kích hoạt hiệu ứng đỏ màn hình trên HUD
        if (hud != null)
        {
            hud.TriggerPressureWarning();
        }
    }

    // Sau này dùng để nâng cấp
    public void UpgradeSafeDepth(float amount)
    {
        safeDepth += amount;
    }
}
