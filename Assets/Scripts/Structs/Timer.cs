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

    private bool CheckTimer()
    {
        if (_isTicking)
        {
            if (_internalTimerValue <= 0f)
            {
                _isTicking = false;
            }

            _lastTickTimeDifference = Time.time - _lastTickTime;
            _lastTickTime = Time.time;
            _internalTimerValue -= _lastTickTimeDifference;
        }

        return _isTicking;
    }

    public bool IsActive()
    {
        return CheckTimer();
    }
}

