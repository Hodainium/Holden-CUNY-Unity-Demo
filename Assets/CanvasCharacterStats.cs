using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController.Examples;
using TMPro;
using UnityEngine.UI;

public class CanvasCharacterStats : MonoBehaviour
{
    [SerializeField] HoldenExamplePlayer holdenExamplePlayer;
    private HoldenExampleCharacterController _playerController;
    [SerializeField] TMP_Text _textUIObject;
    [SerializeField] Image _climbMeterChargeBarImage;
    [SerializeField] Image _runMeterChargeBarImage;
    private bool _isWallTransition;
    string tmpString;
    private float _climbTimer, _runTimer;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = holdenExamplePlayer.Character;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        tmpString = "Base Velocity= " + _playerController.Motor.BaseVelocity.magnitude.ToString() + "\n" +
            "Velocity vector= " + _playerController.Motor.BaseVelocity.ToString() + "\n" +
            "Player ground state= " + _playerController.CurrentCharacterGroundedState.ToString() + "\n" +
            "Player stance state= " + _playerController.CurrentCharacterState.ToString() + "\n" +
            "Player wall look state= " + _playerController.CurrentCharacterWallLookState.ToString() + "\n" +
            "Is touching wall= " + _playerController.IsTouchingWall().ToString() + "\n" +
            "Magnet charge= " + _playerController.GetClimbTimer().ToString() + "\n" + 
            "Jump count= " + _playerController.JumpCountDebug.ToString();

        _textUIObject.text = tmpString;

        _climbTimer = _playerController.GetClimbTimer();
        _runTimer = _playerController.GetRunTimer();

        _climbMeterChargeBarImage.fillAmount = _climbTimer/_playerController.MaxClimbCharge;
        _runMeterChargeBarImage.fillAmount = _runTimer/ _playerController.MaxClimbCharge;
    }
}
