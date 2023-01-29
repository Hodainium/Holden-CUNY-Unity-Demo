using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPointLogic : MonoBehaviour
{
    public static Transform _currentRespawnTransform;
    private BoxCollider _triggerCollider;
    private bool _isDefaultPoint;
    // Start is called before the first frame update
    void Start()
    {
        _triggerCollider = this.gameObject.GetComponent<BoxCollider>();
        if (_isDefaultPoint)
        {
            _currentRespawnTransform = this.transform;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 3)
        {
            _triggerCollider.enabled = false;
            _currentRespawnTransform = this.transform;
        }
    }
}
