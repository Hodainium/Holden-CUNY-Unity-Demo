using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Playables;

public class BananaMoverControllerNew : MonoBehaviour, IMoverController
{
    public enum BananaState
    {
        Stable,
        Shaking,
        Falling
    }

    [SerializeField] PhysicsMover Mover;

    [SerializeField] float _shakingSpeed = 1f;
    [SerializeField] float _fallingSpeed = 1f;

    [SerializeField] float _shakingTime = 1f;

    //[SerializeField] double _timeOffset = 0d;

    [SerializeField] PlayableDirector ShakingDirector;
    [SerializeField] PlayableDirector FallingDirector;
    private BananaState _currentBananaState;
    private Transform _transform;
    private Timer _shakingTimer;
    private float _fallingTime;
    private double _fallingDurationTime;
    private float _lastTimeCheck;

    private void Start()
    {
        _transform = this.transform;
        ShakingDirector.timeUpdateMode = DirectorUpdateMode.Manual;
        FallingDirector.timeUpdateMode = DirectorUpdateMode.Manual;
        _currentBananaState = BananaState.Stable;
        Mover.MoverController = this;
        _shakingTimer = new Timer(_shakingTime);
        _fallingDurationTime = FallingDirector.duration;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 3) 
        {
            if (_currentBananaState == BananaState.Stable)
            {
                _currentBananaState = BananaState.Shaking;
                _shakingTimer.ResetTimer();
            }
        }
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        Vector3 positionBeforeAnim = _transform.position;
        Quaternion rotationBeforeAnim = _transform.rotation;



        switch (_currentBananaState)
        {
            case BananaState.Shaking:
                {
                    if (_shakingTimer.IsActive())
                    {
                        EvaluateAtTimeShaking((Time.time * _shakingSpeed));
                    }
                    else
                    {
                        _currentBananaState = BananaState.Falling;


                        _fallingTime = 0f;
                        _lastTimeCheck = Time.time;
                        EvaluateAtTimeFalling((_fallingTime * _fallingSpeed));
                    }


                    break;
                }
            case BananaState.Falling:
                {
                    _fallingTime += Time.time - _lastTimeCheck;                    
                    _lastTimeCheck = Time.time;

                    if (_fallingTime >= _fallingDurationTime)
                    {
                        this.gameObject.SetActive(false);
                        //EvaluateAtTimeFalling((_fallingDurationTime * _fallingSpeed));
                        
                    }
                    else
                    {
                        Debug.Log("Falling time: " + _fallingTime);
                        EvaluateAtTimeFalling((_fallingTime * _fallingSpeed));
                    }
                    break;
                }

                
        }
        


        goalPosition = _transform.position;
        goalRotation = _transform.rotation;

        _transform.position = positionBeforeAnim;
        _transform.rotation = rotationBeforeAnim;
    }

    public void EvaluateAtTimeShaking(double time)
    {
        ShakingDirector.time = time % ShakingDirector.duration;
        ShakingDirector.Evaluate();
    }

    public void EvaluateAtTimeFalling(double time)
    {
        FallingDirector.time = time % FallingDirector.duration;
        FallingDirector.Evaluate();
    }
}
