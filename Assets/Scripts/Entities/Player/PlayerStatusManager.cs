using UnityEngine;
using System;

public class PlayerStatusManager : MonoBehaviour
{
    [Header("--- MÁU (HEALTH) ---")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("--- OXY (OXYGEN) ---")]
    public float maxOxygen = 100f;
    public float currentOxygen;
    public float oxygenDepletionRate = 1f;
    public float sprintOxygenMultiplier = 2f;
    public float oxygenWarningThreshold = 20f;

    [Header("--- PIN (BATTERY) ---")]
    public float maxBattery = 100f;
    public float currentBattery;
    public float baseBatteryDrain = 0.5f;

    // Sự kiện cho UI (Dùng chung Action để HUD dễ đăng ký)
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnOxygenChanged;
    public event Action<float, float> OnBatteryChanged;
    public event Action<bool> OnOxygenWarning;
    public event Action OnDeath;
    public event Action OnBatteryEmpty;

    private DiverController diverController;
    private bool isOxygenWarning = false;
    private float speedBatteryDrain = 0f;
    private float lightBatteryDrain = 0f;

    private void Awake()
    {
        diverController = GetComponent<DiverController>();
        currentHealth = maxHealth;
        currentOxygen = maxOxygen;
        currentBattery = maxBattery;
    }

    private void Start()
    {
        // Thông báo giá trị ban đầu cho HUD
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);
    }

    private void Update()
    {
        HandleOxygen();
        HandleBattery();
    }

    // --- LOGIC OXY ---
    private void HandleOxygen()
    {
        if (currentOxygen <= 0)
        {
            // Hết Oxy -> Trừ máu
            TakeDamage(5f * Time.deltaTime);
            return;
        }

        float rate = oxygenDepletionRate;
        if (diverController != null && diverController.IsSprinting())
            rate *= sprintOxygenMultiplier;

        currentOxygen = Mathf.Max(0, currentOxygen - rate * Time.deltaTime);
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);

        // Cảnh báo Oxy
        float pct = (currentOxygen / maxOxygen) * 100f;
        if (pct <= oxygenWarningThreshold && !isOxygenWarning && currentOxygen > 0)
        {
            isOxygenWarning = true;
            OnOxygenWarning?.Invoke(true);
        }
        else if ((pct > oxygenWarningThreshold || currentOxygen <= 0) && isOxygenWarning)
        {
            isOxygenWarning = false;
            OnOxygenWarning?.Invoke(false);
        }
    }

    // --- LOGIC PIN ---
    private void HandleBattery()
    {
        if (currentBattery <= 0) return;

        float totalDrain = baseBatteryDrain + speedBatteryDrain + lightBatteryDrain;
        currentBattery = Mathf.Max(0, currentBattery - totalDrain * Time.deltaTime);
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);

        if (currentBattery <= 0)
        {
            OnBatteryEmpty?.Invoke();
            Debug.Log("Hết Pin! Tàu ngầm đang trôi tự do...");
        }
    }

    public void SetSpeedBatteryDrain(float value) => speedBatteryDrain = value * 5f;
    public void SetLightBatteryDrain(float value) => lightBatteryDrain = value * 3f;

    // --- LOGIC MÁU ---
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0) OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void RestoreOxygen(float amount)
    {
        currentOxygen = Mathf.Min(maxOxygen, currentOxygen + amount);
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Min(maxBattery, currentBattery + amount);
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);
    }

    // Helper cho UI
    public float GetOxygenPercentage() => currentOxygen / maxOxygen;
}
