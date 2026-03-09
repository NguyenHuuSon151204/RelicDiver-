using UnityEngine;

public class UnderwaterParticles : MonoBehaviour
{
    [Header("Cấu hình Hạt")]
    [SerializeField] private ParticleSystem bubblesLead; // Bong bóng từ bình oxy
    [SerializeField] private ParticleSystem seaDust;    // Bụi biển lơ lửng
    [SerializeField] private float surfaceY = 1.5f;     // Mực nước hiện tại

    private void Update()
    {
        bool isUnderwater = transform.position.y < surfaceY;

        // Nếu dưới nước thì bật bụi biển
        if (seaDust != null)
        {
            var emission = seaDust.emission;
            emission.enabled = isUnderwater;
        }

        // Nếu dưới nước và đang di chuyển thì phun bong bóng
        if (bubblesLead != null)
        {
            var emission = bubblesLead.emission;
            DiverController controller = GetComponent<DiverController>();
            
            // Phun bong bóng khi lặn xuống hoặc bơi
            emission.enabled = isUnderwater;
        }
    }
}
