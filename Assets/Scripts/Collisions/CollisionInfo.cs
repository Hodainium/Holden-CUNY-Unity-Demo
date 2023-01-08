using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInfo : MonoBehaviour
{
    public enum ColliderType
    {
        Obstacle,
        Enemy,
        Projectile,
        PLAYER
    }

    [SerializeField] ColliderType _colliderType;

    public ColliderType GetColliderType()
    {
        return _colliderType;
    }
}
