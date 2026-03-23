using UnityEngine;

public class LevelBGMManager : MonoBehaviour
{
    [Header("Cấu hình Nhạc nền Màn chơi")]
    [SerializeField] private AudioClip levelBGM;

    private void Start()
    {
        Debug.Log($"LevelBGM: Đang kiểm tra... Nhạc đã gán: {levelBGM != null}, AudioManager: {AudioManager.Instance != null}");

        // Tự động phát nhạc khi màn chơi bắt đầu thông qua AudioManager
        if (levelBGM != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(levelBGM);
            Debug.Log("<color=green>LevelBGM: Đã gọi lệnh phát nhạc thành công!</color>");
        }
        else if (levelBGM == null)
        {
            Debug.LogWarning("LevelBGM: Bạn chưa gán file nhạc vào ô Level BGM!");
        }
    }
}
