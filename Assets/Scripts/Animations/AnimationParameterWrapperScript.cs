using System;
using UnityEngine;

[Serializable]
class AnimationParameterWrapperScript : MonoBehaviour
{
    [SerializeField] Animator _animator;
    private int _animVelocityXFloatID, _animVelocityYFloatID, _animVelocityZFloatID, _animSpeedFloatID, _animDashXFloatID, _animDashYFloatID, _animDashZFloatID, _animVelocityMagnitudeFloatID;
    private int _animAttack1HeldBoolID, _animIsTouchingWallBoolID, _animIsClimbingBoolID, _animIsRunningBoolID;
    private int _animGroundedStateEnumID, _animWallLookStateEnumID;

    private int _animUpdateGroundedStateTriggerID, _animUpdateWallLookStateTriggerID, _animJumpTriggerID, _animLandStableTriggerID, _animDashTriggerID;

    private void Awake()
    {
        _animVelocityXFloatID = Animator.StringToHash("VelocityX");
        _animVelocityYFloatID = Animator.StringToHash("VelocityY");
        _animVelocityZFloatID = Animator.StringToHash("VelocityZ");
        _animVelocityMagnitudeFloatID = Animator.StringToHash("VelocityMagnitude");
        _animSpeedFloatID = Animator.StringToHash("SpeedFloat");
        _animDashXFloatID = Animator.StringToHash("DashX");
        _animDashYFloatID = Animator.StringToHash("DashY");
        _animDashZFloatID = Animator.StringToHash("DashZ");

        _animAttack1HeldBoolID = Animator.StringToHash("Attack1Held");
        _animIsTouchingWallBoolID = Animator.StringToHash("IsTouchingWall");
        _animIsClimbingBoolID = Animator.StringToHash("IsClimbing");
        _animIsRunningBoolID = Animator.StringToHash("IsRunning");

        _animGroundedStateEnumID = Animator.StringToHash("CharacterGroundedStateEnumIndex");
        _animWallLookStateEnumID = Animator.StringToHash("CharacterWallLookStateEnumIndex");

        _animUpdateGroundedStateTriggerID = Animator.StringToHash("UpdateGroundedStateTrigger");
        _animUpdateWallLookStateTriggerID = Animator.StringToHash("UpdateWallLookStateTrigger");
        _animJumpTriggerID = Animator.StringToHash("JumpTrigger");
        _animLandStableTriggerID = Animator.StringToHash("LandStableTrigger");
        _animDashTriggerID = Animator.StringToHash("DashTrigger");
    }

    public void SetVelocityX(float animatorVelocityX, float velocityDampTime=0.1f)
    {
        _animator.SetFloat(_animVelocityXFloatID, animatorVelocityX, velocityDampTime, Time.deltaTime);
    }

    public void SetVelocityY(float animatorVelocityY, float velocityDampTime = 0.1f)
    {
        _animator.SetFloat(_animVelocityYFloatID, animatorVelocityY, velocityDampTime, Time.deltaTime);
    }

    public void SetVelocityZ(float animatorVelocityZ, float velocityDampTime = 0.1f)
    {
        _animator.SetFloat(_animVelocityZFloatID, animatorVelocityZ, velocityDampTime, Time.deltaTime);
    }

    public void SetVelocityMagnitude(float magnitude, float velocityDampTime = 0.1f)
    {
        _animator.SetFloat(_animVelocityMagnitudeFloatID, magnitude, velocityDampTime, Time.deltaTime);
    }

    public void SetDashX(float animatorDashX, float velocityDampTime = 0.1f)
    {
        _animator.SetFloat(_animDashXFloatID, animatorDashX, velocityDampTime, Time.deltaTime);
    }

    public void SetDashY(float animatorDashY, float velocityDampTime = 0.1f)
    {
        _animator.SetFloat(_animDashYFloatID, animatorDashY, velocityDampTime, Time.deltaTime);
    }

    public void SetDashZ(float animatorDashZ, float velocityDampTime = 0.1f)
    {
        _animator.SetFloat(_animDashZFloatID, animatorDashZ, velocityDampTime, Time.deltaTime);
    }

    public void SetSpeedFloat(float value, float velocityDampTime = 0.1f)
    {
        _animator.SetFloat(_animSpeedFloatID, value, velocityDampTime, Time.deltaTime);
    }

    public void SetIsClimbingBool(bool isClimbing)
    {
        _animator.SetBool(_animIsClimbingBoolID, isClimbing);
    }

    public void SetIsTouchingWallBool(bool isTouchingWall)
    {
        _animator.SetBool(_animIsTouchingWallBoolID, isTouchingWall);
    }

    public void SetAttack1State(bool isHeld)
    {
        _animator.SetBool(_animAttack1HeldBoolID, isHeld);
    }

    public void SetGroundedStateEnum(int enumIndex)
    {
        _animator.SetInteger(_animGroundedStateEnumID, enumIndex);
    }

    public void SetWallLookStateEnum(int enumIndex)
    {
        _animator.SetInteger(_animWallLookStateEnumID, enumIndex);
    }

    public void SetIsRunningBool(bool value)
    {
        _animator.SetBool(_animIsRunningBoolID, value);
    }

    public void SetGroundedStateTrigger()
    {
        _animator.SetTrigger(_animUpdateGroundedStateTriggerID);
    }

    public void SetWallLookStateTrigger()
    {
        _animator.SetTrigger(_animUpdateWallLookStateTriggerID);
    }

    public void ResetWallLookStateTrigger()
    {
        _animator.ResetTrigger(_animUpdateWallLookStateTriggerID);
    }

    public void SetJumpTrigger()
    {
        _animator.SetTrigger(_animJumpTriggerID);
    }

    public void SetDashTrigger()
    {
        _animator.SetTrigger(_animDashTriggerID);
    }

    public void SetLandStableTrigger()
    {
        _animator.SetTrigger(_animLandStableTriggerID);
    }

    public void ResetLandStableTrigger()
    {
        _animator.ResetTrigger(_animLandStableTriggerID);
    }

    public void SetAnimatorFloat(int animVarHash, float value)
    {
        _animator.SetFloat(animVarHash, value);
    }

    public void SetAnimatorBool(int animVarHash, bool value)
    {
        _animator.SetBool(animVarHash, value);
    }

    public void SetAnimatorInt(int animVarHash, int value)
    {
        _animator.SetInteger(animVarHash, value);
    }
}

