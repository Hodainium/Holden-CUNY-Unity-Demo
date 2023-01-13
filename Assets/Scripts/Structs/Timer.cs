using UnityEngine;

public struct Timer
{
    private bool _isTicking;
    private float _maxTimerValue;
    private float _internalTimerValue;
    private float _lastTickTime;
    private float _lastTickTimeDifference;

    public Timer(float maxTimerValue)
    {
        _isTicking = false;
        _maxTimerValue = maxTimerValue;
        _internalTimerValue = 0f;
        _lastTickTime = 0f;
        _lastTickTimeDifference = 0f;        
    }

    public void ResetTimer()
    {
        _isTicking = true;
        _internalTimerValue = _maxTimerValue;
        _lastTickTime = Time.time;
    }

    public float GetTime()
    {
        CheckTimer();
        if (!_isTicking)
        {
            _internalTimerValue = 0f;
        }
        return _internalTimerValue;
    }

    private bool CheckTimer()
    {
        if (_isTicking)
        {
            _lastTickTimeDifference = Time.time - _lastTickTime;
            _lastTickTime = Time.time;
            _internalTimerValue -= _lastTickTimeDifference;
            Debug.Log("Internal timer value: " + _internalTimerValue);
            if (_internalTimerValue <= 0f)
            {
                Debug.Log("Final value: " + _internalTimerValue);
                _isTicking = false;
            }
        }

        return _isTicking;
    }

    public bool IsActive()
    {
        return CheckTimer();
    }
}

