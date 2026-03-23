using UnityEngine;
using System.Collections;

public class InteractiveRock : MonoBehaviour
{
    [Header("--- Cấu hình ---")]
    public GameObject hiddenItem;       // Vật phẩm giấu bên dưới
    public float interactDistance = 3f;  // Khoảng cách thợ lặn có thể click
    public bool isFake = false;         // Đá giả, không có đồ

    [Header("--- Hiệu ứng ---")]
    public GameObject glowEffect;       // Hiệu ứng phát sáng khi lại gần
    public float glowDistance = 5f;     // Khoảng cách bắt đầu phát sáng
    public AudioClip tiltSound;         // Âm thanh lật đá

    private bool isOpened = false;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Collider2D rockCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rockCollider = GetComponent<Collider2D>();
        
        // Luôn để đá là Trigger để người chơi đi xuyên qua được
        if (rockCollider) rockCollider.isTrigger = true;

        // Tắt con của hiddenItem (Collider và Sprite của vật phẩm) lúc đầu
        if (hiddenItem != null)
        {
            Collider2D itemCol = hiddenItem.GetComponent<Collider2D>();
            if (itemCol) itemCol.enabled = false;

            SpriteRenderer itemSr = hiddenItem.GetComponent<SpriteRenderer>();
            if (itemSr) itemSr.enabled = false;
        }

        if (glowEffect) glowEffect.SetActive(false);

        // Tìm player bằng tag
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.transform;
    }

    void Update()
    {
        if (isOpened) return;

        // 1. Hiệu ứng phát sáng khi lại gần (Chỉ chạy nếu có player)
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (glowEffect != null)
            {
                glowEffect.SetActive(dist <= glowDistance);
            }
        }
        else // Nếu không có player, đảm bảo hiệu ứng tắt
        {
            if (glowEffect != null)
            {
                glowEffect.SetActive(false);
            }
        }

        // 2. Tự kiểm tra Click chuột (Cách này nhạy và xuyên thấu tốt hơn OnMouseDown)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (rockCollider.OverlapPoint(mousePos))
            {
                OpenRock();
                Debug.Log("<color=cyan>Đã lật đá thành công (Logic xuyên thấu)!</color>");
            }
        }
    }

    void OpenRock()
    {
        isOpened = true;
        if (glowEffect) glowEffect.SetActive(false);

        // Phát âm thanh
        if (tiltSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(tiltSound);

        // Chạy animation lật và biến mất
        StartCoroutine(TiltAndFadeRoutine());
    }

    IEnumerator TiltAndFadeRoutine()
    {
        float duration = 0.6f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        float startZ = transform.eulerAngles.z;
        Vector3 startScale = transform.localScale;

        // Random hướng lật (trái hoặc phải)
        float flipDir = Random.value > 0.5f ? 1f : -1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = Mathf.Sin(t * Mathf.PI); // Tạo độ nảy (Arc)

            // 1. Vị trí: Bay lên và sang bên
            transform.position = startPos + new Vector3(t * 2f * flipDir, curve * 1.5f, 0);

            // 2. Xoay: Lật mạnh hơn (90-120 độ)
            transform.eulerAngles = new Vector3(0, 0, startZ + (t * 120f * -flipDir));

            // 3. Thu nhỏ và mờ dần
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            if (spriteRenderer)
            {
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1, 0, t);
                spriteRenderer.color = c;
            }

            yield return null;
        }

        // Sau khi đá biến mất: Kích hoạt vật phẩm bên dưới
        if (!isFake && hiddenItem != null)
        {
            Collider2D itemCol = hiddenItem.GetComponent<Collider2D>();
            if (itemCol) itemCol.enabled = true;

            SpriteRenderer itemSr = hiddenItem.GetComponent<SpriteRenderer>();
            if (itemSr) itemSr.enabled = true;
            
            Debug.Log("<color=green>Đã tìm thấy đồ vật!</color>");
        }

        // Xóa hoàn toàn tảng đá
        Destroy(gameObject);
    }
}
