using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Thanh chỉ số")]
    [SerializeField] private Slider oxygenSlider;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private TextMeshProUGUI artifactText;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Hiệu ứng Cảnh báo")]
    [SerializeField] private Image dangerOverlay; // Lớp phủ đỏ toàn màn hình (Vignette hoặc Transparent Red)
    [SerializeField] private float minFlashSpeed = 2f;
    [SerializeField] private float maxFlashSpeed = 10f;
    [SerializeField] private float maxOverlayAlpha = 0.5f; // Độ đậm tối đa của lớp phủ

    [Header("Màn hình thông báo")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    private Coroutine flashCoroutine;
    private OxygenSystem playerOxygen;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerOxygen = player.GetComponent<OxygenSystem>();
            if (playerOxygen != null)
            {
                playerOxygen.OnOxygenChanged += UpdateOxygen;
                playerOxygen.OnOxygenWarning += HandleOxygenWarning;
            }

            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null) 
            {
                health.OnHealthChanged += UpdateHealth;
                health.OnDeath += ShowGameOver;
            }

            BatterySystem battery = player.GetComponent<BatterySystem>();
            if (battery != null)
            {
                battery.OnBatteryChanged += UpdateBattery;
            }
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrency;
            UpdateCurrency(CurrencyManager.Instance.GetCurrentCurrency());
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnArtifactCollected += UpdateArtifactCount;
            LevelManager.Instance.OnLevelComplete += ShowWinScreen;
        }

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);

        // Ban đầu ẩn lớp phủ
        if (dangerOverlay)
        {
            Color c = dangerOverlay.color;
            c.a = 0;
            dangerOverlay.color = c;
        }
    }

    private void UpdateOxygen(float current, float max)
    {
        if (oxygenSlider) oxygenSlider.value = current / max;
    }

    private void HandleOxygenWarning(bool isWarning)
    {
        if (isWarning)
        {
            if (flashCoroutine == null)
                flashCoroutine = StartCoroutine(FlashDangerOverlay());
        }
        else
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
                // Đảm bảo lớp phủ biến mất khi hết cảnh báo
                if (dangerOverlay)
                {
                    Color c = dangerOverlay.color;
                    c.a = 0;
                    dangerOverlay.color = c;
                }
            }
        }
    }

    private IEnumerator FlashDangerOverlay()
    {
        while (true)
        {
            if (playerOxygen == null || dangerOverlay == null) yield break;

            float oxygenPct = playerOxygen.GetOxygenPercentage();
            float currentFlashSpeed = Mathf.Lerp(maxFlashSpeed, minFlashSpeed, oxygenPct / 0.2f);

            float t = (Mathf.Sin(Time.time * currentFlashSpeed) + 1f) / 2f;
            float targetAlpha = t * maxOverlayAlpha;

            Color c = dangerOverlay.color;
            c.a = targetAlpha;
            dangerOverlay.color = c;

            yield return null;
        }
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthSlider) healthSlider.value = current / max;
    }

    private void UpdateArtifactCount(int collected, int required)
    {
        if (artifactText) artifactText.text = $"Cổ vật: {collected}/{required}";
    }

    private void ShowWinScreen()
    {
        if (winPanel) winPanel.SetActive(true);
    }

    private void UpdateBattery(float current, float max)
    {
        if (batterySlider) batterySlider.value = current / max;
    }

    private void UpdateCurrency(int amount)
    {
        if (currencyText) currencyText.text = $"Credits: {amount}";
    }

    public void TriggerPressureWarning()
    {
        if (dangerOverlay != null)
        {
            StartCoroutine(FlashOnce());
        }
    }

    private IEnumerator FlashOnce()
    {
        float timer = 0;
        float duration = 0.5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(maxOverlayAlpha, 0, timer / duration);
            Color c = dangerOverlay.color;
            c.a = alpha;
            dangerOverlay.color = c;
            yield return null;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }
}
