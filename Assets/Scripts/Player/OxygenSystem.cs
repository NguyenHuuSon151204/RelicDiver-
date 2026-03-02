using UnityEngine;
using System;

public class OxygenSystem : MonoBehaviour
{
    [Header("Cấu hình Oxy")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float depletionRate = 1f; 
    [SerializeField] private float sprintDepletionMultiplier = 2f;
    [SerializeField] private float warningThreshold = 20f; // Ngưỡng cảnh báo (20%)

    private float currentOxygen;
    private DiverController diverController;
    private HealthSystem healthSystem;

    public event Action<float, float> OnOxygenChanged; 
    public event Action<bool> OnOxygenWarning; // True khi dưới 20%
    public event Action OnOxygenEmpty;

    private bool isWarning = false;

    private void Awake()
    {
        diverController = GetComponent<DiverController>();
        healthSystem = GetComponent<HealthSystem>();
        currentOxygen = maxOxygen;
    }

    private void Start()
    {
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    private void Update()
    {
        DepleteOxygen();
        HandleOxygenLogic();
    }

    private void DepleteOxygen()
    {
        if (currentOxygen <= 0) return;

        float actualRate = depletionRate;
        if (diverController != null && diverController.IsSprinting())
        {
            actualRate *= sprintDepletionMultiplier;
        }

        currentOxygen -= actualRate * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);

        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    private void HandleOxygenLogic()
    {
        // Kiểm tra ngưỡng cảnh báo 20%
        float percentage = (currentOxygen / maxOxygen) * 100f;
        
        if (percentage <= warningThreshold && !isWarning && currentOxygen > 0)
        {
            isWarning = true;
            OnOxygenWarning?.Invoke(true);
        }
        else if ((percentage > warningThreshold || currentOxygen <= 0) && isWarning)
        {
            isWarning = false;
            OnOxygenWarning?.Invoke(false);
        }

        // Nếu hết Oxy -> Trừ máu (Đuối nước)
        if (currentOxygen <= 0)
        {
            if (healthSystem != null)
            {
                // Trừ 5 máu mỗi giây khi hết oxy
                healthSystem.TakeDamage(5f * Time.deltaTime);
            }
            OnOxygenEmpty?.Invoke();
        }
    }

    public void RestoreOxygen(float amount)
    {
        currentOxygen = Mathf.Clamp(currentOxygen + amount, 0, maxOxygen);
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    public float GetOxygenPercentage() => currentOxygen / maxOxygen;
}
