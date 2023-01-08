using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    [SerializeField] CollisionInfo _collisionInfo;

    //[SerializeField] HitInfo _hitInfo;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collisionentered");

        CollisionInfo otherCollisionInfo = collision.gameObject.GetComponent<CollisionInfo>();

        if (otherCollisionInfo == null)
        {
            Debug.LogError("Collision info returned NULL!");
        }
        else
        {
            switch (otherCollisionInfo.GetColliderType())
            {
                case CollisionInfo.ColliderType.Enemy:
                    {
                        //Decide who pushes who
                        //Do damage if it's done
                        break;
                    }
                case CollisionInfo.ColliderType.Obstacle:
                    {
                        Debug.Log("Yes");
                        float rayDrawDistance = 0.5f;
                        Debug.DrawRay(collision.GetContact(0).point, collision.GetContact(0).normal * rayDrawDistance);
                        break;
                    }
                case CollisionInfo.ColliderType.Projectile:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }
    }

}
