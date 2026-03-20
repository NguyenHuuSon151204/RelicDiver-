using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [System.Serializable]
    public class CharacterUIComponents
    {
        public TextMeshProUGUI nameText;
        public RectTransform nameTransform;
        public Image iconImage;
        public RectTransform iconTransform;
    }

    [Header("UI Cấu hình")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI sharedContentText;
    
    [Header("Nhân vật bên Trái (Thợ lặn)")]
    [SerializeField] private CharacterUIComponents leftCharacter;

    [Header("Nhân vật bên Phải (Tiến sĩ)")]
    [SerializeField] private CharacterUIComponents rightCharacter;

    [Header("Hiệu ứng Visual")]
    [SerializeField] private float activeScale = 1.15f;
    [SerializeField] private float inactiveScale = 0.9f;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float animationSpeed = 5f;

    [Header("Cài đặt")]
    [SerializeField] private float typingSpeed = 0.04f;

    private DialogueData currentDialogue;
    private int lineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    public event Action OnDialogueEnd;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                sharedContentText.text = currentDialogue.lines[lineIndex].content;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }

        UpdateVisuals();
    }

    public void StartDialogue(DialogueData data)
    {
        currentDialogue = data;
        lineIndex = 0;
        dialogueActive = true;
        
        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (HUDController.Instance != null) HUDController.Instance.SetHUDVisible(false); // Ẩn HUD
        DisplayLine();
    }

    private void DisplayLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine(currentDialogue.lines[lineIndex].content));
    }

    private void UpdateVisuals()
    {
        if (!dialogueActive) return;

        bool isRightSpeaking = currentDialogue.lines[lineIndex].isRightSide;

        // Xử lý bên Trái
        ApplyState(leftCharacter, !isRightSpeaking);
        
        // Xử lý bên Phải
        ApplyState(rightCharacter, isRightSpeaking);
    }

    private void ApplyState(CharacterUIComponents ui, bool isActive)
    {
        if (ui == null) return;

        float targetScale = isActive ? activeScale : inactiveScale;
        Color targetColor = isActive ? activeColor : inactiveColor;

        // Phóng to / Thu nhỏ Tên
        if (ui.nameTransform != null)
            ui.nameTransform.localScale = Vector3.Lerp(ui.nameTransform.localScale, Vector3.one * targetScale, Time.deltaTime * animationSpeed);
        
        // Phóng to / Thu nhỏ Icon
        if (ui.iconTransform != null)
            ui.iconTransform.localScale = Vector3.Lerp(ui.iconTransform.localScale, Vector3.one * targetScale, Time.deltaTime * animationSpeed);

        // Sáng / Tối Tên
        if (ui.nameText != null)
            ui.nameText.color = Color.Lerp(ui.nameText.color, targetColor, Time.deltaTime * animationSpeed);

        // Sáng / Tối Icon
        if (ui.iconImage != null)
            ui.iconImage.color = Color.Lerp(ui.iconImage.color, targetColor, Time.deltaTime * animationSpeed);
    }

    private IEnumerator TypeLine(string fullText)
    {
        isTyping = true;
        sharedContentText.text = "";
        
        foreach (char c in fullText.ToCharArray())
        {
            sharedContentText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }

    private void NextLine()
    {
        lineIndex++;
        
        if (lineIndex < currentDialogue.lines.Length)
        {
            DisplayLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (HUDController.Instance != null) HUDController.Instance.SetHUDVisible(true); // Hiện lại HUD
        OnDialogueEnd?.Invoke();
    }

    public bool IsDialogueActive() => dialogueActive;
}
