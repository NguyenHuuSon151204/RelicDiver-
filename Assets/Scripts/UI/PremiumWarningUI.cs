using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PremiumWarningUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    
    [Header("Cấu hình hiệu ứng")]
    public float fadeSpeed = 5f;
    public float pulseSpeed = 2f;
    public float minPulseAlpha = 0.4f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0; // Bắt đầu ẩn hoàn toàn
    }

    public void ShowWarning(bool show)
    {
        isShowing = show;
    }

    void Update()
    {
        // 1. Hiệu ứng Fade In / Fade Out mượt mà
        float targetAlpha = isShowing ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // 2. Hiệu ứng Nhấp nháy nhịp điệu (Pulse) khi đang hiện
        if (isShowing && canvasGroup.alpha > 0.9f)
        {
            float pulse = minPulseAlpha + (1f - minPulseAlpha) * (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f);
            canvasGroup.alpha = pulse;
        }
    }
}
