using UnityEngine;

public class HomeBaseTrigger : MonoBehaviour
{
    [Header("Cấu hình")]
    [SerializeField] private bool showDebugLogs = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có phải Thợ lặn (Player) chạm vào không
        if (collision.CompareTag("Player"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetPlayerAtHomeBase(true);
                if (showDebugLogs) Debug.Log("<color=cyan>HomeBaseTrigger:</color> Thợ lặn đã về tới mặt nước/trạm!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Khi rời khỏi vùng đích
        if (collision.CompareTag("Player"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetPlayerAtHomeBase(false);
                if (showDebugLogs) Debug.Log("<color=orange>HomeBaseTrigger:</color> Thợ lặn đã rời khỏi mặt nước/trạm.");
            }
        }
    }
}
