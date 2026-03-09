using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class TimeCycleManager : MonoBehaviour
{
    [Header("Cấu hình Thời gian")]
    [SerializeField] private float dayDurationSeconds = 120f; // Một ngày dài 2 phút
    [SerializeField] private Light2D globalLight;
    [SerializeField] private TextMeshProUGUI timeText; // Kéo Text hiển thị giờ vào đây
    
    [Header("Giờ trong game")]
    [SerializeField] private int startHour = 8; // Bắt đầu lúc 8h sáng
    [SerializeField] private int endHour = 22;  // Kết thúc lúc 10h tối (22h)

    [Header("Cấu hình Ánh sáng")]
    [SerializeField] private float maxIntensity = 1.0f; 
    [SerializeField] private float minIntensity = 0.05f; 

    private float currentTime = 0f;
    private bool isNight = false;

    public System.Action OnNightFall;

    private void Start()
    {
        if (globalLight == null)
        {
            Light2D[] allLights = FindObjectsOfType<Light2D>();
            foreach (var light in allLights)
            {
                if (light.lightType == Light2D.LightType.Global)
                {
                    globalLight = light;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        float progress = currentTime / dayDurationSeconds;

        if (progress <= 1.0f)
        {
            // ĐÃ GỠ BỎ: Để AtmosphereMaster quản lý toàn bộ ánh sáng
            // float intensityMultiplier = Mathf.Lerp(maxIntensity, minIntensity, progress);
            // if (globalLight != null) { globalLight.intensity = intensityMultiplier; }

            // Cập nhật đồng hồ UI
            UpdateClockUI(progress);

            // Xử lý sự kiện ban đêm
            if (progress > 0.8f && !isNight)
            {
                isNight = true;
                OnNightFall?.Invoke();
                Debug.Log("<color=blue>TRỜI ĐÃ TỐI:</color> Cẩn thận quái vật đêm!");
            }
        }
    }

    private void UpdateClockUI(float progress)
    {
        if (timeText == null) return;

        // Tính toán giờ phút dựa trên % tiến trình của ngày
        float totalHoursInGame = endHour - startHour;
        float currentInGameHour = startHour + (progress * totalHoursInGame);
        
        int hours = Mathf.FloorToInt(currentInGameHour);
        int minutes = Mathf.FloorToInt((currentInGameHour - hours) * 60f);

        // Định dạng hiển thị 00:00
        timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);

        // Đổi màu đỏ khi sắp hết giờ (sau 20h)
        if (hours >= 20) timeText.color = Color.red;
        else timeText.color = Color.white;
    }

    public float GetTimeProgress() => currentTime / dayDurationSeconds;
}
