using UnityEngine;

/// <summary>
/// Manages the library stealth section.
/// Subscribes to InkManager.OnStealthBegin to activate.
/// Feeds results back to Ink via ResumeFromStealth(choiceIndex).
///
/// Ink choice indices in library_stealth_start knot:
///   0 = success (strawberry found)
///   1 = caught once
///   2 = caught twice
///   3 = bad detour (lives exhausted)
/// </summary>
public class StealthController : MonoBehaviour
{
    [SerializeField] PatrolAI  _patrolAI;
    [SerializeField] Transform _player;
    [SerializeField] float     _detectionRadius = 2.5f;
    [SerializeField] GameObject _caughtFlash;   // optional red flash image

    const int SUCCESS_CHOICE      = 0;
    const int CAUGHT_ONCE_CHOICE  = 1;
    const int CAUGHT_TWICE_CHOICE = 2;
    const int BAD_DETOUR_CHOICE   = 3;

    bool _active;
    bool _playerHiding;
    int  _catchCount;

    void OnEnable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnStealthBegin += Activate;
    }

    void OnDisable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnStealthBegin -= Activate;
    }

    void Activate()
    {
        _active      = true;
        _catchCount  = 0;
        _playerHiding = false;
        _patrolAI.StartPatrol();
    }

    void Deactivate()
    {
        _active = false;
        _patrolAI.StopPatrol();
    }

    /// <summary>Called by HideSpot triggers placed on bookshelf colliders.</summary>
    public void SetPlayerHiding(bool hiding) => _playerHiding = hiding;

    /// <summary>Called by the strawberry pickup trigger in the library.</summary>
    public void OnStrawberryReached()
    {
        if (!_active) return;
        Deactivate();
        InkManager.Instance.ResumeFromStealth(SUCCESS_CHOICE);
    }

    void Update()
    {
        if (!_active || _playerHiding) return;
        if (_patrolAI.CanSeeTarget(_player.position, _detectionRadius))
            HandleCaught();
    }

    void HandleCaught()
    {
        _catchCount++;
        ShowFlash();
        Deactivate();

        int choiceIndex = _catchCount switch {
            1 => CAUGHT_ONCE_CHOICE,
            2 => CAUGHT_TWICE_CHOICE,
            _ => BAD_DETOUR_CHOICE
        };

        InkManager.Instance.ResumeFromStealth(choiceIndex);
    }

    void ShowFlash()
    {
        if (_caughtFlash == null) return;
        _caughtFlash.SetActive(true);
        Invoke(nameof(HideFlash), 0.3f);
    }

    void HideFlash()
    {
        if (_caughtFlash != null) _caughtFlash.SetActive(false);
    }
}

/// <summary>
/// Attach to each bookshelf collider (IsTrigger = true) in the library scene.
/// Tells StealthController when the player is hidden.
/// </summary>
public class HideSpot : MonoBehaviour
{
    StealthController _stealth;
    void Awake() => _stealth = FindFirstObjectByType<StealthController>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _stealth?.SetPlayerHiding(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _stealth?.SetPlayerHiding(false);
    }
}

/// <summary>
/// Place on the IR-section strawberry trigger box in the library.
/// </summary>
public class StrawberryTrigger : MonoBehaviour
{
    StealthController _stealth;
    void Awake() => _stealth = FindFirstObjectByType<StealthController>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _stealth?.OnStrawberryReached();
    }
}
