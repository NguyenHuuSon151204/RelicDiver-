using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIGlowEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Cấu hình Phát sáng")]
    [SerializeField] private Image buttonImage;          // Hình ảnh nền nút
    [SerializeField] private TextMeshProUGUI buttonText; // Chữ trên nút
    [SerializeField] private Color normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color glowColor = new Color(0f, 1f, 1f, 1f); // Màu xanh Cyan rực rỡ
    [SerializeField] private float transitionSpeed = 10f;

    private bool isSelected = false;
    private Color targetColor;

    private void Start()
    {
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        targetColor = normalColor;
        UpdateVisuals(true); // Ép màu ban đầu
    }

    private void Update()
    {
        // Hiệu ứng chuyển màu mượt mà
        if (buttonImage != null)
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.unscaledDeltaTime * transitionSpeed);
        
        if (buttonText != null)
            buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.unscaledDeltaTime * transitionSpeed);
    }

    // Khi Chuột rà vào
    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.selectedObject = gameObject; // Ép EventSystem chọn nút này luôn
        SetGlow(true);
    }

    // Khi Chuột rời đi
    public void OnPointerExit(PointerEventData eventData)
    {
        SetGlow(false);
    }

    // Khi Phím W/S chọn vào
    public void OnSelect(BaseEventData eventData)
    {
        SetGlow(true);
    }

    // Khi Phím W/S rời sang nút khác
    public void OnDeselect(BaseEventData eventData)
    {
        SetGlow(false);
    }

    private void SetGlow(bool active)
    {
        targetColor = active ? glowColor : normalColor;
        
        // Hiệu ứng "Giật nhẹ" khi chọn (Premium feel)
        if (active) transform.localScale = Vector3.one * 1.05f;
        else transform.localScale = Vector3.one;
    }

    private void UpdateVisuals(bool instant)
    {
        if (buttonImage) buttonImage.color = targetColor;
        if (buttonText) buttonText.color = targetColor;
    }
}
