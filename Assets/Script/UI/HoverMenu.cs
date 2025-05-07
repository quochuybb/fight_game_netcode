using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI buttonText; // Kéo thả Text vào đây

    [Header("Settings")]
    [SerializeField] private Color hoverColor = Color.yellow; // Màu khi hover
    [SerializeField] private float fadeDuration = 0.3f; // Thời gian hiệu ứng
    [SerializeField] private float blinkSpeed = 0.2f; // Tốc độ nhấp nháy khi click

    private Color _originalColor;
    private bool _isBlinking;
    [SerializeField] private float hoverScale = 1.1f;
    
    private void Start()
    {
        _originalColor = buttonText.color; // Lưu màu gốc
        GetComponent<Button>().onClick.AddListener(OnClick); // Đăng ký sự kiện click
    }

    // Highlight khi hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.DOColor(hoverColor, fadeDuration);
        buttonText.DOColor(hoverColor, fadeDuration);
        transform.DOScale(hoverScale, fadeDuration); // Phóng to
    }

    // Trở về màu gốc khi rời chuột
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.DOColor(_originalColor, fadeDuration);
        buttonText.DOColor(_originalColor, fadeDuration);
        transform.DOScale(1, fadeDuration); // Trở về kích thước gốc
    }

    // Nhấp nháy khi click
    private void OnClick()
    {
        if (_isBlinking) return;
        _isBlinking = true;

        Sequence blinkSequence = DOTween.Sequence();
        blinkSequence
            .Append(buttonText.DOFade(0, blinkSpeed)) // Ẩn text
            .Append(buttonText.DOFade(1, blinkSpeed)) // Hiện text
            .SetLoops(3) // Lặp 3 lần (6 bước)
            .OnComplete(() => _isBlinking = false); // Reset trạng thái
    }
}