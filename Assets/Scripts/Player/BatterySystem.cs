using UnityEngine;
using System;

public class BatterySystem : MonoBehaviour
{
    [Header("Cấu hình Pin")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float currentBattery;
    [SerializeField] private float baseDrainRate = 0.5f; // Tiêu hao cơ bản theo thời gian

    public event Action<float, float> OnBatteryChanged; // (current, max)
    public event Action OnBatteryEmpty;

    private float speedDrainMultiplier = 0f;
    private float lightDrainMultiplier = 0f;

    private void Start()
    {
        currentBattery = maxBattery;
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);
    }

    private void Update()
    {
        if (currentBattery <= 0) return;

        // Tính toán lượng tiêu hao tổng hợp
        float totalDrain = baseDrainRate + speedDrainMultiplier + lightDrainMultiplier;
        
        currentBattery -= totalDrain * Time.deltaTime;
        currentBattery = Mathf.Max(currentBattery, 0f);

        OnBatteryChanged?.Invoke(currentBattery, maxBattery);

        if (currentBattery <= 0)
        {
            OnBatteryEmpty?.Invoke();
            Debug.Log("Hết Pin! Tàu ngầm đang trôi tự do...");
        }
    }

    // Nhận giá trị từ Cần gạt (0.0 đến 1.0)
    public void SetSpeedDrain(float value)
    {
        // Giả sử speed lever kéo lên cao nhất tốn thêm 5 đơn vị/giây
        speedDrainMultiplier = value * 5f;
    }

    public void SetLightDrain(float value)
    {
        // Giả sử đèn sáng nhất tốn thêm 3 đơn vị/giây
        lightDrainMultiplier = value * 3f;
    }

    public void Recharge(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Min(currentBattery, maxBattery);
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);
    }

    public float GetBatteryPercentage() => currentBattery / maxBattery;
}
