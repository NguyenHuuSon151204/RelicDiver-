using UnityEngine;
using UnityEngine.UI;

public class TutorialImageManager : MonoBehaviour
{
    public static TutorialImageManager Instance { get; private set; }

    [Header("--- Cẩm Nang (Slideshow Màn 1) ---")]
    [Tooltip("Panel chứa toàn bộ sách hướng dẫn")]
    public GameObject slideshowPanel; 
    [Tooltip("Kéo lần lượt các trang (Slide 1, Slide 2...) vào đây")]
    public GameObject[] slides; 
    
    [Header("Nút Điều hướng Cẩm nang")]
    public Button btnNext;
    public Button btnPrev;
    [Tooltip("Nút VÀO VIỆC (chỉ hiện ở trang cuối)")]
    public Button btnClose;

    [Header("--- Hướng Dẫn Tàu Ngầm (Màn 4) ---")]
    [Tooltip("Bảng hướng dẫn phím F lái tàu ngầm")]
    public GameObject submarineTutorialPanel;
    public Button btnCloseSubmarineInfo;

    private int currentSlideIndex = 0;
    private bool isPausedByTutorial = false;

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. Gán sự kiện cho các nút bấm
        if (btnNext) btnNext.onClick.AddListener(NextSlide);
        if (btnPrev) btnPrev.onClick.AddListener(PrevSlide);
        if (btnClose) btnClose.onClick.AddListener(CloseSlideshow);
        if (btnCloseSubmarineInfo) btnCloseSubmarineInfo.onClick.AddListener(CloseSubmarineTutorial);

        // 2. Ẩn toàn bộ UI lúc bắt đầu
        if (slideshowPanel) slideshowPanel.SetActive(false);
        if (submarineTutorialPanel) submarineTutorialPanel.SetActive(false);

        // 3. Tự động kiểm tra sau 1 nhịp (để LevelManager kịp khởi tạo)
        StartCoroutine(AutoCheckTutorial());
    }

    private System.Collections.IEnumerator AutoCheckTutorial()
    {
        // Chờ 1 frame để đảm bảo LevelManager.Instance đã được gán
        yield return null;

        if (LevelManager.Instance != null && LevelManager.Instance.isTutorial && slides.Length > 0)
        {
            // Kiểm tra xem đã xem hướng dẫn này chưa
            string tutorialKey = "TutorialSeen_" + LevelManager.Instance.levelName;
            bool hasSeen = PlayerPrefs.GetInt(tutorialKey, 0) == 1;

            if (!hasSeen)
            {
                Debug.Log($"<color=cyan>Hệ thống nhận diện Màn Tutorial ({LevelManager.Instance.levelName}). Đang mở Cẩm nang...</color>");
                ShowSlideshow();
                // Đánh dấu là đã xem để lần sau không hiện lại
                PlayerPrefs.SetInt(tutorialKey, 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log($"<color=yellow>Người chơi đã xem hướng dẫn màn {LevelManager.Instance.levelName} trước đó. Bỏ qua.</color>");
            }
        }
        else
        {
            Debug.Log("<color=yellow>Bỏ qua tự động mở Cẩm nang (Không phải Màn Tutorial hoặc thiếu Slide).</color>");
        }
    }

    // Hàm gọi để Reset hướng dẫn nếu bạn muốn test lại (có thể gọi từ console hoặc gắn vào nút trong Menu)
    public void ResetTutorials()
    {
        if (LevelManager.Instance != null)
        {
            string tutorialKey = "TutorialSeen_" + LevelManager.Instance.levelName;
            PlayerPrefs.DeleteKey(tutorialKey);
            Debug.Log("<color=orange>Đã Reset hướng dẫn cho màn này!</color>");
        }
    }

    // ==========================================
    // LOGIC SLIDESHOW (Lật trang Màn 1, 2)
    // ==========================================
    public void ShowSlideshow()
    {
        if (slideshowPanel == null || slides.Length == 0) return;

        // Tạm dừng thời gian game (Stop mọi thứ) để người chơi yên tâm đọc
        isPausedByTutorial = true;
        Time.timeScale = 0f; 
        
        slideshowPanel.SetActive(true);
        currentSlideIndex = 0;
        RefreshSlides();
    }

    private void NextSlide()
    {
        if (currentSlideIndex < slides.Length - 1)
        {
            currentSlideIndex++;
            RefreshSlides();
        }
    }

    private void PrevSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            RefreshSlides();
        }
    }

    private void RefreshSlides()
    {
        // Bật/Tắt các trang (chỉ trang ở chỉ số hiện tại mới được bật)
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null) slides[i].SetActive(i == currentSlideIndex);
        }

        // Xử lý Nút Trở Lại (Không hiện ở trang đầu tiên)
        if (btnPrev) btnPrev.gameObject.SetActive(currentSlideIndex > 0);
        
        // Kiểm tra xem có phải Trang Cuối không?
        bool isLastPage = (currentSlideIndex == slides.Length - 1);
        
        // Nút Tiếp (Không hiện ở trang cuối)
        if (btnNext) btnNext.gameObject.SetActive(!isLastPage);
        
        // Nút Đóng (BẮT ĐẦU CHƠI) CHỈ hiện ở Trang Cuối để ép người chơi đọc hết
        if (btnClose) btnClose.gameObject.SetActive(isLastPage); 
    }

    public void CloseSlideshow()
    {
        if (slideshowPanel) slideshowPanel.SetActive(false);
        if (isPausedByTutorial)
        {
            // Trả lại tốc độ chạy game bình thường
            Time.timeScale = 1f; 
            isPausedByTutorial = false;
        }
        Debug.Log("<color=green>Đã đóng Cẩm nang! Bắt đầu nhiệm vụ.</color>");
    }

    // ==========================================
    // LOGIC TÀU NGẦM (Màn 4)
    // ==========================================
    public void ShowSubmarineTutorial()
    {
        if (submarineTutorialPanel)
        {
            isPausedByTutorial = true;
            Time.timeScale = 0f;
            submarineTutorialPanel.SetActive(true);
        }
    }

    public void CloseSubmarineTutorial()
    {
        if (submarineTutorialPanel) submarineTutorialPanel.SetActive(false);
        if (isPausedByTutorial)
        {
            Time.timeScale = 1f;
            isPausedByTutorial = false;
        }
    }
}
