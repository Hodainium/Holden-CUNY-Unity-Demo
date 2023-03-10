using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using TMPro;
using UnityEngine.UI;

public class CanvasCharacterStats : MonoBehaviour
{
    [SerializeField] HoldenPlayerManager _playerManager;
    private HoldenCharacterController _playerController;
    [SerializeField] TMP_Text _textUIObject;
    [SerializeField] Image _climbMeterChargeBarImage;
    [SerializeField] Image _runMeterChargeBarImage;
    [SerializeField] Image _dashTimerChargeBarImage;
    private bool _isWallTransition;
    string tmpString;
    private float _climbTimer, _runTimer, _dashTimer;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = _playerManager.CharacterController;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        tmpString = "Base Velocity= " + _playerController.Motor.BaseVelocity.magnitude.ToString() + "\n" +
            "Velocity vector= " + _playerController.Motor.BaseVelocity.ToString() + "\n" +
            "Movement value= " + _playerController.PeekMovementValue().ToString() + "\n" +
            "Player ground state= " + _playerController.CurrentCharacterGroundedState.ToString() + "\n" +
            "Player wall look state= " + _playerController.CurrentCharacterWallLookState.ToString() + "\n" +
            "Is wall running= " + _playerController.DebugGetIsClimbing().ToString() + "\n" +
            "Dash count= " + _playerController.DashCountDebug.ToString() + "\n" +
            "Jump count= " + _playerController.JumpCountDebug.ToString();

        _textUIObject.text = tmpString;

        //_climbTimer = _playerController.GetClimbTimer();
        //_runTimer = _playerController.GetRunTimer();
        //_dashTimer = _playerController.GetDashTimer();

        //_climbMeterChargeBarImage.fillAmount = _climbTimer/_playerController.MaxClimbCharge;
        //_runMeterChargeBarImage.fillAmount = _runTimer/ _playerController.MaxClimbCharge;
        //_dashTimerChargeBarImage.fillAmount = _dashTimer / _playerController.DashCoolDownTimeInFrames;
    }
}
