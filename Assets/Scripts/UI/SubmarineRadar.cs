using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SubmarineRadar : MonoBehaviour
{
    [Header("Cấu hình Radar")]
    [SerializeField] private float scanRange = 20f;
    [SerializeField] private float scanDuration = 2f;
    [SerializeField] private KeyCode scanKey = KeyCode.Space;
    
    [Header("Giao diện Radar")]
    [SerializeField] private RectTransform radarScreen; // Cái khung hình tròn Radar
    [SerializeField] private GameObject pingPrefab; // Cái chấm Icon trên Radar
    [SerializeField] private Image scanWaveUI; // Hiệu ứng vòng tròn lan tỏa trên UI

    private List<GameObject> activePings = new List<GameObject>();
    private bool isScanning = false;
    private float scanTimer = 0f;

    void Update()
    {
        // Chỉ quét khi đang ở trong tàu (Tận dụng biến isInside từ SubmarineStation)
        if (Input.GetKeyDown(scanKey) && !isScanning)
        {
            StartPing();
        }

        if (isScanning)
        {
            UpdateScanEffect();
        }
    }

    private void StartPing()
    {
        isScanning = true;
        scanTimer = 0f;
        
        // 1. Xóa các chấm cũ
        ClearPings();

        // 2. Tìm tất cả mục tiêu trong tầm
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRange);
        
        foreach (var hit in hits)
        {
            RadarTarget target = hit.GetComponent<RadarTarget>();
            if (target != null)
            {
                CreatePingOnRadar(target);
            }
        }
        
        Debug.Log("<color=lime>Radar: Đang quét...</color>");
    }

    private void CreatePingOnRadar(RadarTarget target)
    {
        if (pingPrefab == null) 
        {
            Debug.LogError("Chưa gán Ping Prefab cho SubmarineRadar! Hãy kéo một UI Image vào ô Ping Prefab trong Inspector.");
            return;
        }

        // Tính toán vị trí tương đối từ tàu đến mục tiêu
        Vector2 diff = target.transform.position - transform.position;
        
        // Chuyển đổi vị trí thế giới sang tọa độ Radar (Scale lại cho khớp khung tròn)
        float radarRadius = radarScreen.rect.width / 2;
        Vector2 radarPos = (diff / scanRange) * radarRadius;

        // Tạo Icon
        GameObject ping = Instantiate(pingPrefab, radarScreen);
        RectTransform rt = ping.GetComponent<RectTransform>();
        rt.anchoredPosition = radarPos;

        // Cài đặt hình ảnh và màu sắc
        Image img = ping.GetComponent<Image>();
        if (target.radarIcon) img.sprite = target.radarIcon;
        img.color = target.iconColor;

        activePings.Add(ping);
        
        // Hiệu ứng mờ dần cho Icon
        StartCoroutine(FadeOutPing(img));
    }

    private void UpdateScanEffect()
    {
        scanTimer += Time.deltaTime;
        float progress = scanTimer / scanDuration;

        // Hiệu ứng vòng tròn lan tỏa trên UI
        if (scanWaveUI)
        {
            scanWaveUI.gameObject.SetActive(true);
            scanWaveUI.rectTransform.localScale = Vector3.one * progress * 2f;
            Color c = scanWaveUI.color;
            c.a = 1f - progress;
            scanWaveUI.color = c;
        }

        if (progress >= 1f)
        {
            isScanning = false;
            if (scanWaveUI) scanWaveUI.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeOutPing(Image img)
    {
        float t = 0f;
        while (t < scanDuration)
        {
            t += Time.deltaTime;
            Color c = img.color;
            c.a = 1f - (t / scanDuration);
            img.color = c;
            yield return null;
        }
        Destroy(img.gameObject);
    }

    private void ClearPings()
    {
        foreach (var p in activePings) if(p) Destroy(p);
        activePings.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}
