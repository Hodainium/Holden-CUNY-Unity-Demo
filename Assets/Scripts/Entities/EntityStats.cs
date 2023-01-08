using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityLogic))]

public class EntityStats : MonoBehaviour
{
    [SerializeField] float _baseHP = 100;
    [SerializeField] int _level = 1;
    [SerializeField] EntityLogic _entityLogic;

    [EnumNamedArray(typeof(DamageInfo.DamageType))]
    [SerializeField] float[] _BaseResistanceArray = new float[Enum.GetValues(typeof(DamageInfo.DamageType)).Length];
    

    Dictionary<DamageInfo.DamageType, float> _Resistances = new Dictionary<DamageInfo.DamageType, float>();

    float _totalHP, _currentHP;

    private void Awake()
    {
        SetResistances(_BaseResistanceArray);
        SetTotalHP();
        ResetCurrentHP();

    }

    void SetTotalHP()
    {
        _totalHP = _baseHP;        
    }

    void ResetCurrentHP()
    {
        _currentHP = _totalHP;
    }

    void SetResistance(DamageInfo.DamageType damageType, float resistanceValue)
    {
        _Resistances[damageType] = resistanceValue;
    }

    void SetResistances(float[] resistanceValues) //sets all values of an array to elements
    {
        int i = 0;
        foreach (DamageInfo.DamageType damageType in Enum.GetValues(typeof(DamageInfo.DamageType)))
        {
            //Debug.Log("Setting " + damageType + " resistance to a value of " + resistanceValues[i]);
            _Resistances[damageType] = resistanceValues[i];
            i++;
        }
    }

    void ResetResistToBase() //sets all values of an array to elements
    {
        SetResistances(_BaseResistanceArray);
    }

    void SetResistanceConditional(float[] tempResistanceValues) //sets all values of an array to elements EXCEPT ones that are 0
    {
        int i = 0;
        foreach (DamageInfo.DamageType damageType in Enum.GetValues(typeof(DamageInfo.DamageType)))
        {
            if (tempResistanceValues[i] != 0f)
            {
                _Resistances[damageType] = tempResistanceValues[i];
            }
            i++;
        }
    }

    void DealDamage(float finalDamage)
    {

        DecreaseHP(finalDamage);
        _entityLogic.CheckHP();
    }

    void HealDamage(float damage)
    {        
        Debug.Log("Healing and overheal not fully implemented");
        IncreaseHP(Mathf.Abs(damage));
        _entityLogic.CheckHP();
    }

    void DecreaseHP(float damage)
    {
        _currentHP -= damage;
        Debug.Log("Did " + damage + " damage");
    }

    void IncreaseHP(float health)
    {
        _currentHP += health;
    }

    public float GetCurrentHP()
    {
        return _currentHP;
    }

    public float GetTotalHP()
    {
        return _totalHP;
    }

    public float RecieveDamage(DamageInfo damageInfo)
    {
        float calcDamage;
        float resistanceValueToType = _Resistances[damageInfo.GetDamageType()];
        if (resistanceValueToType > 0)
        {
            calcDamage = damageInfo.GetDamage() * ((100 - resistanceValueToType) / 100);
            //Output text of damage along with color of elemental type
            DealDamage(calcDamage);
        }
        else if (resistanceValueToType < 0)
        {
            calcDamage = resistanceValueToType * 0.5f * damageInfo.GetDamage(); // Negative number
            HealDamage(calcDamage);
        }
        else
        {
            calcDamage = 0;
            //do nothing cause 0 is null
        }

        return calcDamage;
    }

    //implement dictionary for resistances so that the damage type aligns with resistance value
    //hp is float value
    //level is float value
    //somehow incorporate level into damage and resitance scaling
    //base values with a scaling bool?
}

