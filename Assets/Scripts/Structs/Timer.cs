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
        if (maxTimerValue <= 0f)
        {
            _maxTimerValue = 0.1f;
        }
        else
        {
            _maxTimerValue = maxTimerValue;
        }
        _isTicking = false;
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

    private float GetTimerPercentToCompletion()
    {
        CheckTimer();
        if (_isTicking)
        {
            return 1f - (_internalTimerValue / _maxTimerValue);
        }
        else
        {
            return 1f;
        }

    }

    public bool IsActive()
    {
        return CheckTimer();
    }
}

