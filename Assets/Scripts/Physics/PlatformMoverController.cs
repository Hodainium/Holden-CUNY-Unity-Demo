using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Playables;

public class PlatformMoverController : MonoBehaviour, IMoverController
{
    [SerializeField] PhysicsMover Mover;

    [SerializeField] float _speed = 1f;

    [SerializeField] PlayableDirector Director;

    private Transform _transform;

    private void Start()
    {
        _transform = this.transform;
        Director.timeUpdateMode = DirectorUpdateMode.Manual;

        Mover.MoverController = this;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        Vector3 positionBeforeAnim = _transform.position;
        Quaternion rotationBeforeAnim = _transform.rotation;

        EvaluateAtTime(Time.time * _speed);

        goalPosition = _transform.position;
        goalRotation = _transform.rotation;

        _transform.position = positionBeforeAnim;
        _transform.rotation = rotationBeforeAnim;
    }

    public void EvaluateAtTime(double time)
    {
        Director.time = time % Director.duration;
        Director.Evaluate();
    }
}
