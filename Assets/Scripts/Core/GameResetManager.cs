using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResetManager : MonoBehaviour
{
    [Header("--- Cấu hình Reset ---")]
    public string mainMenuSceneName = "MainMenu"; // Tên scene chính của bạn

    /// <summary>
    /// Xóa sạch mọi dữ liệu đã lưu và khởi động lại game từ đầu.
    /// </summary>
    public void ResetGameProgress()
    {
        // 1. Xóa sạch PlayerPrefs (Tutorial, Cutscene, Móc lưới...)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("<color=red><b>🔥🔥🔥 ĐÃ XÓA TOÀN BỘ DỮ LIỆU GAME! 🔥🔥🔥</b></color>");

        // 2. Tải lại Scene (Có thể là MainMenu hoặc Scene hiện tại)
        // Nếu bạn muốn về MainMenu, hãy đảm bảo tên Scene trùng với biến trên.
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    // Mẹo: Tôi thêm cả phím tắt Ctrl + Shift + R để bạn reset nhanh trong lúc Test mà không cần nút.
    #if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            ResetGameProgress();
        }
    }
    #endif
}
