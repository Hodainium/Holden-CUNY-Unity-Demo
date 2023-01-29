using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class PortalLogic : MonoBehaviour
{
    [SerializeField] Transform _teleportPointTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (!_teleportPointTransform)
        {
            _teleportPointTransform = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.layer == 3)
        {
            collider.gameObject.GetComponent<KinematicCharacterMotor>().SetPosition(_teleportPointTransform.position);
        }
    }
}
