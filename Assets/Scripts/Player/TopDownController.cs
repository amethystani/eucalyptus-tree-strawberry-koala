using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownController : MonoBehaviour
{
    [SerializeField] float _speed = 3f;

    Rigidbody2D _rb;
    Vector2     _moveInput;
    bool        _dialogueActive;

    void Awake()
    {
        _rb              = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    void OnEnable()  => DialogueUI.OnDialogueActiveChanged += SetDialogueActive;
    void OnDisable() => DialogueUI.OnDialogueActiveChanged -= SetDialogueActive;

    void SetDialogueActive(bool active)
    {
        _dialogueActive = active;
        if (active) _rb.linearVelocity = Vector2.zero;
    }

    // Called by Unity Input System action (Player/Move)
    void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

    // Called by VirtualJoystick on mobile
    public void SetMobileInput(Vector2 input) => _moveInput = input;

    void FixedUpdate()
    {
        if (_dialogueActive)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }
        _rb.linearVelocity = _moveInput.normalized * _speed;
    }
}
