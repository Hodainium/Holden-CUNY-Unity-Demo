using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBehaviorScript : MonoBehaviour
{
    //[SerializeField] Transform healthSpriteTransform;
    [SerializeField] Transform NPCTransform;
    [SerializeField] EntityLogic _EntityLogic;

    public void OnDamageBehavior()
    {
        NPCTransform.localScale = NPCTransform.localScale * _EntityLogic.GetPercentHP();
    }

    public void OnHealBehavior()
    {
        NPCTransform.localScale = NPCTransform.localScale * _EntityLogic.GetPercentHP();
    }

    public void OnDeathBehavior()
    {
        // Play death anim.
    }
}
