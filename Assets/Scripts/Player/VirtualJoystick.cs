using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen joystick for mobile. Disabled automatically on non-mobile builds.
/// Place in a Canvas (Screen Space - Overlay) at the bottom-left corner.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] RectTransform _background;
    [SerializeField] RectTransform _handle;
    [SerializeField] float         _range = 60f;   // max handle travel in px

    TopDownController _controller;
    Canvas            _canvas;
    Vector2           _startPos;

    void Awake()
    {
        _canvas     = GetComponentInParent<Canvas>();
        _controller = FindFirstObjectByType<TopDownController>();

#if !UNITY_ANDROID && !UNITY_IOS
        gameObject.SetActive(false);
#endif
    }

    public void OnPointerDown(PointerEventData e)
    {
        _startPos               = e.position;
        _background.position    = e.position;
        _handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData e)
    {
        Vector2 delta   = e.position - _startPos;
        Vector2 clamped = Vector2.ClampMagnitude(delta, _range);
        _handle.anchoredPosition = clamped / _canvas.scaleFactor;
        _controller?.SetMobileInput(clamped / _range);
    }

    public void OnPointerUp(PointerEventData e)
    {
        _handle.anchoredPosition = Vector2.zero;
        _controller?.SetMobileInput(Vector2.zero);
    }
}
