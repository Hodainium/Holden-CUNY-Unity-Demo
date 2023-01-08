using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorLogic : MonoBehaviour//, IDamage, IPhysics
{
    
    [SerializeField] HitInfo hitInfo; //change hitinfo to array even if it is one it can be a one aray

    [SerializeField] SensorObject sensor;

    private void Awake()
    {
        //iPhysicsRef = this.gameObject.GetComponent<IPhysics>();
        //iDamageRef = this.gameObject.GetComponent<IDamage>();
    }

    public void SendHit(RaycastHit2D hit)
    {
        EntityLogic entityLogic = hit.collider.gameObject.GetComponent<EntityLogic>();

        entityLogic.RecieveHit(hitInfo); 
    }

}

