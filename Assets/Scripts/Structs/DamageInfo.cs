using System;
using UnityEngine;

[Serializable]
public struct DamageInfo // need to change these public to private with methods to access
{
    public enum DamageType { Water, Fire, Air, Earth, None };

    [SerializeField] DamageType _damageType;
    [SerializeField] float _damage;

    public DamageInfo(DamageType damageTypeTemp = DamageType.None, float damageTemp = 0f)
    {
        _damageType = damageTypeTemp;
        _damage = damageTemp;
    }

    public DamageType GetDamageType()
    {
        return _damageType;
    }

    public float GetDamage()
    {
        return _damage;
    }

}
