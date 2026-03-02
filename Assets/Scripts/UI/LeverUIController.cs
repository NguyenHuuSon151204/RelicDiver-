using UnityEngine;
using UnityEngine.UI;

public class LeverUIController : MonoBehaviour
{
    [Header("Cấu hình Cần gạt (Sliders)")]
    [SerializeField] private Slider speedLever;
    [SerializeField] private Slider lightLever;

    [Header("Tham chiếu hệ thống")]
    [SerializeField] private DiverController playerController;
    [SerializeField] private BatterySystem batterySystem;
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D submarineLight;

    [Header("Cấu hình ánh sáng")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float minOuterRadius = 3f;
    [SerializeField] private float maxOuterRadius = 10f;

    private void Start()
    {
        // Tự động tìm player nếu chưa gán
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<DiverController>();
                batterySystem = player.GetComponent<BatterySystem>();
            }
        }

        // Đăng ký sự kiện khi Slider thay đổi
        if (speedLever)
        {
            speedLever.onValueChanged.AddListener(OnSpeedLeverChanged);
            // Khởi tạo giá trị mặc định cho Diver
            OnSpeedLeverChanged(speedLever.value);
        }

        if (lightLever)
        {
            lightLever.onValueChanged.AddListener(OnLightLeverChanged);
            // Khởi tạo giá trị mặc định cho Ánh sáng
            OnLightLeverChanged(lightLever.value);
        }
    }

    private void OnSpeedLeverChanged(float value)
    {
        if (playerController) playerController.SetLeverSpeedMultiplier(value);
        if (batterySystem) batterySystem.SetSpeedDrain(value);
        
        Debug.Log($"Cần gạt Tốc độ: {value * 100}%");
    }

    private void OnLightLeverChanged(float value)
    {
        if (submarineLight)
        {
            submarineLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, value);
            submarineLight.pointLightOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, value);
        }

        if (batterySystem) batterySystem.SetLightDrain(value);

        Debug.Log($"Cần gạt Đèn: {value * 100}%");
    }

    private void OnDestroy()
    {
        if (speedLever) speedLever.onValueChanged.RemoveListener(OnSpeedLeverChanged);
        if (lightLever) lightLever.onValueChanged.RemoveListener(OnLightLeverChanged);
    }
}
