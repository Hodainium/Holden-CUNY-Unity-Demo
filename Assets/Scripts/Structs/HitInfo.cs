using System;
using UnityEngine;

[Serializable]
public struct HitInfo
{
    [SerializeField] DamageInfo damageInfo;
    [SerializeField] PhysicsInfo physicsInfo;

    HitInfo(DamageInfo tempDamageInfo, PhysicsInfo tempPhysicsInfo)
    {
        damageInfo = tempDamageInfo;
        physicsInfo = tempPhysicsInfo;
    }

    public DamageInfo GetDamageInfo()
    {
        return damageInfo;
    }

    public PhysicsInfo GetPhysicsInfo()
    {
        return physicsInfo;
    }
}
