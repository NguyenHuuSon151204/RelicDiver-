using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RepairMiniGame : MonoBehaviour
{
    [Header("Cấu hình Mini-game")]
    [SerializeField] private GameObject repairPanel;
    [SerializeField] private TextMeshProUGUI sequenceText;
    [SerializeField] private int sequenceLength = 4;
    [SerializeField] private float repairAmount = 25f;

    [Header("Tham chiếu")]
    [SerializeField] private DurabilitySystem durabilitySystem;

    private List<KeyCode> currentSequence = new List<KeyCode>();
    private int currentIndex = 0;
    private bool isActive = false;

    private readonly KeyCode[] possibleKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    private void Start()
    {
        if (repairPanel) repairPanel.SetActive(false);
    }

    private void Update()
    {
        // Nhấn R để bắt đầu sửa nếu bị hỏng hoặc muốn bảo trì
        if (Input.GetKeyDown(KeyCode.R) && !isActive)
        {
            StartRepairGame();
        }

        if (isActive)
        {
            CheckInput();
        }
    }

    public void StartRepairGame()
    {
        isActive = true;
        currentIndex = 0;
        GenerateSequence();
        UpdateUI();
        if (repairPanel) repairPanel.SetActive(true);
        
        // Tạm dừng di chuyển thợ lặn khi đang sửa
        DiverController controller = GetComponent<DiverController>();
        if (controller != null) controller.enabled = false;
    }

    private void GenerateSequence()
    {
        currentSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            currentSequence.Add(possibleKeys[Random.Range(0, possibleKeys.Length)]);
        }
    }

    private void CheckInput()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(currentSequence[currentIndex]))
            {
                currentIndex++;
                Debug.Log("Chính xác!");
                UpdateUI();

                if (currentIndex >= currentSequence.Count)
                {
                    FinishRepair();
                }
            }
            else if (!Input.GetKeyDown(KeyCode.R)) // Sai phím
            {
                Debug.Log("Sai rồi! Làm lại từ đầu.");
                currentIndex = 0;
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        if (!sequenceText) return;

        string display = "NHẤN PHÍM: ";
        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (i < currentIndex)
                display += $"<color=green>{currentSequence[i]}</color> ";
            else if (i == currentIndex)
                display += $"<color=yellow><u>{currentSequence[i]}</u></color> ";
            else
                display += $"{currentSequence[i]} ";
        }
        sequenceText.text = display;
    }

    private void FinishRepair()
    {
        isActive = false;
        if (repairPanel) repairPanel.SetActive(false);
        
        if (durabilitySystem != null)
        {
            durabilitySystem.Repair(repairAmount);
        }

        // Cho phép di chuyển lại
        DiverController controller = GetComponent<DiverController>();
        if (controller != null) controller.enabled = true;

        Debug.Log("Đã sửa xong!");
    }
}
