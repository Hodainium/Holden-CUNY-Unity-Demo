using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Playables;

public class BananaMoverController : MonoBehaviour, IMoverController
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
    [SerializeField] float _shakingTimeOffset = 0f;
    [SerializeField] bool ForceShake = false;
    [SerializeField] GameObject _bananaBoxObject;

    //[SerializeField] double _timeOffset = 0d;

    [SerializeField] PlayableDirector ShakingDirector;
    [SerializeField] PlayableDirector FallingDirector;
    [SerializeField] float _respawnTime = 5f;
    //private GameObject _parentObject;
    private BananaState _currentBananaState;
    private Transform _transform;
    private Timer _shakingTimer;
    private float _fallingTime;
    private double _fallingDurationTime;
    private float _lastTimeCheck;
    private float _shakingStartingTime;
    private Quaternion _defaultRotation;
    private Vector3 _defaultPosition;
    private MeshRenderer _bananaMesh;
    private Timer _respawnTimer;
    
    private bool _isBananaDisabled;

    private void Start()
    {
        _transform = this.transform;
        ShakingDirector.timeUpdateMode = DirectorUpdateMode.Manual;
        FallingDirector.timeUpdateMode = DirectorUpdateMode.Manual;
        _currentBananaState = BananaState.Stable;
        Mover.MoverController = this;
        _shakingTimer = new Timer(_shakingTime);
        _fallingDurationTime = FallingDirector.duration;
        _bananaMesh = this.gameObject.GetComponent<MeshRenderer>();
        _respawnTimer = new Timer(_respawnTime);

        _defaultRotation = this.transform.rotation;
        _defaultPosition = this.transform.position;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 3) 
        {
            if (_currentBananaState == BananaState.Stable)
            {                
                StartShaking();
            }
        }
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        Vector3 positionBeforeAnim = _transform.position;
        Quaternion rotationBeforeAnim = _transform.rotation;

        if (_isBananaDisabled)
        {
            if (!_respawnTimer.IsActive())
            {
                ResetBanana();
            }
        }
        else
        {
            switch (_currentBananaState)
            {
                case BananaState.Shaking:
                    {
                        if (_shakingTimer.IsActive())
                        {
                            EvaluateAtTimeShaking((Time.time - _shakingStartingTime * _shakingSpeed) + _shakingTimeOffset);
                        }
                        else
                        {
                            _currentBananaState = BananaState.Falling;


                            _fallingTime = 0f;
                            _lastTimeCheck = Time.time;
                            DisableBananaBox();
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
                            DisableBananaMesh();
                            _respawnTimer.ResetTimer();
                            _isBananaDisabled = true;
                            //EvaluateAtTimeFalling((_fallingDurationTime * _fallingSpeed));

                        }
                        else
                        {
                            EvaluateAtTimeFalling((_fallingTime * _fallingSpeed));
                        }
                        break;
                    }


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

    public void EnableBananaMesh()
    {
        _bananaMesh.enabled = true;

    }

    public void DisableBananaMesh()
    {
        _bananaMesh.enabled = false;

    }

    public void EnableBananaBox()
    {
        _bananaBoxObject.SetActive(true);
    }

    public void DisableBananaBox()
    {        
        _bananaBoxObject.SetActive(false);
    }

    private void StartShaking()
    {
        _currentBananaState = BananaState.Shaking;
        _shakingTimer.ResetTimer();
        _shakingStartingTime = Time.time;
    }

    private void ResetBanana()
    {
        _currentBananaState = BananaState.Stable;
        EnableBananaMesh();
        EnableBananaBox();
        this.transform.position = _defaultPosition;
        this.transform.rotation = _defaultRotation;
        _isBananaDisabled = false;
    }
}
