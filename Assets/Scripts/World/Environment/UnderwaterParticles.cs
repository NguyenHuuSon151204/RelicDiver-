using UnityEngine;

public class UnderwaterParticles : MonoBehaviour
{
    [Header("Cấu hình Hạt")]
    [SerializeField] private ParticleSystem bubblesLead; // Bong bóng từ bình oxy
    [SerializeField] private ParticleSystem seaDust;    // Bụi biển lơ lửng
    [SerializeField] private int sortingOrder = 10;      // Đảm bảo hiện trên cùng

    private void Start()
    {
        // 1. Tự động thiết lập hiển thị cho cả 2 loại hạt
        SetupRenderer(bubblesLead);
        SetupRenderer(seaDust);

        // 2. Kích hoạt toàn bộ ngay lập tức và giữ chúng luôn chạy
        ActivateAllParticles();
    }

    private void SetupRenderer(ParticleSystem ps)
    {
        if (ps == null) return;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null) renderer.sortingOrder = sortingOrder;

        var main = ps.main;
        // Chốt chặn cuối cùng: Không bao giờ được phép tự tắt (Always Simulate)
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        main.playOnAwake = true;

        // Đưa trục Z của Particle System hơi lồi về phía Camera
        Vector3 pos = ps.transform.localPosition;
        pos.z = -0.5f; 
        ps.transform.localPosition = pos;
    }

    private void ActivateAllParticles()
    {
        if (seaDust != null)
        {
            var emission = seaDust.emission;
            emission.enabled = true;
            seaDust.Play();
        }

        if (bubblesLead != null)
        {
            var emission = bubblesLead.emission;
            emission.enabled = true;
            bubblesLead.Play();
        }

        Debug.Log("<color=green>HUD: Đã kích hoạt vĩnh viễn Bụi biển và Bong bóng!</color>");
    }

    // Đã gỡ bỏ Update() và biến SurfaceY để tránh lỗi bong bóng lúc có lúc không
}
