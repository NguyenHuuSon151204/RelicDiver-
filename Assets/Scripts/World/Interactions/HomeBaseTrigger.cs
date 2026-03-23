using UnityEngine;

public class HomeBaseTrigger : MonoBehaviour
{
    [Header("Cấu hình")]
    [SerializeField] private GameObject activeVisuals; // Đèn hoặc hiệu ứng khi trạm sẵn sàng
    [SerializeField] private AudioClip activationSound; // Âm thanh khi hoàn thành nhiệm vụ
    [SerializeField] private bool showDebugLogs = true;

    private bool isActivated = false;

    private void Start()
    {
        // Ban đầu tắt đèn trạm
        if (activeVisuals) activeVisuals.SetActive(false);
        
        // Đăng ký sự kiện hoàn thành nhiệm vụ
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionComplete += ActivateHomeBase;
            
            // Nếu chẳng may nhiệm vụ đã xong trước khi Start (đề phòng)
            if (MissionManager.Instance.IsMissionComplete()) ActivateHomeBase();
        }
    }

    private void OnDestroy()
    {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionComplete -= ActivateHomeBase;
    }

    private void ActivateHomeBase()
    {
        if (isActivated) return;
        isActivated = true;

        if (activeVisuals) activeVisuals.SetActive(true);
        if (activationSound && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(activationSound);

        Debug.Log("<color=cyan>HomeBase:</color> Trạm đã được kích hoạt! Bạn có thể cập bến.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActivated) return; // Nếu chưa xong nhiệm vụ thì trigger này coi như chưa bật

        // Kiểm tra xem có phải Thợ lặn (Player) hoặc Tàu ngầm (Submarine) chạm vào không
        if (collision.CompareTag("Player") || collision.CompareTag("Submarine"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetPlayerAtHomeBase(true);
                if (showDebugLogs) Debug.Log($"<color=cyan>HomeBaseTrigger:</color> {collision.tag} đã về tới mặt nước/trạm!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Khi rời khỏi vùng đích (Player hoặc Submarine)
        if (collision.CompareTag("Player") || collision.CompareTag("Submarine"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetPlayerAtHomeBase(false);
                if (showDebugLogs) Debug.Log($"<color=orange>HomeBaseTrigger:</color> {collision.tag} đã rời khỏi mặt nước/trạm.");
            }
        }
    }
}
