using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CanvasGroup faderGroup;

    [Header("Settings Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("First Selection (For W/S Keys)")]
    [SerializeField] private Selectable firstButtonMenu;
    [SerializeField] private Selectable firstButtonSettings;

    [Header("Sliding & Scaling Animation")]
    [SerializeField] private RectTransform mainMenuRect;
    [SerializeField] private Vector2 menuCenterPos = Vector2.zero;
    [SerializeField] private Vector2 menuSidePos = new Vector2(-500f, 0f);
    [SerializeField] private float menuSideScale = 0.8f; // Thu nhỏ còn 80%
    [SerializeField] private float slideSpeed = 5f;
    private Vector2 targetMenuPos;
    private Vector3 targetScale;

    [Header("Logo Animation")]
    [SerializeField] private Transform logoTransform;
    [SerializeField] private float pulseAmount = 0.05f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectionPanel;

    private void Awake()
    {
        targetMenuPos = menuCenterPos; 
        targetScale = Vector3.one;
    }

    private void Start()
    {
        
        // Khởi động các Sliders theo giá trị đã lưu
        if (musicSlider && AudioManager.Instance != null)
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
        
        if (sfxSlider && AudioManager.Instance != null)
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();

        // Đảm bảo fader và level selection ẩn lúc đầu
        if (faderGroup) faderGroup.alpha = 0f;
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
        
        if (menuPanel) menuPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // 1. Hiệu ứng trượt vị trí và thu nhỏ cho toàn bộ Menu Layout
        if (mainMenuRect)
        {
            mainMenuRect.anchoredPosition = Vector2.Lerp(mainMenuRect.anchoredPosition, targetMenuPos, Time.deltaTime * slideSpeed);
            mainMenuRect.localScale = Vector3.Lerp(mainMenuRect.localScale, targetScale, Time.deltaTime * slideSpeed);
        }

        // 2. Hiệu ứng "Rập rình" cho logo (Hủy bỏ nhân targetScale vì đã là con của mainMenuRect)
        if (logoTransform)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            logoTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    // Hàm để gọi khi mở bảng chọn màn
    public void SetMenuSide(bool isSide)
    {
        targetMenuPos = isSide ? menuSidePos : menuCenterPos;
        targetScale = isSide ? new Vector3(menuSideScale, menuSideScale, 1f) : Vector3.one;
    }

    public void OpenLevelSelection()
    {
        if (levelSelectionPanel) levelSelectionPanel.SetActive(true);
    }

    public void CloseLevelSelection()
    {
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
    }

    // Giữ lại hàm này để gọi từ nút Play bên trong Level Card
    public void StartGame(int sceneIndex)
    {
        Debug.Log($"<color=green>BẮT ĐẦU NHIỆM VỤ: {sceneIndex}</color>");
        StartCoroutine(FadeAndStart(sceneIndex));
    }

    private IEnumerator FadeAndStart(int sceneIndex)
    {
        if (faderGroup)
        {
            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                faderGroup.alpha = timer;
                yield return null;
            }
        }
        SceneManager.LoadScene(1); // Vào Màn 1
    }

    public void OpenSettings()
    {
        if (menuPanel) menuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
        if (firstButtonSettings != null) firstButtonSettings.Select();
    }

    public void CloseSettings()
    {
        if (menuPanel) menuPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (firstButtonMenu != null) firstButtonMenu.Select();
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    public void ExitGame()
    {
        Debug.Log("<color=red>THOÁT GAME...</color>");
        Application.Quit();
    }
}
