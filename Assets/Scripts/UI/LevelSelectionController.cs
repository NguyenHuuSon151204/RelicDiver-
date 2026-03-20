using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectionController : MonoBehaviour
{
    [System.Serializable]
    public struct LevelCard
    {
        [Header("--- Dữ liệu chữ (Nhập vào đây) ---")]
        public string chapterTitle;
        public string levelName;
        [TextArea(2, 4)] public string levelDetails;

        [Header("--- Tham chiếu UI (Kéo thả) ---")]
        public Button playButton;
        public GameObject lockIcon;
        public Image previewDisplay; // Ô Image trên UI của Card
        public Sprite levelScreenshot; // Ảnh bạn chụp màn chơi kéo vào đây
        public TextMeshProUGUI chapterText;
        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI detailsText;
    }

    [Header("Panel Settings")]
    [SerializeField] private GameObject mainPanel; // Cả cái bảng chọn màn to đùng
    [SerializeField] private MainMenuController mainMenuController;

    [Header("Danh sách Thẻ Màn chơi")]
    [SerializeField] private LevelCard[] levelCards;
    [SerializeField] private int startBuildIndex = 1;

    private void OnEnable()
    {
        if (mainMenuController) mainMenuController.SetMenuSide(true);
    }

    private void OnDisable()
    {
        if (mainMenuController) mainMenuController.SetMenuSide(false);
    }

    private void Start()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

        for (int i = 0; i < levelCards.Length; i++)
        {
            // 1. Tự động gán dữ liệu chữ đã nhập trong Inspector lên UI
            if (levelCards[i].chapterText) levelCards[i].chapterText.text = levelCards[i].chapterTitle;
            if (levelCards[i].levelNameText) levelCards[i].levelNameText.text = levelCards[i].levelName;
            if (levelCards[i].detailsText) levelCards[i].detailsText.text = levelCards[i].levelDetails;

            int levelNum = i + 1;

            // 2. Xử lý trạng thái Khóa/Mở
            if (levelNum > reachedLevel)
            {
                // MÀN BỊ KHÓA: Hiện ổ khóa, ẩn nút Play
                if (levelCards[i].lockIcon) levelCards[i].lockIcon.SetActive(true);
                if (levelCards[i].playButton) levelCards[i].playButton.gameObject.SetActive(false);
            }
            else
            {
                // MÀN ĐÃ MỞ: Hiện nút Play, ẩn ổ khóa
                if (levelCards[i].lockIcon) levelCards[i].lockIcon.SetActive(false);
                if (levelCards[i].playButton) 
                {
                    levelCards[i].playButton.gameObject.SetActive(true);
                    
                    // Gán lệnh vào nút Play
                    int sceneIndex = startBuildIndex + i;
                    levelCards[i].playButton.onClick.RemoveAllListeners();
                    levelCards[i].playButton.onClick.AddListener(() => LoadLevel(sceneIndex));
                }
            }
        }
    }

    public void LoadLevel(int index)
    {
        if (mainMenuController != null)
        {
            mainMenuController.StartGame(index);
        }
        else
        {
            // Nếu quên gán controller, vẫn vào được màn nhưng không có fade
            SceneManager.LoadScene(index);
        }
    }

    public static void UnlockNextLevel(int currentLevelIndex)
    {
        // Logic mở khóa dựa trên Index màn chơi
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);
        int currentLevelNum = currentLevelIndex; // Giả sử màn 1 có index 1

        if (currentLevelNum + 1 > reachedLevel)
        {
            PlayerPrefs.SetInt("ReachedLevel", currentLevelNum + 1);
            PlayerPrefs.Save();
        }
    }

    public void BackToMenu() => SceneManager.LoadScene(0);
}
