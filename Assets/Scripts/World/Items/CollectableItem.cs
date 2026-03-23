using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    public string itemName = "Vật phẩm";
    public GameObject collectEffect;
    public AudioClip collectSound;

    [Header("--- Hiệu ứng Highlight ---")]
    public GameObject highlightObject; // Đối tượng sáng viền (thường là bản sao sprite màu trắng/glow)
    public float highlightDistance = 3f;

    [Header("Tutorial Settings")]
    public bool isFirstItem = false;

    private Transform playerTransform;

    private void Start()
    {
        // Ban đầu ẩn viền sáng
        if (highlightObject) highlightObject.SetActive(false);
        
        // Tìm player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) playerTransform = p.transform;
    }

    private void Update()
    {
        if (playerTransform == null || highlightObject == null) return;

        // Tính khoảng cách đến player
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        
        // Bật/Tắt viền sáng dựa trên khoảng cách
        highlightObject.SetActive(dist <= highlightDistance);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra nếu là Player (Thợ lặn) hoặc Tàu ngầm chạm vào
        if (other.CompareTag("Player") || other.GetComponentInParent<SubmarineStation>() != null)
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Cộng điểm vào Level Manager và Mission Manager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddRelic();
        }

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.AddProgress(itemName);
        }

        // Tạo hiệu ứng thu thập nếu có
        if (collectEffect)
        {
            GameObject fx = Instantiate(collectEffect, transform.position, Quaternion.identity);
            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        if (collectSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(collectSound);
        }

        Debug.Log($"<color=cyan>Đã thu thập:</color> {itemName}");
        
        // Biến mất khỏi màn chơi
        Destroy(gameObject);
    }
}
