using UnityEngine;
using UnityEngine.UI;

public class LeverUIController : MonoBehaviour
{
    public static LeverUIController Instance { get; private set; }

    [Header("Cấu hình Cần gạt (Sliders)")]
    [SerializeField] private GameObject speedLeverRoot; // Kéo cái GameObject chứa thanh tốc độ vào đây
    [SerializeField] private Slider speedLever;
    [SerializeField] private Slider lightLever;

    [Header("Tham chiếu hệ thống")]
    [SerializeField] private DiverController playerController;
    private PlayerStatusManager playerStatus;
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D currentLight;

    [Header("Cấu hình ánh sáng")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float minOuterRadius = 3f;
    [SerializeField] private float maxOuterRadius = 10f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Khóa điều hướng bàn phím
        if (speedLever) DisableNavigation(speedLever);
        if (lightLever) DisableNavigation(lightLever);

        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<DiverController>();
                playerStatus = player.GetComponent<PlayerStatusManager>();
                
                if (currentLight == null)
                    currentLight = player.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
            }
        }

        if (speedLever) speedLever.onValueChanged.AddListener(OnSpeedLeverChanged);
        if (lightLever)
        {
            lightLever.onValueChanged.AddListener(OnLightLeverChanged);
            lightLever.value = 0.5f;
            OnLightLeverChanged(0.5f);
        }
        
        // KHÔNG ép ẩn ở đây nữa để tránh ghi đè lệnh từ SubmarineStation.Start()
        // Bạn hãy tắt speedLeverRoot chủ động trong Unity Inspector là được.
    }

    private void DisableNavigation(Slider slider)
    {
        Navigation nav = slider.navigation;
        nav.mode = Navigation.Mode.None;
        slider.navigation = nav;
    }

    // HÀM QUAN TRỌNG: Gọi khi lên/xuống tàu
    public void SwitchLightTarget(UnityEngine.Rendering.Universal.Light2D newLight)
    {
        currentLight = newLight;
        // Cập nhật ngay lập tức theo giá trị hiện tại của cần gạt
        OnLightLeverChanged(lightLever.value);
    }

    public void ShowSpeedSlider(bool show)
    {
        if (speedLeverRoot) speedLeverRoot.SetActive(show);
    }

    private void OnSpeedLeverChanged(float value)
    {
        // Chỉ điều khiển nếu thanh này đang hiện (tức là đang ở trong tàu)
        // Lưu ý: Tôi sẽ cập nhật SubmarineStation để nhận giá trị này
        Debug.Log($"Cần gạt Tốc độ Tàu: {value * 100}%");
    }

    private void OnLightLeverChanged(float value)
    {
        if (currentLight)
        {
            currentLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, value);
            currentLight.pointLightOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, value);
        }

        if (playerStatus != null) playerStatus.SetLightBatteryDrain(value);
    }

    public float GetSpeedLeverValue() => speedLever ? speedLever.value : 0f;

    private void OnDestroy()
    {
        if (speedLever) speedLever.onValueChanged.RemoveListener(OnSpeedLeverChanged);
        if (lightLever) lightLever.onValueChanged.RemoveListener(OnLightLeverChanged);
    }
}
