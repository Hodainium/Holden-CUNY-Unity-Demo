using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityStats))]
[RequireComponent(typeof(EntityBehaviorScript))]

public class EntityLogic : MonoBehaviour//, IDamageable
{
    //[SerializeField] GameObject enemyHealthSpriteObject;
    [SerializeField] EntityStats _entityStats;
    [SerializeField] EntityBehaviorScript _entityBehaviorScript;
    private float _lastHP;

    //incorporate enemy behavior here

    private void Awake()
    {
        _lastHP = _entityStats.GetTotalHP();
    }

    public void CheckHP()
    {
        float currentHP = _entityStats.GetCurrentHP();
        Debug.Log("HP was : " + _lastHP + ". Now is: " + currentHP);
        if (_lastHP > currentHP)
        {
            
            // Damage behavior. Check to see if dead before doing hurt behavior.
            if (currentHP <= 0)
            {
                TriggerDeathBehavior();
                KillEntity();
            }
            else
            {
                TriggerDamageBehavior();
            }
        }
        else if (_lastHP < currentHP)
        {
            //healing

            TriggerHealBehavior();
            //overheal maybe goes here
        }

        SetLastHPToCurrent();
        
        // Nothing happens here because no damage was healed or dealt.
    }

    public float GetPercentHP()
    {
        return (_entityStats.GetCurrentHP() / _entityStats.GetTotalHP());
    }

    public void KillEntity()
    {
        Debug.Log(this.gameObject.name + " has been killed");
        this.gameObject.SetActive(false);
    }

    public void RecieveHit(HitInfo tempHitInfo)
    {
        float damageDone = SendDamage(tempHitInfo.GetDamageInfo());
        SendPhysics(tempHitInfo.GetPhysicsInfo(), damageDone);
    }

    private float SendDamage(DamageInfo damageInfo)
    {

        return _entityStats.RecieveDamage(damageInfo);
    }

    private void TriggerDamageBehavior()
    {
        // Handle damage behavior
        _entityBehaviorScript.OnDamageBehavior();
    }

    private void TriggerHealBehavior()
    {
        // Handle heal behavior
        _entityBehaviorScript.OnHealBehavior();
    }

    private void TriggerDeathBehavior()
    {
        // Handle heal behavior
        _entityBehaviorScript.OnDeathBehavior();
    }

    private void SendPhysics(PhysicsInfo physicsInfo, float damageDone)
    {
        //Vector3 newPos = transform.position + (Vector3)(movementVector * movementMultiplier);

        if (physicsInfo.IsFlat())
        {
            //Physics(flat value)
        }
        else
        {
            //get damage done
            //physics(scaled number)
        }
    }

    private void SetLastHPToCurrent()
    {
        _lastHP = _entityStats.GetCurrentHP();
    }

}
