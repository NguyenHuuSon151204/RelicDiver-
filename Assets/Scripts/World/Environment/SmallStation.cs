using UnityEngine;

public class SmallStation : MonoBehaviour
{
    [Header("Cấu hình Hồi phục")]
    public float oxygenRefillRate = 15f;
    public float healthRefillRate = 5f;
    public float detectionRange = 3f;

    [Header("Visual & Sound")]
    public SpriteRenderer stationHighlight;
    public ParticleSystem refillEffect; // Hiệu ứng bong bóng khí liti
    public AudioClip refillSound;        // Âm thanh hồi oxy (loop)
    public Color activeColor = Color.green;
    public Color idleColor = Color.white;

    private bool isPlayerInside = false;
    private PlayerStatusManager playerStatus;

    private void Start()
    {
        // Luôn đảm bảo có CircleCollider2D và nó phải là TRIGGER
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = detectionRange;
        }
        
        col.isTrigger = true; // Ép thành trigger để không bị giựt khi chạm

        if (stationHighlight) stationHighlight.color = idleColor;
    }

    private void Update()
    {
        if (isPlayerInside && playerStatus != null)
        {
            playerStatus.RestoreOxygen(oxygenRefillRate * Time.deltaTime);
            playerStatus.Heal(healthRefillRate * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerStatus = other.GetComponent<PlayerStatusManager>();
            
            if (stationHighlight) stationHighlight.color = activeColor;
            if (refillEffect) refillEffect.Play(); // Chạy hiệu ứng hạt

            // Báo cho Level Manager là đã về đến căn cứ
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetPlayerAtHomeBase(true);
            }
            
            // Âm thanh khi bắt đầu hồi (Tùy chọn)
            if (refillSound && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(refillSound);

            Debug.Log("🏠 Đã vào khu vực hồi phục.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (stationHighlight) stationHighlight.color = idleColor;
            if (refillEffect) refillEffect.Stop(); // Tắt hiệu ứng hạt
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetPlayerAtHomeBase(false);
            }
            
            Debug.Log("🌊 Đã rời khu vực hồi phục.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
