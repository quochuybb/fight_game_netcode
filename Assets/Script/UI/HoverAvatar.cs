using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HoverAvatar : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Settings")]
    [SerializeField] private float hoverScale = 1.2f;     
    [SerializeField] private float hoverRotateZ = 30f;     
    [SerializeField] private float clickScale = 1.4f;     
    [SerializeField] private float clickRotateZ = 45f;     
    [SerializeField] private float duration = 0.3f;        
    private bool _isAnimating = false;                    

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isAnimating) return;

        transform.DOScale(hoverScale, duration).SetEase(Ease.OutBack);                    
        transform.DORotate(new Vector3(0, 0, hoverRotateZ), duration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isAnimating) return;

        transform.DOScale(1f, duration).SetEase(Ease.OutBack);
        transform.DORotate(Vector3.zero, duration).SetEase(Ease.OutBack);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(clickScale, duration).SetEase(Ease.OutBack))
           .Join(transform.DORotate(new Vector3(0, 0, clickRotateZ), duration).SetEase(Ease.OutBack))
           .Append(transform.DOScale(1f, duration).SetEase(Ease.InBack))
           .Join(transform.DORotate(Vector3.zero, duration).SetEase(Ease.InBack))
           .OnComplete(() => _isAnimating = false);
    }
}
