using UnityEngine;

public class PhotoTarget : MonoBehaviour
{
    public string targetName = "Vết nứt";
    public bool isPhotographed = false;
    
    [Header("Visuals")]
    public GameObject highlightEffect; // Hiệu ứng khung ngắm khi camera rọi trúng

    private void Start()
    {
        if (highlightEffect) highlightEffect.SetActive(false);
    }

    public void OnPhotographed()
    {
        if (isPhotographed) return;
        
        isPhotographed = true;
        
        // Báo cho Level Manager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddPhoto();
        }

        // Tắt highlight sau khi chụp xong
        if (highlightEffect) highlightEffect.SetActive(false);
        
        Debug.Log($"<color=green>Đã hoàn thành mục tiêu chụp ảnh:</color> {targetName}");
    }
}
