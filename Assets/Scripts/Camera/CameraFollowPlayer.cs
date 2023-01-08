using System;
using UnityEngine;

class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] Transform _playerTransform;
    Vector3 transformDifference;

    private void Awake()
    {
        transformDifference = transform.position - _playerTransform.position;
    }

    private void Update()
    {
        transform.position = _playerTransform.position + transformDifference;
    }
}

