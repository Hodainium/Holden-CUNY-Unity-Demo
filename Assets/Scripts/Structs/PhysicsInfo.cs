using UnityEngine;
using System;

[Serializable]
public struct PhysicsInfo
{
    [SerializeField] Vector2 launchAngle;
    [SerializeField] bool IsFlatForce;
    [SerializeField] float forceValue; //functions as either multiplier or flat value

    public PhysicsInfo(Vector2 tempLaunchAngle = default(Vector2), float tempForceValue = 0f, bool isFlatForce = false)
    {
        // default vector2 is 0,0
        launchAngle = tempLaunchAngle;
        forceValue = tempForceValue;
        IsFlatForce = isFlatForce;
    }

    public bool IsFlat()
    {
        return IsFlatForce;
    }
}

