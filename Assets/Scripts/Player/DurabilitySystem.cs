using UnityEngine;
using System;

public class DurabilitySystem : MonoBehaviour
{
    [Header("Cấu hình Độ bền")]
    [SerializeField] private float maxDurability = 100f;
    [SerializeField] private float collisionDamage = 10f;
    [SerializeField] private float minVelocityToDamage = 2f;

    private float currentDurability;
    public event Action<float, float> OnDurabilityChanged;
    public event Action OnBroken;

    private void Start()
    {
        currentDurability = maxDurability;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Chỉ nhận sát thương nếu va chạm đủ mạnh
        if (collision.relativeVelocity.magnitude > minVelocityToDamage)
        {
            TakeDamage(collisionDamage);
            Debug.Log("<color=orange>VA CHẠM!</color> Độ bền còn lại: " + currentDurability);
        }
    }

    public void TakeDamage(float amount)
    {
        currentDurability -= amount;
        currentDurability = Mathf.Max(currentDurability, 0);
        
        OnDurabilityChanged?.Invoke(currentDurability, maxDurability);

        if (currentDurability <= 0)
        {
            OnBroken?.Invoke();
        }
    }

    public void Repair(float amount)
    {
        currentDurability += amount;
        currentDurability = Mathf.Min(currentDurability, maxDurability);
        OnDurabilityChanged?.Invoke(currentDurability, maxDurability);
    }

    public float GetDurabilityPercentage() => currentDurability / maxDurability;
}
