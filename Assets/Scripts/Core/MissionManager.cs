using UnityEngine;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [System.Serializable]
    public class LevelMission
    {
        public string levelName;
        public string missionDescription; // NỘI DUNG TỰ VIẾT (Ví dụ: "Thu thập vật thần bí")
        public DialogueData briefingDialogue; 
        
        [Header("Số lượng cần nhặt")]
        public int requiredAmount;
        public int currentAmount;
        
        [HideInInspector] public bool hasPlayedBriefing = false;
    }

    [Header("--- Cấu hình 5 Chương ---")]
    public List<LevelMission> levels = new List<LevelMission>();
    public int currentLevelIndex = 0;

    [Header("UI Tham chiếu")]
    public TMPro.TextMeshProUGUI objectiveText; 

    public System.Action OnMissionComplete;
    private bool missionTriggered = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCurrentLevel();
    }

    public void StartCurrentLevel()
    {
        if (currentLevelIndex < levels.Count)
        {
            var mission = levels[currentLevelIndex];
            if (mission.briefingDialogue != null && DialogueManager.Instance != null && !mission.hasPlayedBriefing)
            {
                mission.hasPlayedBriefing = true;
                DialogueManager.Instance.StartDialogue(mission.briefingDialogue);
            }

            UpdateObjectiveUI();
        }
    }

    public void AddProgress(string itemName)
    {
        if (currentLevelIndex < levels.Count)
        {
            var mission = levels[currentLevelIndex];
            // Tự động tăng điểm mà không cần kiểm tra tên vật phẩm cụ thể
            mission.currentAmount++;
            UpdateObjectiveUI();

            // Kiểm tra hoàn thành ngay lập tức
            if (IsMissionComplete() && !missionTriggered)
            {
                missionTriggered = true;
                OnMissionComplete?.Invoke();
                Debug.Log("<color=green>MissionManager:</color> Nhiệm vụ đã hoàn tất!");
            }
        }
    }

    public bool IsMissionComplete()
    {
        if (currentLevelIndex >= levels.Count) return true;
        return levels[currentLevelIndex].currentAmount >= levels[currentLevelIndex].requiredAmount;
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText != null)
        {
            var mission = levels[currentLevelIndex];
            // Hiển thị nội dung mô tả tự viết + tỉ lệ nhặt
            objectiveText.text = $"{mission.missionDescription}: {mission.currentAmount}/{mission.requiredAmount}";
        }
    }
}
