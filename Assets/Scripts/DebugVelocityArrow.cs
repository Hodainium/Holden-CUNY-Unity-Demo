using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController.Examples;

public class DebugVelocityArrow : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float XOffset = 0f;
    [SerializeField] HoldenExampleCharacterController _playerController;
    [SerializeField] Camera PlayerCamera;
    Transform Transform; 

    void Start()
    {
        Transform = this.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Quaternion rotation = new Quaternion();
        //Gizmos.DrawSphere(PlayerController.CameraFollowPoint.position + PlayerController.Motor.BaseVelocity, 1f);
        Transform.position = _playerController.CameraFollowPoint.position;
        //Vector3 vect = Transform.position + PlayerController.Motor.BaseVelocity;
        //vect.x *= -1;

        if (_playerController.Motor.BaseVelocity.sqrMagnitude > 0.00001f)
        {
            rotation.SetLookRotation(_playerController.Motor.BaseVelocity, Transform.up); //* PlayerController.MeshRoot.rotation;
            Transform.rotation = rotation;
            //Transform.rotation *= Quaternion.Euler(0, -90, 0); // this adds a 90 degrees Y rotation
        }
        else
        {
            Transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.right);
        }
        
    }
}
