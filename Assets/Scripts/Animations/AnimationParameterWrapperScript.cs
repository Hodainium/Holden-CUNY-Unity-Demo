using System;
using UnityEngine;

[Serializable]
class AnimationParameterWrapperScript : MonoBehaviour
{
    [SerializeField] Animator _animatorPlayerMesh;
    [SerializeField] Animator _animatorPlayerRoot;
    private int _animVelocityXFloatID, _animVelocityYFloatID, _animVelocityZFloatID, _animSpeedFloatID, _animDashXFloatID, _animDashYFloatID, _animDashZFloatID, _animVelocityMagnitudeFloatID;
    private int _animAttack1HeldBoolID, _animIsTouchingWallBoolID, _animIsWallRunningBoolID, _animIsRunningBoolID, _animIsWallRunLeftBoolID;
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
        _animIsWallRunningBoolID = Animator.StringToHash("IsWallRunning");
        _animIsRunningBoolID = Animator.StringToHash("IsRunning");
        _animIsWallRunLeftBoolID = Animator.StringToHash("IsWallRunLeft");

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
        _animatorPlayerMesh.SetFloat(_animVelocityXFloatID, animatorVelocityX, velocityDampTime, Time.deltaTime);
    }

    public void SetVelocityY(float animatorVelocityY, float velocityDampTime = 0.1f)
    {
        _animatorPlayerMesh.SetFloat(_animVelocityYFloatID, animatorVelocityY, velocityDampTime, Time.deltaTime);
    }

    public void SetVelocityZ(float animatorVelocityZ, float velocityDampTime = 0.1f)
    {
        _animatorPlayerMesh.SetFloat(_animVelocityZFloatID, animatorVelocityZ, velocityDampTime, Time.deltaTime);
    }

    public void SetVelocityMagnitude(float magnitude, float velocityDampTime = 0.1f)
    {
        _animatorPlayerMesh.SetFloat(_animVelocityMagnitudeFloatID, magnitude, velocityDampTime, Time.deltaTime);
    }

    public void SetDashX(float animatorDashX, float velocityDampTime = 0.1f)
    {
        _animatorPlayerMesh.SetFloat(_animDashXFloatID, animatorDashX, velocityDampTime, Time.deltaTime);
    }

    public void SetDashY(float animatorDashY, float velocityDampTime = 0.1f)
    {
        _animatorPlayerMesh.SetFloat(_animDashYFloatID, animatorDashY, velocityDampTime, Time.deltaTime);
    }

    public void SetDashZ(float animatorDashZ, float velocityDampTime = 0.1f)
    {
        _animatorPlayerMesh.SetFloat(_animDashZFloatID, animatorDashZ, velocityDampTime, Time.deltaTime);
    }

    public void SetSpeedFloat(float value, float velocityDampTime = 0.1f)
    {
        _animatorPlayerMesh.SetFloat(_animSpeedFloatID, value, velocityDampTime, Time.deltaTime);
    }

    public void SetIsWallRunningBool(bool isClimbing)
    {
        _animatorPlayerMesh.SetBool(_animIsWallRunningBoolID, isClimbing);
    }

    public void SetIsWallRunningBool2(bool isClimbing)
    {
        _animatorPlayerRoot.SetBool(_animIsWallRunningBoolID, isClimbing);
    }

    public void SetIsTouchingWallBool(bool isTouchingWall)
    {
        _animatorPlayerMesh.SetBool(_animIsTouchingWallBoolID, isTouchingWall);
    }

    public void SetIsWallRunLeftBool(bool isWallRunLeft)
    {
        _animatorPlayerMesh.SetBool(_animIsWallRunLeftBoolID, isWallRunLeft);
    }

    public void SetAttack1State(bool isHeld)
    {
        _animatorPlayerMesh.SetBool(_animAttack1HeldBoolID, isHeld);
    }

    public void SetGroundedStateEnum(int enumIndex)
    {
        _animatorPlayerMesh.SetInteger(_animGroundedStateEnumID, enumIndex);
    }

    public void SetWallLookStateEnum(int enumIndex)
    {
        _animatorPlayerMesh.SetInteger(_animWallLookStateEnumID, enumIndex);
    }

    public void SetWallLookStateEnum2(int enumIndex)
    {
        _animatorPlayerRoot.SetInteger(_animWallLookStateEnumID, enumIndex);
    }

    public void SetIsRunningBool(bool value)
    {
        _animatorPlayerMesh.SetBool(_animIsRunningBoolID, value);
    }

    public void SetGroundedStateTrigger()
    {
        _animatorPlayerMesh.SetTrigger(_animUpdateGroundedStateTriggerID);
    }

    public void SetWallLookStateTrigger()
    {
        _animatorPlayerMesh.SetTrigger(_animUpdateWallLookStateTriggerID);
    }

    public void ResetWallLookStateTrigger()
    {
        _animatorPlayerMesh.ResetTrigger(_animUpdateWallLookStateTriggerID);
    }

    public void SetJumpTrigger()
    {
        _animatorPlayerMesh.SetTrigger(_animJumpTriggerID);
    }

    public void SetDashTrigger()
    {
        _animatorPlayerMesh.SetTrigger(_animDashTriggerID);
    }

    public void SetLandStableTrigger()
    {
        _animatorPlayerMesh.SetTrigger(_animLandStableTriggerID);
    }

    public void ResetLandStableTrigger()
    {
        _animatorPlayerMesh.ResetTrigger(_animLandStableTriggerID);
    }

    public void SetAnimatorFloat(int animVarHash, float value)
    {
        _animatorPlayerMesh.SetFloat(animVarHash, value);
    }

    public void SetAnimatorBool(int animVarHash, bool value)
    {
        _animatorPlayerMesh.SetBool(animVarHash, value);
    }

    public void SetAnimatorInt(int animVarHash, int value)
    {
        _animatorPlayerMesh.SetInteger(animVarHash, value);
    }
}

