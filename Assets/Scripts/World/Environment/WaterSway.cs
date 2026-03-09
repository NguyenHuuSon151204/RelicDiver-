using UnityEngine;

public class WaterSway : MonoBehaviour
{
    [Header("Cấu hình Đung đưa")]
    [SerializeField] private float swayAmount = 5f; // Góc đung đưa (độ)
    [SerializeField] private float swaySpeed = 1f;  // Tốc độ nhanh hay chậm
    [SerializeField] private float timeOffset;     // Độ trễ để các bụi rong không lắc giống hệt nhau

    private float initialZRotation;

    private void Start()
    {
        initialZRotation = transform.rotation.eulerAngles.z;
        if (timeOffset == 0) timeOffset = Random.Range(0f, 2f);
    }

    private void Update()
    {
        float sway = Mathf.Sin((Time.time + timeOffset) * swaySpeed) * swayAmount;
        transform.rotation = Quaternion.Euler(0, 0, initialZRotation + sway);
    }
}
