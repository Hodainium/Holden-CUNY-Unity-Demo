using System;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public enum CharacterState
    {
        Default
    }

    public enum CharacterGroundedState
    {
        GroundedStable,
        GroundedUnstable,
        Airborne,
        Wall
    }

    public enum CharacterWallLookState
    {
        Forward,
        Left,
        Right,
        Away,
        NoWall
    }

    public enum CameraOrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchPressed;
        public bool CrouchUp;
        public bool Attack1Down;
        public bool Attack1Up;
        public bool ClimbDown;
        public bool ClimbUp;
        public bool RunDown;
        public bool RunUp;
        public bool DashPressed;
        public bool AimDashDown;
        public bool AimDashUp;        
    }

    public struct AICharacterInputs
    {
        public Vector3 MoveVector;
        public Vector3 LookVector;
    }

    public enum BonusOrientationMethod
    {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
        TowardsVelocityOnInputsPlane
    }

    public class HoldenExampleCharacterController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        [Header("Stable Movement")]
        public float MaxMoveWalkSpeed = 5f;
        public float MaxMoveJogSpeed = 10f;
        public float MaxMoveRunSpeed = 15f;
        public float MaxMoveBoostSpeed = 20f;
        public float MaxMoveStableSpeed = 22f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public CameraOrientationMethod CameraOrientationMethod = CameraOrientationMethod.TowardsCamera;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;
        public float MaxStableFallSpeed = 100f;

        [Header("Wall Sliding and Running")]
        public bool AllowWallRun = true;
        public float WallRunAccelerationSpeed = 30f;
        
        public float WallDrag = 0.3f;
        public float ForcedWallDrag = 0.5f;
        public float WallClimbMovementSharpness = 15f;
        public float WallInputControlledTractionMax = 0.9f;
        public float WallTraction = 0.5f;
        public float WallInputControlledSpeedMax = 5f;

        [Header("New Wall Climb stuff")]
        public float WallClimbMinimumNeededSpeed = 15f;
        public float MaxWallSpeedLROne = 20f;
        public float MaxWallSpeedLRTwo = 25f;
        public float WallClimbSpeedFrontHorizontalMax = 8f;
        public float WallClimbSpeedFrontVerticalOneMax = 15f;
        public float WallClimbSpeedFrontVerticalTwoMax = 20f;
        public float WallAttractForce = 0.1f;
        public float WallRepulseForce = 1f;
        public int WallHitFixedUpdateGraceCount = 2;
        public float xAngleOffset = 0f;

        [Header("Magnet")]
        public float MaxClimbCharge = 15f;
        public float ClimbDrainRate = 10f;

        [Header("Running")]
        public float MaxRunCharge = 15f;
        public float RunDrainRate = 10f;
        public float RunRechargeRate = 10f;

        [Header("Dashing")]
        public int AirDashCountMax = 2;
        public float DashForce = 5f;
        public int DashCoolDownTimeInFrames = 20;
        public int DashSpeedBoostTimeInFrames = 24;
        public int DashSpeedFreezeTimeInFrames = 10;
        public int DashSpeedPostExitTimeInFrames = 6;
        public float DashDrag = 5f;
        public AnimationCurve dashVelocityCurve;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public int JumpCountMax = 2;
        public float JumpUpSpeed = 10f;
        public float JumpForwardMomentumScalar = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;
        public Transform CameraFollowPoint;
        public float CrouchedCapsuleHeight = 1f;

        public CharacterState CurrentCharacterState { get; private set; }
        public CharacterGroundedState CurrentCharacterGroundedState { get; private set; }
        public CharacterWallLookState CurrentCharacterWallLookState { get; private set; }

        private CharacterWallLookState _previousCharacterWallLookState;

        private Collider[] _probedColliders = new Collider[8];
        private RaycastHit[] _probedHits = new RaycastHit[8];
        private Vector3 _moveInputVector;
        private Vector3 _moveInputRawVector;
        private Vector3 _lookInputVector;
        private bool _jumpRequested = false;
        private Vector3 _wallNormal;
        private bool _attack1Requested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private bool _shouldBeCrouching = false;
        private bool _isCrouching = false;
        private int _jumpCountCurrent;        
        private bool _wantsToClimb = false;
        private Vector3 _previousVelocity;
        private Vector3 _transitionalVelocity;
        private bool _hasTransitionalVelocity;
        private bool _isWallTransition;
        private Timer _wallHitCheckTimer;
        private Timer _dashCooldownTimer;
        private Timer _dashSpeedBoostTimer;
        private Timer _dashSpeedFreezeTimer;
        private float _currentClimbCharge = 0f;
        private float _currentRunCharge = 0f;
        private Collider _currentWallCollider = null;
        private Vector3 _moveInputRawXZX;
        private bool _wantsToRun;
        private float _movementSpeedFloat = 0;
        private bool _wantsToDash;
        private bool _isDashing;
        Vector3 _dashVector;
        private bool _isExitingDash = false;
        private int _frames;
        private int _frames1;
        private int _dashCountCurrent;
        private bool _isRunning;
        private bool _wantsToCrouch;
        private bool _isAimingDash;
        Vector3 _lookInputNonPlanarVector;
        private bool _isWallRunning;
        private Vector3 _wallHitPoint;
        private bool _wantsToTest;
        private bool _isWallJump;



        const float SIXTYFPSFRAMEINTERVAL = 0.01667f;

        public int JumpCountDebug { get { return _jumpCountCurrent; } }
        public int DashCountDebug { get { return _dashCountCurrent; } }

        private Vector3 lastInnerNormal = Vector3.zero;
        private Vector3 lastOuterNormal = Vector3.zero;

        

        [SerializeField] PlayerAttackScript _playerAttackScript;
        [SerializeField] PlayerMovementScript _playerMovementScript;
        [SerializeField] AnimationEventWrapperScript _animationEventWrapperScript;
        [SerializeField] AnimationParameterWrapperScript _playerAnimationParameterWrapperScript;

        private void Awake()
        {
            // Handle initial state
            CharacterStateTransitionTo(CharacterState.Default);
            CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedStable);

            _currentClimbCharge = MaxClimbCharge;
            _currentRunCharge = MaxRunCharge;

            _wallHitCheckTimer = new Timer(Time.fixedDeltaTime*WallHitFixedUpdateGraceCount);
            _dashCooldownTimer = new Timer(DashCoolDownTimeInFrames * SIXTYFPSFRAMEINTERVAL);
            _dashSpeedBoostTimer = new Timer(DashSpeedBoostTimeInFrames * SIXTYFPSFRAMEINTERVAL); //Value is length of a single 60fps frame interval in seconds
            _dashSpeedFreezeTimer = new Timer(DashSpeedFreezeTimeInFrames * SIXTYFPSFRAMEINTERVAL);

            // Assign the characterController to the motor
            Motor.CharacterController = this;
        }

        private void OnEnable()
        {
            _previousVelocity = Vector3.zero;
        }

        /// <summary>
        /// Handles movement state transitions and enter/exit callbacks
        /// </summary>
        

        public void ResetWallLookState()
        {
            CurrentCharacterWallLookState = CharacterWallLookState.NoWall;
        }

        public float PeekMovementValue()
        {
            return _movementSpeedFloat;
        }

        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            Vector3 cameraPlanarDirection;
            Quaternion cameraPlanarRotation;

            if (CurrentCharacterGroundedState == CharacterGroundedState.Wall)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
                if (cameraPlanarDirection.sqrMagnitude == 0f)
                {
                    cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
                }
                cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
            }
            else
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
                if (cameraPlanarDirection.sqrMagnitude == 0f)
                {
                    cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
                }
                cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
            }

            

            switch (CurrentCharacterState)
            {
                default:
                    {
                        // Move and look inputs
                        _moveInputVector = cameraPlanarRotation * moveInputVector;
                        _moveInputRawVector = moveInputVector;
                        _moveInputRawXZX = new Vector3(moveInputVector.x, moveInputVector.z, moveInputVector.x);
                        _lookInputNonPlanarVector = inputs.CameraRotation * Vector3.forward;

                        switch (CameraOrientationMethod)
                        {
                            case CameraOrientationMethod.TowardsCamera:
                                _lookInputVector = cameraPlanarDirection;
                                break;
                            case CameraOrientationMethod.TowardsMovement:
                                _lookInputVector = _moveInputVector.normalized;
                                break;
                        }

                        // Jumping input
                        if (inputs.JumpDown)
                        {
                            _timeSinceJumpRequested = 0f;
                            _jumpRequested = true;
                        }

                        if (inputs.Attack1Down)
                        {
                            _wantsToTest = true;
                            //_timeSinceJumpRequested = 0f;
                            //_jumpRequested = true;
                        }
                        if (inputs.Attack1Up)
                        {
                            _wantsToTest = false;
                            //_timeSinceJumpRequested = 0f;
                            //_jumpRequested = true;
                        }

                        if (inputs.ClimbDown)
                        {
                            _wantsToClimb = true;
                        }
                        
                        if (inputs.ClimbUp)
                        {
                            _wantsToClimb = false;
                        }

                        if (inputs.RunDown)
                        {
                            _wantsToRun = true;
                        }

                        if (inputs.RunUp)
                        {
                            _wantsToRun = false;
                        }

                        if (inputs.DashPressed)
                        {
                            _wantsToDash = true;
                        }

                        if (inputs.CrouchPressed)
                        {
                            _wantsToCrouch = !_wantsToCrouch;
                        }

                        if (inputs.AimDashDown)
                        {
                            _isAimingDash = true;
                        }

                        if (inputs.AimDashUp)
                        {
                            _isAimingDash = false;
                        }

                        // Crouching input
                        if (_wantsToCrouch)
                        {
                            _shouldBeCrouching = true;

                            if (!_isCrouching)
                            {
                                _isCrouching = true;
                                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                                MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                            }
                        }
                        else if (!_wantsToCrouch)
                        {
                            _shouldBeCrouching = false;
                        }

                        break;
                    }
            }
        }

        private void ResetCrouching()
        {
            _wantsToCrouch = false;
            _shouldBeCrouching = false;
        }

        /// <summary>
        /// This is called every frame by the AI script in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref AICharacterInputs inputs)
        {
            _moveInputVector = inputs.MoveVector;
            _lookInputVector = inputs.LookVector;
        }

        private Quaternion _tmpTransientRot;

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            Vector3 currentUp = (currentRotation * Vector3.up);

            

            if (CurrentCharacterGroundedState == CharacterGroundedState.Wall)
            {
                Vector3 wallRightDirection = Vector3.Cross(Motor.CharacterUp, _wallNormal);

                switch (CurrentCharacterWallLookState)
                {
                    case CharacterWallLookState.Forward:
                        {
                            currentRotation = Quaternion.LookRotation(-_wallNormal, Motor.CharacterUp);
                            break;
                        }

                    case CharacterWallLookState.Away:
                        currentRotation = Quaternion.LookRotation(_wallNormal, Motor.CharacterUp);
                        break;
                    case CharacterWallLookState.Left:
                        currentRotation = Quaternion.LookRotation(-wallRightDirection, Motor.CharacterUp);
                        break;
                    case CharacterWallLookState.Right:
                        currentRotation = Quaternion.LookRotation(wallRightDirection, Motor.CharacterUp);
                        break;
                }
            }
            else
            {
                if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                {
                    // Smoothly interpolate from current to target look direction
                    Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                    // Set the current rotation (which will be used by the KinematicCharacterMotor)
                    currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Vector3.up); //
                }

                
                switch (BonusOrientationMethod)
                {
                    case BonusOrientationMethod.TowardsGravity:
                        {
                            // Rotate from current up to invert gravity
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                            break;
                        }
                    case BonusOrientationMethod.TowardsGroundSlopeAndGravity:
                        {
                            if (Motor.GroundingStatus.IsStableOnGround)
                            {
                                Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);
                                
                                Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                                // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                                Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                            }
                            else
                            {
                                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;

                            }
                            break;
                        }
                    case BonusOrientationMethod.TowardsVelocityOnInputsPlane:
                        {
                            Vector3 velocityOnInputsPlane;
                            Vector3 smoothedLookInputDirection;

                            switch (CurrentCharacterGroundedState)
                            {
                                default:
                                    {
                                        velocityOnInputsPlane = Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterUp);
                                        smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, velocityOnInputsPlane, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                                        break;
                                    }
                            }
                            

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);

                            if (Motor.GroundingStatus.IsStableOnGround && !_wantsToTest)
                            {
                                Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

                                Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                                // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                                Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                            }
                            else
                            {
                                Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                                currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                            }

                            break;
                        }
                    default:
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                            break;
                        }
                }
            }

            

        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Debug.Log("Entering grounded state: " + CurrentCharacterGroundedState);
            switch (CurrentCharacterGroundedState)
            {
                case CharacterGroundedState.GroundedStable:
                    {
                        float currentVelocityMagnitude = currentVelocity.magnitude;

                        Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;                        

                        // Reorient velocity on slope
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                        // Calculate target velocity
                        Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                        Vector3 targetMovementVelocity;

                        float currentVelocityMagnitudeDirty = currentVelocity.magnitude + 0.1f; // re-evaluate magnitude

                        if (currentVelocityMagnitudeDirty >= MaxMoveBoostSpeed && CanRun() && (_wantsToRun || _isRunning)) // play boosting -> boosting
                        {
                            _movementSpeedFloat = 4;
                            targetMovementVelocity = reorientedInput * MaxMoveStableSpeed;
                        }
                        else if ((((currentVelocityMagnitudeDirty > MaxMoveRunSpeed) && _wantsToRun) || _isRunning) && CanRun()) //play running -> boosting          // && CanRun()
                        {
                            _movementSpeedFloat = 3 + (currentVelocityMagnitudeDirty - MaxMoveRunSpeed) / (MaxMoveBoostSpeed - MaxMoveRunSpeed); // we want an extra state here this code is
                            targetMovementVelocity = reorientedInput * MaxMoveBoostSpeed;
                            _isRunning = true;
                        }
                        else if (currentVelocityMagnitudeDirty > MaxMoveJogSpeed || _isExitingDash) //Play jogging -> running
                        {
                            _movementSpeedFloat = 2 + (currentVelocityMagnitudeDirty - MaxMoveJogSpeed) / (MaxMoveRunSpeed - MaxMoveJogSpeed);
                            targetMovementVelocity = reorientedInput * MaxMoveRunSpeed;
                        }
                        else if (currentVelocityMagnitudeDirty > MaxMoveWalkSpeed) //Play walking -> jogging
                        {
                            _movementSpeedFloat = 1 + (currentVelocityMagnitudeDirty - MaxMoveWalkSpeed)/(MaxMoveJogSpeed - MaxMoveWalkSpeed);
                            targetMovementVelocity = reorientedInput * MaxMoveJogSpeed;
                        }
                        else if (_moveInputVector.magnitude > 0.1f) //Play idle -> walking //Need to check for input or else cant move
                        {
                            _movementSpeedFloat = currentVelocityMagnitudeDirty/MaxMoveWalkSpeed;
                            targetMovementVelocity = reorientedInput * MaxMoveJogSpeed;
                        }
                        else //Play idle
                        {
                            _isRunning = false;
                            _movementSpeedFloat = 0f;
                            targetMovementVelocity = Vector3.zero;
                        }
                        // Smooth movement Velocity
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));

                        break;
                    }

                case CharacterGroundedState.GroundedUnstable:
                    {

                        // Add move input
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            Vector3 addedVelocity = GetAddedAirVelocity(ref currentVelocity, deltaTime);

                            // Prevent air-climbing sloped walls
                            if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)//If we are heading in a positive direction, push back away from the slope.
                            {
                                Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                                addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                            }

                            // Apply added velocity
                            currentVelocity += addedVelocity;
                        }

                        // Gravity
                        currentVelocity += Gravity * deltaTime;

                        // Drag
                        currentVelocity *= (1f / (1f + (Drag * deltaTime)));

                        break;
                    }

                case CharacterGroundedState.Airborne:
                    {

                        // Apply added velocity
                        currentVelocity += GetAddedAirVelocity(ref currentVelocity, deltaTime);

                        // Gravity
                        currentVelocity += Gravity * deltaTime;

                        // Drag
                        currentVelocity *= (1f / (1f + (Drag * deltaTime)));

                        break;
                    }

                case CharacterGroundedState.Wall:
                    {
                        Vector3 gravityVelocity = Vector3.zero;
                        Vector3 addedWallRunVelocity = Vector3.zero;
                        Vector3 wallRunBoostAdditive = Vector3.zero;
                        Vector3 wallClimbAdditive = Vector3.zero;
                        Vector3 wallClimbInputDirectionNormalized;
                        Vector3 targetMovementVelocity;

                        switch (CurrentCharacterWallLookState)
                        {
                            case CharacterWallLookState.Forward: //Start climbing wall
                                {
                                    float inputAgainstWall = Vector3.Dot(_moveInputVector, -_wallNormal);
                                    Debug.Log("Wall input: " + inputAgainstWall);
                                    if (CanClimb() && inputAgainstWall > -0.2f) //If we are above a certain velocity slide on the wall instead of climb. Uncontrollable velocity  
                                    {
                                        wallClimbInputDirectionNormalized = Vector3.ProjectOnPlane(_moveInputRawXZX, _wallNormal).normalized; //= _moveInputRawXZY.normalized; //
                                        
                                        targetMovementVelocity = Vector3.right * wallClimbInputDirectionNormalized.x * WallClimbSpeedFrontHorizontalMax;
                                        targetMovementVelocity *= (1f / (1f + (WallDrag * deltaTime)));
                                        gravityVelocity = Vector3.zero; 
                                        DrainClimbCharge(deltaTime);
                                        AddVelocity(-_wallNormal * WallAttractForce * deltaTime);;
                                        _isWallRunning = true;
                                        float currentVelocityMagnitudeDirty = currentVelocity.magnitude + 0.1f;

                                        if (currentVelocityMagnitudeDirty >= WallClimbSpeedFrontVerticalTwoMax)
                                        {
                                            _movementSpeedFloat = 1f;
                                            targetMovementVelocity += Vector3.up * Mathf.Max(wallClimbInputDirectionNormalized.y, 0.8f) * WallClimbSpeedFrontVerticalTwoMax;
                                        }
                                        else if (currentVelocityMagnitudeDirty >= WallClimbSpeedFrontVerticalOneMax) //play boosting -> boosting
                                        {
                                            _movementSpeedFloat = (currentVelocityMagnitudeDirty - WallClimbSpeedFrontVerticalOneMax) / (WallClimbSpeedFrontVerticalTwoMax - WallClimbSpeedFrontVerticalOneMax);
                                            targetMovementVelocity += Vector3.up * Mathf.Max(wallClimbInputDirectionNormalized.y, 0.8f) * WallClimbSpeedFrontVerticalTwoMax;
                                        }
                                        else //Play running -> boosting
                                        {
                                            _movementSpeedFloat = 0f; // we want an extra state here this code is
                                            targetMovementVelocity += Vector3.up * Mathf.Max(wallClimbInputDirectionNormalized.y, 0.8f) * WallClimbSpeedFrontVerticalOneMax;
                                        }
                                        Debug.Log("Target move velo: " + targetMovementVelocity);
                                        targetMovementVelocity *= (1f / (1f + (WallDrag * deltaTime)));

                                    }                                        
                                    else
                                    {
                                        
                                        Debug.Log("KICKING OFF");
                                        //Debug.Break();
                                        //AddVelocity(_wallNormal.normalized * WallRepulseForce * Time.deltaTime);
                                        _isWallJump = true;
                                        gravityVelocity = deltaTime * Gravity;
                                        targetMovementVelocity = currentVelocity + GetAddedAirVelocity(ref currentVelocity, deltaTime);
                                        _isWallRunning = false;
                                    }                                
                                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                                    currentVelocity += gravityVelocity;
                                    

                                    break;
                                }
                            case CharacterWallLookState.Away: //Start sliding off wall
                                {
                                    break;
                                }
                            case CharacterWallLookState.Left:
                            case CharacterWallLookState.Right:
                                {                                    
                                    Vector3 wallRunDirectionNormalized = Vector3.ProjectOnPlane(Motor.CharacterForward, _wallNormal).normalized;
                                    float inputTowardsMovement = Vector3.Dot(_moveInputVector, wallRunDirectionNormalized);


                                    if (CanClimb() && inputTowardsMovement > -0.2f) //If we are above a certain velocity slide on the wall instead of climb. Uncontrollable velocity  
                                    {                                        
                                        
                                        gravityVelocity = Vector3.zero;
                                        DrainClimbCharge(deltaTime);
                                        AddVelocity(_wallNormal * -WallAttractForce * deltaTime);
                                        

                                        float currentVelocityMagnitudeDirty = currentVelocity.magnitude + 0.1f;

                                        if (currentVelocityMagnitudeDirty >= MaxWallSpeedLRTwo)
                                        {
                                            _movementSpeedFloat = 1f;
                                            targetMovementVelocity = wallRunDirectionNormalized * MaxWallSpeedLRTwo;
                                        }
                                        else if (currentVelocityMagnitudeDirty >= MaxWallSpeedLROne) //play boosting -> boosting
                                        {
                                            _movementSpeedFloat = (currentVelocityMagnitudeDirty - MaxWallSpeedLROne) / (MaxWallSpeedLRTwo - MaxWallSpeedLROne); ;
                                            targetMovementVelocity = wallRunDirectionNormalized * MaxWallSpeedLRTwo;
                                        }
                                        else //Play running -> boosting
                                        {
                                            _movementSpeedFloat = 0f; // we want an extra state here this code is
                                            targetMovementVelocity = wallRunDirectionNormalized * MaxWallSpeedLROne;
                                        }
                                        targetMovementVelocity *= (1f / (1f + (WallDrag * deltaTime)));

                                        AnimSetIsWallRunLeftBool(CurrentCharacterWallLookState == CharacterWallLookState.Left);
                                        _isWallRunning = true;
                                    }
                                    else
                                    {

                                        AddVelocity(_wallNormal.normalized * WallRepulseForce * Time.deltaTime);
                                        gravityVelocity = deltaTime * Gravity;
                                        targetMovementVelocity = currentVelocity + GetAddedAirVelocity(ref currentVelocity, deltaTime);
                                        _isWallRunning = false;
                                        _isWallJump = true;
                                    }
                                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                                    currentVelocity += gravityVelocity;

                                    break;
                                }
                        }
                        if (_jumpRequested)
                        {
                            _isWallJump = true;
                        }
                        break;
                    }
            }

            

            Vector3 targetDashMovementVelocity;
            if (_wantsToDash && CanDash())
            {

                if (_isAimingDash)
                {
                    _dashVector = _lookInputNonPlanarVector.normalized * DashForce;
                    //if (currentVelocity.magnitude > 0.1)
                    //{
                    //    _dashVector = currentVelocity.normalized * DashForce;
                    //}
                    //else
                    //{
                    //    _dashVector = Motor.CharacterForward.normalized * DashForce;
                    //}               
                }
                else
                {
                    
                    if (_moveInputVector.magnitude > 0.1)
                    {
                        _dashVector = _moveInputVector.normalized * DashForce;
                    }
                    else
                    {
                        _dashVector = Motor.CharacterForward.normalized * DashForce;
                    }
                    
                }
                _wantsToDash = false;
                _isDashing = true;
                _dashCooldownTimer.ResetTimer();
                _dashSpeedBoostTimer.ResetTimer();
                _dashCountCurrent--;
                SetAnimatorDashTriggerAndDirection(_dashVector, ref currentVelocity);
            }


            if (_isDashing)
            {
                if (_dashSpeedBoostTimer.IsActive())
                {
                    targetDashMovementVelocity = _dashVector;
                    targetDashMovementVelocity *= dashVelocityCurve.Evaluate(_dashSpeedBoostTimer.GetTime());
                }
                else
                {
                    _isDashing = false;
                    _isExitingDash = true;
                    _dashSpeedFreezeTimer.ResetTimer();
                    targetDashMovementVelocity = Vector3.zero;
                }
                currentVelocity = Vector3.Lerp(currentVelocity, targetDashMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
            else if (_isExitingDash)
            {
                if (_dashSpeedFreezeTimer.IsActive())
                {
                    targetDashMovementVelocity = Vector3.zero;
                    currentVelocity = Vector3.Lerp(currentVelocity, targetDashMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                }
                else
                {
                    _isExitingDash = false;
                    _isRunning = true;
                }
                
            }

            // Handle jumping
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested && (_jumpCountCurrent > 0) || _isWallJump)
            {

                if (!_isWallJump)
                {
                    _jumpCountCurrent--;
                }
                _isWallJump = false;
                //See if we actually are allowed to jump _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))

                Vector3 jumpDirection;

                switch (CurrentCharacterGroundedState)
                {
                    case CharacterGroundedState.GroundedStable:
                        {
                            jumpDirection = Motor.GroundingStatus.GroundNormal;
                            break;
                        }
                    case CharacterGroundedState.GroundedUnstable:
                        {
                            jumpDirection = Motor.GroundingStatus.GroundNormal;
                            break;
                        }
                    case CharacterGroundedState.Wall:
                        {

                            jumpDirection = _wallNormal + Motor.CharacterUp;
                            break;
                        }
                    case CharacterGroundedState.Airborne:
                    default:
                        {
                            jumpDirection = Motor.CharacterUp;
                            break;
                        }
                }

                        // Makes the character skip ground probing/snapping on its next update. 
                        // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                        Motor.ForceUnground();
                CharacterGroundedStateTransitionTo(CharacterGroundedState.Airborne);
                SetAnimatorJumpTrigger();
                ResetAnimatorLandStableTrigger();

                // Add to the return velocity and reset jump state
                currentVelocity += (jumpDirection * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                
                _jumpRequested = false;
                _jumpConsumed = true;
                _jumpedThisFrame = true;

                if(_isDashing)
                {
                    _isDashing = false;
                }
                
            }

            if (_wantsToTest)
            {
                AddVelocity(Vector3.forward * WallAttractForce * deltaTime);
            }

            // Take into account additive velocity (add a force)
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }

            _previousVelocity = currentVelocity;
            Debug.Log("Current velo: " + currentVelocity);
            Debug.Log("Exiting grounded state: " + CurrentCharacterGroundedState);
            UpdateAnimatorIsWallRunningBool();
            UpdateAnimatorSpeedFloat();
            SetAnimatorMagnitudeFloat(currentVelocity.magnitude);
            SetAnimatorVelocityVector(ref currentVelocity);
        }

        




        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            if (CurrentCharacterGroundedState == CharacterGroundedState.Wall)
            {
                if (!_wallHitCheckTimer.IsActive()) //Turns out rotation comes before velocity. Need to figure out how to do update this before.
                {
                    
                    Debug.Log("STOPPED TOUCHING WALL");
                    StoppedTouchingWall();
                }
            }
            

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // Handle jump-related values
                        {
                            // Handle jumping pre-ground grace period
                            if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                            {
                                _jumpRequested = false;
                            }

                            if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                            {
                                // If we're on a ground surface, reset jumping values
                                if (!_jumpedThisFrame)
                                {
                                    _jumpConsumed = false;
                                    _jumpCountCurrent = JumpCountMax;
                                }
                                _timeSinceLastAbleToJump = 0f;
                            }
                            else
                            {
                                // Keep track of time since we were last able to jump (for grace period)
                                _timeSinceLastAbleToJump += deltaTime;
                            }

                            if (Motor.GroundingStatus.IsStableOnGround)
                            {
                                _dashCountCurrent = AirDashCountMax;
                            }
                        }

                        // Handle uncrouching
                        if (_isCrouching && !_shouldBeCrouching)
                        {
                            // Do an overlap test with the character's standing height to see if there are any obstructions
                            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                            if (Motor.CharacterOverlap(
                                Motor.TransientPosition,
                                Motor.TransientRotation,
                                _probedColliders,
                                Motor.CollidableLayers,
                                QueryTriggerInteraction.Ignore) > 0)
                            {
                                // If obstructions, just stick to crouching dimensions
                                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                            }
                            else
                            {
                                // If no obstructions, uncrouch
                                MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                                _isCrouching = false;
                            }
                        }
                        break;
                    }
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            {
                CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedStable);
            }
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLeaveStableGround();
            }
            switch (CurrentCharacterGroundedState)
            {
                case CharacterGroundedState.GroundedStable:
                    {
                        if (!Motor.GroundingStatus.IsStableOnGround)
                        {
                            if (Motor.GroundingStatus.FoundAnyGround)
                            {
                                CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedUnstable);
                            }
                            else
                            {
                                CharacterGroundedStateTransitionTo(CharacterGroundedState.Airborne);
                            }
                        }
                        break;
                    }
                case CharacterGroundedState.GroundedUnstable:
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedStable);
                        }
                        else if (!Motor.GroundingStatus.FoundAnyGround)
                        {
                            CharacterGroundedStateTransitionTo(CharacterGroundedState.Airborne);
                        }
                        break;
                    }
                case CharacterGroundedState.Airborne:
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedStable);
                        }
                        else if (Motor.GroundingStatus.FoundAnyGround)
                        {
                            CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedUnstable);
                        }
                        break;
                    }
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0)
            {
                return true;
            }

            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            //if (hitCollider.gameObject.layer == 6) //Wall layer
            //{
            //    if (MathF.Abs(Vector3.Dot(hitNormal.normalized, Motor.CharacterUp)) < 0.2f)
            //    {
            //        _wallHitCheckTimer.ResetTimer();
            //        Debug.Log("Resseting timer");

            //        if (_currentWallCollider != hitCollider)
            //        {
            //            _currentWallCollider = hitCollider;
            //            _wallNormal = hitNormal;
            //            _wallHitPoint = hitNormal;


            //            StartedTouchingWall();
            //        }
            //        else if (_wallNormal != hitNormal)
            //        {
            //            _wallNormal = hitNormal;
            //            _wallHitPoint = hitNormal;
            //        }
            //    }
            //}
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            Debug.Log("movement hit");
            if (hitCollider.gameObject.layer == 6) //Wall layer
            {
                if (MathF.Abs(Vector3.Dot(hitNormal.normalized, Motor.CharacterUp)) < 0.2f) //
                {
                    Debug.Log("Resseting timer");
                    _wallHitCheckTimer.ResetTimer();

                    if (_currentWallCollider != hitCollider)
                    {
                        _currentWallCollider = hitCollider;
                        _wallNormal = hitNormal;
                        _wallHitPoint = hitNormal;

                        //_wallHitCheckTimer.ResetTimer();
                        StartedTouchingWall();
                    }
                    else if (_wallNormal != hitNormal)
                    {
                        _wallNormal = hitNormal;
                        _wallHitPoint = hitNormal;
                    }
                    else
                    {
                    }
                }
            }
        }

        private void StartedTouchingWall()
        {
            EvaluateWallLookState(); //Update CharacterWallLookState
            CharacterGroundedStateTransitionTo(CharacterGroundedState.Wall);
        }

        private void StoppedTouchingWall()
        {
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedStable);
            }
            else if (Motor.GroundingStatus.FoundAnyGround)
            {
                CharacterGroundedStateTransitionTo(CharacterGroundedState.GroundedUnstable);                
            }
            else
            {
                CharacterGroundedStateTransitionTo(CharacterGroundedState.Airborne);
            }            
        }

        public void AddVelocity(Vector3 velocity)
        {
            _internalVelocityAdd += velocity;
            //switch (CurrentCharacterState)
            //{
            //    case CharacterState.Default:
            //        {
            //            _internalVelocityAdd += velocity;
            //            break;
            //        }
            //}
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        protected void OnLanded()
        {
            //TransitionToState(CharacterGroundedState.Grounded);
        }

        #region StateTransitions

        public void CharacterStateTransitionTo(CharacterState newState)
        {
            switch (CurrentCharacterState) //Exiting old(current) state
            {
                default:
                    {
                        //Do nothing
                        break;
                    }
            }
            switch (newState) //This is entering the new state //ADD TRANSITION FOR WALLSTATUS, I'm not sure if this will fuck with wall status exit transition part.
            {
                default:
                    {
                        //Do nothing
                        break;
                    }
            }
            
            //SetAnimatorEnumStateChangeTrigger();
            CurrentCharacterState = newState;
        }

        protected void CharacterGroundedStateTransitionTo(CharacterGroundedState newState)
        {
            if (CurrentCharacterGroundedState != newState)
            {
                switch (CurrentCharacterGroundedState) //Exiting old(current) state
                {
                    case CharacterGroundedState.GroundedStable:
                        {
                            break;
                        }
                    case CharacterGroundedState.Wall:
                        {
                            _isWallJump = false;
                            _currentWallCollider = null;
                            ResetWallLookState();
                            _isWallRunning = false;                            
                            break;
                        }
                    default:
                        {
                            //Do nothing
                            break;
                        }
                }
                switch (newState) //This is entering the new state //ADD TRANSITION FOR WALLSTATUS, I'm not sure if this will fuck with wall status exit transition part.
                {
                    case CharacterGroundedState.Airborne:
                        {
                            //Do nothing
                            ResetCrouching();
                            ResetAnimatorLandStableTrigger();
                            break;
                        }
                    case CharacterGroundedState.GroundedStable:
                        {
                            SetAnimatorLandStableTrigger();
                            _currentClimbCharge = MaxClimbCharge;
                        
                            break;
                        }
                    case CharacterGroundedState.Wall:
                        {
                            _isDashing = false;
                            Motor.ForceUnground();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            
                CurrentCharacterGroundedState = newState;
                UpdateAnimatorGroundedStateEnum();
                SetAnimatorEnumGroundedChangeTrigger();
            }
        }

        private void EvaluateWallLookState() 
        {
            float inputWallNormalDotProduct = Vector3.Dot(-_wallNormal.normalized, Motor.CharacterForward);
            Debug.Log(inputWallNormalDotProduct);
            Vector3 wallRightDirection = Vector3.Cross(Motor.CharacterUp, _wallNormal);
            float inputWallRightDotProduct = Vector3.Dot(wallRightDirection, Motor.CharacterForward);
            
            //float CharForwardWallDotProduct = Vector3.Dot(_wallNormal, Motor.CharacterForward);
            if (0.8f < inputWallNormalDotProduct)
            {
                CurrentCharacterWallLookState = CharacterWallLookState.Forward;
            }
            else if (inputWallNormalDotProduct < -0.8f) //We are faced away from the wall
            {
                CurrentCharacterWallLookState = CharacterWallLookState.Away;
            }
            else if (Mathf.Abs(inputWallNormalDotProduct) <= 0.8f) //Are we looking to the left or right?
            {
                if (0f < inputWallRightDotProduct)
                {
                    //Facing right
                    CurrentCharacterWallLookState = CharacterWallLookState.Right;
                }
                else
                {
                    //Facing left
                    CurrentCharacterWallLookState = CharacterWallLookState.Left;
                }

            }
            else
            {
                CurrentCharacterWallLookState = CharacterWallLookState.NoWall;
            }
            
            if (_previousCharacterWallLookState != CurrentCharacterWallLookState)
            {
                _previousCharacterWallLookState = CurrentCharacterWallLookState;
                UpdateAnimatorWallLookStateEnum();
                SetAnimatorEnumWallLookChangeTrigger();
            }
            
        }

        #endregion


        protected void OnLeaveStableGround()
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        protected Vector3 GetAddedAirVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 addedVelocity;
            if (_moveInputVector.sqrMagnitude > 0f)
            {
                addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime;

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane. Make sure we don't add more than the max amount. -H
                    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity. This make it so you can only input right and left I think -H
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                    {

                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                    }
                }
            }
            else
            {
                addedVelocity = Vector3.zero;
            }
            return addedVelocity;
        }

        protected Vector3 GetJumpDirectionNormalized()
        {
            Vector3 jumpDirection;

            switch (CurrentCharacterGroundedState)
            {
                case CharacterGroundedState.GroundedStable:
                    {
                        jumpDirection = Motor.GroundingStatus.GroundNormal;
                        break;
                    }
                case CharacterGroundedState.GroundedUnstable:
                    {
                        jumpDirection = Motor.GroundingStatus.GroundNormal;
                        break;
                    }
                case CharacterGroundedState.Wall:
                    {
                        
                        jumpDirection = _wallNormal + Motor.CharacterUp;
                        break;
                    }
                case CharacterGroundedState.Airborne:
                default:
                    {
                        jumpDirection = Motor.CharacterUp;
                        break;
                    }
            }

            return jumpDirection.normalized;
        }

        protected Vector3 GetWallRunDirection()
        {
            return Vector3.ProjectOnPlane(Motor.CharacterForward, _wallNormal);
        }

        public float GetClimbTimer()
        {
            return _currentClimbCharge;
        }

        public float GetRunTimer()
        {
            return _currentRunCharge;
        }

        private void DrainClimbCharge(float deltaTime)
        {
            float drainAmount = deltaTime * ClimbDrainRate;
            if (_currentClimbCharge > drainAmount)
            {
                _currentClimbCharge -= drainAmount;
            }
            else
            {
                _currentClimbCharge = 0f;
            }
        }

        private void DrainRunCharge(float deltaTime)
        {
            float drainAmount = deltaTime * RunDrainRate;
            if (_currentRunCharge > drainAmount)
            {
                _currentRunCharge -= drainAmount;
            }
            else
            {
                _currentRunCharge = 0f;
            }
        }

        private void RechargeRunCharge(float deltaTime)
        {
            float rechargeAmount = deltaTime * RunRechargeRate;
            _currentRunCharge += rechargeAmount;
            if (_currentRunCharge > MaxRunCharge)
            {
                _currentRunCharge = MaxRunCharge;
            }
        }

        private bool CanClimb()
        {
            Vector3 previousVelocityOnInputsPlane = Vector3.ProjectOnPlane(_previousVelocity, Motor.CharacterUp);
            if (previousVelocityOnInputsPlane.magnitude > WallClimbMinimumNeededSpeed && (_currentClimbCharge > 0f))
            {
                return true;
            }
            else if (_currentClimbCharge > 0f && _isWallRunning)
            {
                return true;
            }
            return false;
        }

        private bool CanRun()
        {
            if ((_currentRunCharge > 0f) && _moveInputVector.magnitude > 0.1f)
            {
                return true;
            }
            return false;
        }

        private bool CanDash()
        {
            if (!_dashCooldownTimer.IsActive() && _dashCountCurrent > 0)
            {
                _dashCooldownTimer.ResetTimer();
                return true;
            }
            return false;
        }

        public bool DebugGetIsClimbing()
        {
            return _isWallRunning;
        }

        private void UpdateAnimatorIsWallRunningBool()
        {
            _playerAnimationParameterWrapperScript.SetIsWallRunningBool(_isWallRunning);
            _playerAnimationParameterWrapperScript.SetIsWallRunningBool2(_isWallRunning);
        }

        private void AnimSetIsWallRunLeftBool(bool isWallRunLeft)
        {
            _playerAnimationParameterWrapperScript.SetIsWallRunLeftBool(isWallRunLeft);
        }

        private void UpdateAnimatorGroundedStateEnum()
        {
            //int index = (int)CurrentCharacterGroundedState;
            _playerAnimationParameterWrapperScript.SetGroundedStateEnum((int)CurrentCharacterGroundedState);
        }

        private void UpdateAnimatorWallLookStateEnum()
        {            
            _playerAnimationParameterWrapperScript.SetWallLookStateEnum(((int)CurrentCharacterWallLookState));
            _playerAnimationParameterWrapperScript.SetWallLookStateEnum2(((int)CurrentCharacterWallLookState));
        }

        private void SetAnimatorIsRunningBool(bool value)
        {
            _playerAnimationParameterWrapperScript.SetIsRunningBool(value);
        }

        private void SetAnimatorEnumGroundedChangeTrigger()
        {
            _playerAnimationParameterWrapperScript.SetGroundedStateTrigger();
        }

        private void SetAnimatorEnumWallLookChangeTrigger()
        {
            _playerAnimationParameterWrapperScript.SetWallLookStateTrigger();
        }

        private void SetAnimatorJumpTrigger()
        {
            _playerAnimationParameterWrapperScript.SetJumpTrigger();
        }

        private void SetAnimatorLandStableTrigger()
        {
            _playerAnimationParameterWrapperScript.SetLandStableTrigger();
        }

        public void SetAnimatorDashTriggerAndDirection(Vector3 dashDirection, ref Vector3 currentVelocity)
        {
            Vector3 dashDirectionNormalized = dashDirection.normalized;
            float dashX = Vector3.Dot(dashDirectionNormalized, Motor.CharacterRight);
            float dashY = Vector3.Dot(currentVelocity.normalized, Motor.CharacterUp); //Y is different because gravity!
            float dashZ = Vector3.Dot(dashDirectionNormalized, Motor.CharacterForward);
            _playerAnimationParameterWrapperScript.SetDashX(dashX);
            _playerAnimationParameterWrapperScript.SetDashY(dashY);
            _playerAnimationParameterWrapperScript.SetDashZ(dashZ);
            _playerAnimationParameterWrapperScript.SetDashTrigger();

        }

        private void ResetAnimatorLandStableTrigger()
        {
            _playerAnimationParameterWrapperScript.ResetLandStableTrigger();
        }

        private void ResetAnimatorEnumWallLookChangeTrigger()
        {
            _playerAnimationParameterWrapperScript.ResetWallLookStateTrigger();
        }

        private void UpdateAnimatorSpeedFloat()
        {
            _playerAnimationParameterWrapperScript.SetSpeedFloat(_movementSpeedFloat);
        }

        private void SetAnimatorMagnitudeFloat(float magnitude)
        {
            _playerAnimationParameterWrapperScript.SetVelocityMagnitude(magnitude);
        }

        public float GetDashTimer()
        {
            return _dashCooldownTimer.GetTime();
        }
        
        public void ResetJumpCount()
        {
            _jumpCountCurrent = JumpCountMax;
        }

        public void AddJump(int jumps)
        {
            _jumpCountCurrent += jumps;
            if (_jumpCountCurrent > JumpCountMax)
            {
                _jumpCountCurrent = JumpCountMax;
            }
        }

        public void ResetDashCount()
        {
            _dashCountCurrent = AirDashCountMax;
        }

        public void AddDash(int dashes)
        {
            _dashCountCurrent += dashes;
            if (_dashCountCurrent > AirDashCountMax)
            {
                _dashCountCurrent = AirDashCountMax;
            }
        }

        private void SetAnimatorVelocityVector(ref Vector3 currentVelocity)
        {
            Vector3 moveInputNormalized = _moveInputVector.normalized;
            float velocityX = Vector3.Dot(moveInputNormalized, Motor.CharacterRight);
            float velocityY = Vector3.Dot(currentVelocity.normalized, Motor.CharacterUp); //Y is different because gravity!
            float velocityZ = Vector3.Dot(moveInputNormalized, Motor.CharacterForward);
            //Compare max speed values for ground and air

            switch(CurrentCharacterGroundedState)
            {
                case CharacterGroundedState.Airborne:
                    {
                        //velocityX = 0f;
                        //velocityZ = 0f;
                        break;
                    }
                case CharacterGroundedState.GroundedStable:
                    {
                        #region old code
                        //Vector3 currentVelocityOnGround = Vector3.ProjectOnPlane(currentVelocity, Motor.GroundingStatus.GroundNormal);
                        //Vector3 xVelocityVect = Vector3.Project(currentVelocityOnGround, Motor.CharacterRight);
                        //Vector3 yVelocityVect = Vector3.Project(currentVelocityOnGround, Motor.CharacterUp);
                        //Vector3 zVelocityVect = Vector3.Project(currentVelocityOnGround, Motor.CharacterForward);

                        //float currentSpeedOnGround = currentVelocityOnGround.magnitude;
                        //if (currentSpeedOnGround > MaxMoveStableSpeed)
                        //{
                        //    overDrive = true;
                        //}
                        //if (currentSpeedOnGround >= MaxMoveStableSpeed * 0.5f)
                        //{
                        //    isRunning = true;
                        //}
                        //if (currentSpeedOnGround > 0.1f)
                        //{
                        //    velocityX = xVelocityVect.magnitude / MaxMoveWalkSpeed;
                        //    velocityY = yVelocityVect.magnitude / MaxStableFallSpeed;
                        //    velocityZ = zVelocityVect.magnitude / MaxMoveWalkSpeed;

                        //}
                        //else
                        //{
                        //    //Velo is 0
                        //    velocityX = 0f;
                        //    velocityZ = 0f;
                        //}

                        //SetAnimatorIsRunningBool(isRunning);
                        #endregion

                        break;
                    }
            }

            //Set speed multiplier in animator
            _playerAnimationParameterWrapperScript.SetVelocityX(velocityX);
            _playerAnimationParameterWrapperScript.SetVelocityY(velocityY);
            _playerAnimationParameterWrapperScript.SetVelocityZ(velocityZ);
        }
    }
}