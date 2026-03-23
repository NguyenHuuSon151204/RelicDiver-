using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("--- Thông tin Màn chơi ---")]
    public string levelName = "Màn chơi";
    [Header("--- Trạng thái ---")]
    private int collectedRelics = 0;

    [Header("--- Quy tắc Màn chơi ---")]
    public bool allowSubmarine = true;
    public bool spawnNightMonster = true;
    public bool isTutorial = false;

    [Header("--- Trạng thái ---")]
    private bool isLevelComplete = false;
    private bool playerAtHomeBase = false;

    // Sự kiện dành cho HUD và các hệ thống khác
    public event System.Action<int, int> OnArtifactCollected; // (collected, total)
    public event System.Action<int, int> OnPhotoTaken;      // (taken, total)
    public event System.Action OnLevelComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddRelic()
    {
        collectedRelics++;
        Debug.Log($"<color=yellow>Nhặt vật phẩm:</color> {collectedRelics}/{GetTargetRelics()}");

        // Kích hoạt sự kiện để HUD cập nhật
        OnArtifactCollected?.Invoke(collectedRelics, GetTargetRelics());

        // (Đã loại bỏ gọi Popup hướng dẫn hoàn thành nhiệm vụ ở đây vì hệ thống Cẩm nang Slideshow đã bao hàm nội dung đó)
        if (isTutorial && collectedRelics >= GetTargetRelics())
        {
            Debug.Log("<color=green>Đã đủ vật phẩm! Hãy bơi về Trạm Sáng Xanh.</color>");
        }
    }

    // Hàm alias dành cho các script cũ
    public void AddArtifact() => AddRelic();

    public void AddPhoto()
    {
        // Tính năng chụp ảnh đã bị gỡ bỏ
    }

    public void SetPlayerAtHomeBase(bool atBase)
    {
        playerAtHomeBase = atBase;
        if (atBase && IsComplete() && !isLevelComplete)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        isLevelComplete = true;
        Debug.Log($"<color=green>HOÀN THÀNH MÀN CHƠI: {levelName}!</color>");
        
        // Kích hoạt sự kiện thắng cuộc
        OnLevelComplete?.Invoke();
        
        // Thực hiện hiện UI Win thông qua HUD (HUDController đã đăng ký sự kiện này)
    }

    public void RestartLevel()
    {
        Debug.Log("<color=red>Restart màn chơi...</color>");
        // Restart scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetCollectedCount() => collectedRelics;
    public int GetTargetRelics() => MissionManager.Instance != null ? MissionManager.Instance.levels[MissionManager.Instance.currentLevelIndex].requiredAmount : 0;
    public int GetTakenPhotosCount() => 0;
    public bool IsComplete() => MissionManager.Instance != null && MissionManager.Instance.IsMissionComplete();
}
