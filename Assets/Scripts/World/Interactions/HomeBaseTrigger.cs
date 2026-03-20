using UnityEngine;

public class HomeBaseTrigger : MonoBehaviour
{
    [Header("Cấu hình")]
    [SerializeField] private bool showDebugLogs = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
