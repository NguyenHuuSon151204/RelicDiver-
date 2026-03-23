using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Status Bars")]
    [SerializeField] private Slider oxygenSlider;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private SegmentedStatBar batteryBar; // Thanh pin chia vạch mới
    [SerializeField] private TextMeshProUGUI artifactText;
    [SerializeField] private TextMeshProUGUI photoText;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color timerNormalColor = Color.white;
    [SerializeField] private Color timerWarningColor = Color.red;
    private float levelTimeLimit = 0f;
    private float currentTimeLeft = 0f;
    private bool timerActive = false;

    [Header("Mission UI")]
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private float typewriterSpeed = 0.05f;
    private Coroutine typewriterCoroutine;

    [Header("Pulsing Effects (Low Stats)")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image oxygenFillImage;
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private Color healthPulseColor = Color.red;
    [SerializeField] private Color oxygenPulseColor = new Color(0, 0.8f, 1f, 1f);

    [Header("Warning Effects")]
    [SerializeField] private Image dangerOverlay; 
    [SerializeField] private float minFlashSpeed = 2f;
    [SerializeField] private float maxFlashSpeed = 10f;
    [SerializeField] private float maxOverlayAlpha = 0.5f;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [Header("Win/Lose Details")]
    [SerializeField] private TextMeshProUGUI winDetailsText;
    [SerializeField] private TextMeshProUGUI loseDetailsText;

    [Header("Screen Transitions")]
    [SerializeField] private CanvasGroup screenFaderGroup;
    [SerializeField] private float fadeDuration = 0.8f;

    private PlayerStatusManager playerStatus;
    private Coroutine flashCoroutine;
    private Coroutine fadeCoroutine;

    private bool isResultShowing = false;

    [Header("Pause & Settings")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Selectable firstButtonPause;
    [SerializeField] private Selectable firstButtonSettings;
    [SerializeField] private GameObject gameplayHUDGroup; // Kéo nhóm các thanh Sliders, Pin, Text vào đây
    public static HUDController Instance { get; private set; }
    private bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        isResultShowing = false; // Reset trạng thái
        isPaused = false;
        Time.timeScale = 1f; // Đảm bảo game chạy bình thường khi vào màn

        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        // Load volume vào Sliders
        if (AudioManager.Instance != null)
        {
            if (musicSlider) musicSlider.value = AudioManager.Instance.GetMusicVolume();
            if (sfxSlider) sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        }

        // Load độ sáng vào Slider
        if (brightnessSlider != null && AtmosphereMaster.Instance != null)
        {
            brightnessSlider.value = AtmosphereMaster.Instance.GetBrightness();
        }

        // Đảm bảo fader ẩn lúc đầu
        if (screenFaderGroup != null)
        {
            screenFaderGroup.alpha = 0f;
            screenFaderGroup.blocksRaycasts = false;
        }

        // Thử tìm Player ngay lúc đầu
        TryFindAndSubscribeToPlayer();


        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnArtifactCollected += UpdateArtifactCount;
            LevelManager.Instance.OnPhotoTaken += UpdatePhotoCount;
            LevelManager.Instance.OnLevelComplete += ShowWinScreen;

            // Khởi tạo Text nhiệm vụ ban đầu dựa trên MissionManager nếu có
            if (MissionManager.Instance == null)
            {
                UpdateMissionText("Objective: Find relics and return to surface.");
            }

            // Handle submarine UI
            if (batterySlider != null)
            {
                batterySlider.gameObject.SetActive(LevelManager.Instance.allowSubmarine);
            }

            // Chỉ ẩn thanh pin nếu hiện tại chưa ở trong tàu
            if (batteryBar != null && currentSubmarine == null) 
                batteryBar.gameObject.SetActive(false);

            // Ban đầu ẩn text ảnh nếu không có nhiệm vụ ảnh (Ảnh đã bị gỡ bỏ mặc định ẩn)
            if (photoText != null) photoText.gameObject.SetActive(false);
        }
        else if (MissionManager.Instance == null)
        {
            UpdateMissionText("Objective: Find relics and return to surface.");
        }

        // Initialize Timer properly
        levelTimeLimit = 300f; // 5 minutes
        currentTimeLeft = levelTimeLimit;
        timerActive = true;

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

    private void Update()
    {
        // Nhấn ESC để tạm dừng
        if (Input.GetKeyDown(KeyCode.Escape) && !isResultShowing)
        {
            TogglePause();
        }

        if (isPaused) return; // Nếu đang pause thì không xử lý các thứ khác

        // Nếu vẫn chưa tìm thấy Player...
        if (playerStatus == null)
        {
            TryFindAndSubscribeToPlayer();
        }

        HandleLowStatPulsing();
        HandleTimer();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel) pausePanel.SetActive(isPaused);
        if (!isPaused && settingsPanel) settingsPanel.SetActive(false); // Ẩn settings nếu bỏ pause
        
        // Hiện/Ẩn và Khóa/Mở con trỏ chuột
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (firstButtonPause != null) firstButtonPause.Select();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OpenSettings()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (pausePanel) pausePanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void OnMusicVolumeChanged(float value)
    {
        Debug.Log($"HUD: Slider Nhạc đang gửi giá trị {value}");
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
        else
            Debug.LogError("HUD LỖI: Không tìm thấy AudioManager Instance!");
    }

    public void OnSFXVolumeChanged(float value)
    {
        Debug.Log($"HUD: Slider SFX đang gửi giá trị {value}");
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
        else
            Debug.LogError("HUD LỖI: Không tìm thấy AudioManager Instance!");
    }

    public void OnBrightnessChanged(float value)
    {
        Debug.Log($"HUD: Slider Độ sáng đang gửi {value}");
        if (AtmosphereMaster.Instance != null)
        {
            AtmosphereMaster.Instance.SetBrightness(value);
        }
        else
        {
            Debug.LogError("HUD LỖI: Không tìm thấy AtmosphereMaster Instance!");
        }
    }

    private void HandleTimer()
    {
        if (!timerActive || isResultShowing) return;

        if (currentTimeLeft > 0)
        {
            // DÙNG unscaledDeltaTime để thời gian đếm thật, ko bị Pause làm đứng đồng hồ
            currentTimeLeft -= Time.unscaledDeltaTime;
            
            if (timerText != null)
            {
                UpdateTimerDisplay(currentTimeLeft);
                if (currentTimeLeft <= 10f)
                {
                    float pulse = (Mathf.Sin(Time.unscaledTime * 15f) + 1f) / 2f;
                    timerText.color = Color.Lerp(timerNormalColor, timerWarningColor, pulse);
                    timerText.transform.localPosition += (Vector3)Random.insideUnitCircle * 0.5f;
                }
            }
        }
        else
        {
            currentTimeLeft = 0;
            timerActive = false;
            if (timerText != null) UpdateTimerDisplay(0);
            ShowGameOver("Time Expired"); 
        }
    }

    private void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void HandleLowStatPulsing()
    {
        // Nháy thanh Máu khi < 20% (Chỉ nháy nếu thanh Fill tồn tại)
        if (playerStatus != null && healthSlider != null && healthFillImage != null)
        {
            if (healthSlider.value < 0.2f)
            {
                float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                healthFillImage.color = Color.Lerp(Color.white, healthPulseColor, alpha);
            }
            else
            {
                healthFillImage.color = Color.white;
            }
        }

        // Nháy thanh Oxy khi < 20%
        if (playerStatus != null && oxygenSlider != null && oxygenFillImage != null)
        {
            if (oxygenSlider.value < 0.2f)
            {
                float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                oxygenFillImage.color = Color.Lerp(Color.white, oxygenPulseColor, alpha);
            }
            else
            {
                oxygenFillImage.color = Color.white;
            }
        }
    }

    private void TryFindAndSubscribeToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStatus = player.GetComponent<PlayerStatusManager>();
            if (playerStatus != null)
            {
                playerStatus.OnOxygenChanged += UpdateOxygen;
                playerStatus.OnOxygenWarning += HandleOxygenWarning;
                playerStatus.OnHealthChanged += UpdateHealth;
                playerStatus.OnBatteryChanged += UpdateBattery;
                playerStatus.OnDeath += () => ShowGameOver(); // Gọi ShowGameOver mặc định
                
                // Cập nhật giá trị ban đầu ngay khi tìm thấy
                UpdateOxygen(playerStatus.currentOxygen, playerStatus.maxOxygen);
                UpdateHealth(playerStatus.currentHealth, playerStatus.maxHealth);
                UpdateBattery(playerStatus.currentBattery, playerStatus.maxBattery);
            }
        }
    }

    public void UpdateMissionText(string newText)
    {
        if (missionText == null) return;
        
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterEffect(newText));
    }

    private IEnumerator TypewriterEffect(string text)
    {
        missionText.text = "";
        foreach (char c in text)
        {
            missionText.text += c;
            yield return new WaitForSecondsRealtime(typewriterSpeed);
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
            if (playerStatus == null || dangerOverlay == null) yield break;

            float oxygenPct = playerStatus.GetOxygenPercentage();
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
        if (artifactText) artifactText.text = $"Relics: {collected}/{required}";
        UpdateCommonMissionText();
    }

    private void UpdatePhotoCount(int taken, int required)
    {
        if (photoText) photoText.text = $"Photos: {taken}/{required}";
        UpdateCommonMissionText();
    }

    private void UpdateCommonMissionText()
    {
        if (LevelManager.Instance == null) return;
        
        // Nếu có MissionManager thì để nó tự cập nhật UI, không ghi đè ở đây nữa
        if (MissionManager.Instance != null) return;

        string mission = $"Progress - relics: {LevelManager.Instance.GetCollectedCount()}/{LevelManager.Instance.GetTargetRelics()}";
        // Tính năng ảnh đã bị gỡ bỏ
        UpdateMissionText(mission);
    }

    private void ShowWinScreen()
    {
        if (isResultShowing) return;
        isResultShowing = true;
        timerActive = false;
        
        if (winPanel == null)
        {
            Debug.LogError("<color=red>HUD: CHƯA KÉO Win Panel VÀO INSPECTOR!</color>");
            return;
        }

        winPanel.SetActive(true);
        CanvasGroup cg = winPanel.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f; 

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeScreen(1f, () => {
            if (cg != null) cg.alpha = 1f;

            if (winDetailsText != null)
            {
                float timeTaken = levelTimeLimit - currentTimeLeft;
                int minutes = Mathf.FloorToInt(timeTaken / 60);
                int seconds = Mathf.FloorToInt(timeTaken % 60);
                string timeStr = string.Format("{0:00}:{1:00}", minutes, seconds);

                int collected = LevelManager.Instance != null ? LevelManager.Instance.GetCollectedCount() : 0;
                int total = LevelManager.Instance != null ? LevelManager.Instance.GetTargetRelics() : 1;

                string details = $"ELAPSED TIME: <color=#FFD700>{timeStr}</color>\n" +
                                 $"RELICS FOUND: <color=#FFD700>{collected}/{total}</color>\n" +
                                 $"STATUS: <color=#00FF00>SUCCESSFUL</color>";
                
                StartCoroutine(ShowDetailsTypewriter(winDetailsText, details));
            }
            else
            {
                Debug.LogWarning("<color=yellow>HUD: CHƯA KÉO Win Details Text VÀO INSPECTOR!</color>");
            }
        }));
    }

    private SubmarineStation currentSubmarine;

    public void SubscribeToSubmarine(SubmarineStation sub)
    {
        if (currentSubmarine != null) currentSubmarine.OnBatteryChanged -= UpdateBattery;
        
        currentSubmarine = sub;
        currentSubmarine.OnBatteryChanged += UpdateBattery;
        UpdateBattery(currentSubmarine.GetCurrentBattery(), currentSubmarine.GetMaxBattery());

        // Hiện thanh pin khi vào tàu
        if (batteryBar != null) batteryBar.gameObject.SetActive(true);
    }

    public void UnsubscribeFromSubmarine()
    {
        if (currentSubmarine != null)
        {
            currentSubmarine.OnBatteryChanged -= UpdateBattery;
            currentSubmarine = null;
        }

        // Ẩn thanh pin khi rời tàu
        if (batteryBar != null) batteryBar.gameObject.SetActive(false);

        if (playerStatus != null)
        {
            UpdateBattery(playerStatus.currentBattery, playerStatus.maxBattery);
        }
    }

    private void UpdateBattery(float current, float max)
    {
        if (batterySlider) batterySlider.value = current / max;
        if (batteryBar) batteryBar.UpdateValue(current, max);
    }

    private void UpdateCurrency(int amount)
    {
        // Đã gỡ bỏ CurrencyManager
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

    // --- CÁC HÀM ĐIỀU HƯỚNG DÀNH CHO NÚT BẤM ---

    public void OnRetryClicked()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
        }
    }

    public void OnNextLevelClicked()
    {
        int currentBuildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        
        // Mở khóa màn tiếp theo trước khi chuyển cảnh
        LevelSelectionController.UnlockNextLevel(currentBuildIndex);

        int nextSceneIndex = currentBuildIndex + 1;
        if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0); 
        }
    }

    public void OnMainMenuClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); 
    }

    public void ShowGameOver(string customReason = "")
    {
        if (isResultShowing) return;
        isResultShowing = true;
        timerActive = false;

        if (gameOverPanel == null)
        {
            Debug.LogError("<color=red>HUD: CHƯA KÉO Lose Panel VÀO INSPECTOR!</color>");
            return;
        }

        gameOverPanel.SetActive(true);
        CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeScreen(1f, () => {
            if (cg != null) cg.alpha = 1f;

            if (loseDetailsText != null)
            {
                string reason = string.IsNullOrEmpty(customReason) ? 
                                (playerStatus != null ? playerStatus.GetLastDamageSource() : "Unknown Failure") : 
                                customReason;
                float depth = playerStatus != null ? playerStatus.GetCurrentDepth() : 0f;

                string details = $"DEPTH REACHED: <color=#FF4500>{depth:F1}M</color>\n" +
                                 $"REASON: <color=#FF4500>{reason}</color>\n" +
                                 $"SIGNAL: <color=#808080>LOST</color>";

                Debug.Log("<color=cyan>Đang bắt đầu hiện chữ bảng Lose...</color>");
                StartCoroutine(ShowDetailsTypewriter(loseDetailsText, details));
            }
            else
            {
                Debug.LogWarning("<color=yellow>HUD: CHƯA KÉO Lose Details Text VÀO INSPECTOR!</color>");
            }
        }));
    }

    private IEnumerator FadeScreen(float targetAlpha, System.Action onComplete = null)
    {
        if (screenFaderGroup == null)
        {
            Debug.LogWarning("HUD: Thiếu Screen Fader Group!");
            onComplete?.Invoke();
            yield break;
        }

        float startAlpha = screenFaderGroup.alpha;
        float timer = 0f;

        if (targetAlpha > 0.5f) screenFaderGroup.blocksRaycasts = true;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            screenFaderGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        screenFaderGroup.alpha = targetAlpha;
        if (targetAlpha <= 0.1f) screenFaderGroup.blocksRaycasts = false;
        
        onComplete?.Invoke();
    }

    private IEnumerator ShowDetailsTypewriter(TextMeshProUGUI textObj, string content)
    {
        textObj.text = "";
        yield return new WaitForSecondsRealtime(0.2f); // Chờ 1 nhịp ngắn sau khi bảng hiện ra
        
        bool inTag = false;
        string currentText = "";
        
        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            if (c == '<') inTag = true;
            currentText += c;
            if (c == '>') inTag = false;

            if (!inTag)
            {
                textObj.text = currentText;
                yield return new WaitForSecondsRealtime(0.02f);
            }
        }
    }

    public void SetHUDVisible(bool visible)
    {
        if (gameplayHUDGroup != null) gameplayHUDGroup.SetActive(visible);
    }
}
