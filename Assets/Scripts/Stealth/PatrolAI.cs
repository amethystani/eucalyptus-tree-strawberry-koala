using UnityEngine;

/// <summary>
/// Moves Prof. Jabin along a list of waypoints, pausing briefly at each one.
/// Call StartPatrol() to activate, StopPatrol() to freeze.
/// </summary>
public class PatrolAI : MonoBehaviour
{
    [SerializeField] Transform[] _waypoints;
    [SerializeField] float       _speed    = 1.5f;
    [SerializeField] float       _waitTime = 1.0f;

    int   _current;
    float _waitTimer;
    bool  _waiting;
    bool  _active;

    public void StartPatrol() => _active = true;
    public void StopPatrol()  => _active = false;

    void Update()
    {
        if (!_active || _waypoints.Length == 0) return;

        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f) _waiting = false;
            return;
        }

        Transform target = _waypoints[_current];
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            _speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            _current   = (_current + 1) % _waypoints.Length;
            _waiting   = true;
            _waitTimer = _waitTime;
        }
    }

    /// <summary>Returns true if targetPos is within the vision radius.</summary>
    public bool CanSeeTarget(Vector2 targetPos, float visionRadius = 2.5f) =>
        Vector2.Distance(transform.position, targetPos) < visionRadius;
}
