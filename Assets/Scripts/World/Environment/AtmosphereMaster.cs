using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosphereMaster : MonoBehaviour
{
    [Header("Mốc mặt nước")]
    public float waterLevelY = 100f;
    
    [Header("Ánh sáng (Global Light)")]
    public Light2D globalLight;
    
    [Header("--- TRÊN MẶT NƯỚC (Giờ giấc) ---")]
    public Color dayColor = Color.white;
    public Color sunsetColor = new Color(1f, 0.6f, 0.4f);
    public Color nightColor = new Color(0.1f, 0.1f, 0.3f);
    public float aboveIntensity = 1.0f;
    
    [Header("--- DƯỚI MẶT NƯỚC (Độ sâu) ---")]
    public Color waterSurfaceColor = new Color(0.1f, 0.3f, 0.6f); // Xanh biển đậm
    public Color waterDeepColor = new Color(0.01f, 0.02f, 0.05f); // Đen biển sâu
    public float underIntensity = 0.5f; // Giảm cường độ
    public float maxDarknessDepth = 150f;

    [Header("Độ mượt ranh giới")]
    public float transitionRange = 2f; // Khoảng cách để hòa quyện màu sắc

    [Header("Thành phần hỗ trợ")]
    public Volume underwaterVolume;
    public Camera mainCam;

    public static AtmosphereMaster Instance { get; private set; }
    private float brightnessMultiplier = 1f;
    private TimeCycleManager timeManager;

    void Awake()
    {
        Instance = this;
        // Mặc định là 1.0 nếu chưa có dữ liệu lưu
        brightnessMultiplier = PlayerPrefs.GetFloat("Brightness", 1f);
        
        // Đề phòng trường hợp Slider lưu nhầm giá trị 0
        if (brightnessMultiplier <= 0.05f) brightnessMultiplier = 1f;
    }

    public void SetBrightness(float value)
    {
        brightnessMultiplier = value;
        PlayerPrefs.SetFloat("Brightness", value);
    }

    public float GetBrightness() => brightnessMultiplier;

    void Start()
    {
        timeManager = FindObjectOfType<TimeCycleManager>();
        if (mainCam == null) mainCam = Camera.main;
        
        // Cố gắng tìm lại đèn nếu bị mất
        if (globalLight == null)
        {
            Light2D[] lights = FindObjectsOfType<Light2D>();
            foreach (var l in lights) if (l.lightType == Light2D.LightType.Global) { globalLight = l; break; }
        }
    }

    void LateUpdate() 
    {
        if (globalLight == null || mainCam == null) return;

        float currentY = mainCam.transform.position.y;
        float timeProgress = (timeManager != null) ? timeManager.GetTimeProgress() : 0f;

        // TÍNH TOÁN ĐỘ HÒA QUYỆN (0 = trên cạn hoàn toàn, 1 = dưới nước hoàn toàn)
        float weight = Mathf.Clamp01((waterLevelY - currentY + (transitionRange / 2f)) / transitionRange);

        // 1. Lấy màu Trên cạn (Theo giờ)
        Color aboveCol;
        float aboveInt;
        if (timeProgress < 0.5f) {
            aboveCol = Color.Lerp(dayColor, sunsetColor, timeProgress * 2f);
            aboveInt = aboveIntensity;
        } else {
            aboveCol = Color.Lerp(sunsetColor, nightColor, (timeProgress - 0.5f) * 2f);
            aboveInt = Mathf.Lerp(aboveIntensity, 0.3f, (timeProgress - 0.5f) * 2f);
        }

        // 2. Lấy màu Dưới nước (Theo độ sâu)
        float depth = Mathf.Max(0, waterLevelY - currentY);
        float depthRatio = Mathf.Clamp01(depth / maxDarknessDepth);
        Color underCol = Color.Lerp(waterSurfaceColor, waterDeepColor, depthRatio);
        float underInt = Mathf.Lerp(underIntensity, 0.05f, depthRatio);

        // 3. HÒA QUYỆN TUYỆT ĐỐI (Transition)
        globalLight.color = Color.Lerp(aboveCol, underCol, weight);
        globalLight.intensity = Mathf.Lerp(aboveInt, underInt, weight);
        
        Color bgCol = Color.Lerp(aboveCol * 0.5f, waterDeepColor, weight);
        mainCam.backgroundColor = bgCol;
        mainCam.clearFlags = CameraClearFlags.SolidColor;

        if (underwaterVolume) underwaterVolume.weight = weight;
    }
}
