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
        Airborne
    }

    public enum CharacterWallLookState
    {
        Default,
        Forward,
        Left,
        Right,
        Away
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
        public bool CrouchDown;
        public bool CrouchUp;
        public bool Attack1Down;
        public bool Attack1Up;
        public bool ClimbDown;
        public bool ClimbUp;
        public bool RunDown;
        public bool RunUp;
        public bool DashDown;
        public bool DashUp;
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
        public float MaxWallRunSpeed = 30f;
        public float WallDrag = 0.3f;
        public float ForcedWallDrag = 0.5f;
        public float WallAttractForce = 0.1f;
        public float WallPushForce = 1f;
        public float WallClimbSpeedMax = 1;
        public float WallClimbMovementSharpness = 15f;
        public float WallInputControlledTractionMax = 0.9f;
        public float WallTraction = 0.5f;
        public float WallInputControlledSpeedMax = 5f;

        [Header("Magnet")]
        public float MaxClimbCharge = 15f;
        public float ClimbDrainRate = 10f;

        [Header("Running")]
        public float MaxRunCharge = 15f;
        public float RunDrainRate = 10f;
        public float RunRechargeRate = 10f;

        [Header("Dashing")]
        public float DashForce = 5f;

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
        private bool _isTouchingWall = false;
        private Timer _wallHitCheckTimer;
        private float _currentClimbCharge = 0f;
        private float _currentRunCharge = 0f;
        private Collider _currentWallCollider = null;
        private Vector3 _moveInputRawXZY;
        private bool _wantsToRun;
        private float _movementSpeedFloat = 0;
        private bool _wantsToDash;

        public int JumpCountDebug { get { return _jumpCountCurrent; } }

        private Vector3 lastInnerNormal = Vector3.zero;
        private Vector3 lastOuterNormal = Vector3.zero;

        

        [SerializeField] PlayerAttackScript _playerAttackScript;
        [SerializeField] PlayerMovementScript _playerMovementScript;
        [SerializeField] AnimationEventWrapperScript _animationEventWrapperScript;
        [SerializeField] AnimationParameterWrapperScript _animationParameterWrapperScript;

        private void Awake()
        {
            // Handle initial state
            CharacterStateTransitionTo(CharacterState.Default);
            GroundStateTransitionTo(CharacterGroundedState.GroundedStable);

            _currentClimbCharge = MaxClimbCharge;
            _currentRunCharge = MaxRunCharge;

            _wallHitCheckTimer = new Timer(Time.fixedDeltaTime);

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
            CurrentCharacterWallLookState = CharacterWallLookState.Default;
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

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // Move and look inputs
                        _moveInputVector = cameraPlanarRotation * moveInputVector;
                        _moveInputRawVector = moveInputVector;
                        _moveInputRawXZY = new Vector3(moveInputVector.x, moveInputVector.z, 0f);

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

                        if (inputs.DashDown)
                        {
                            _wantsToDash = true;
                        }

                        if (inputs.RunUp)
                        {
                            _wantsToDash = false;
                        }

                        // Crouching input
                        if (inputs.CrouchDown)
                        {
                            _shouldBeCrouching = true;

                            if (!_isCrouching)
                            {
                                _isCrouching = true;
                                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                                MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                            }
                        }
                        else if (inputs.CrouchUp)
                        {
                            _shouldBeCrouching = false;
                        }

                        break;
                    }
            }
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
            if (_isTouchingWall)
            {
                Vector3 wallRightDirection = Vector3.Cross(_wallNormal, Motor.CharacterUp);

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
                    currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                }

                Vector3 currentUp = (currentRotation * Vector3.up);
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
                                //case CharacterGroundedState.Airborne: //Rotate towards camera
                                //    {
                                //        velocityOnInputsPlane = Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterUp);
                                //        smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                                //        break;
                                //    }
                                default:
                                    {
                                        velocityOnInputsPlane = Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterUp);
                                        smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, velocityOnInputsPlane, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
                                        break;
                                    }
                            }
                            

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);


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
            switch (CurrentCharacterGroundedState)
            {
                case CharacterGroundedState.GroundedStable:
                    {
                        if (_isTouchingWall)
                        {
                            AddVelocity(_wallNormal * -WallAttractForce * deltaTime);
                        }
                        float currentVelocityMagnitude = currentVelocity.magnitude;

                        Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

                        // Reorient velocity on slope
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                        // Calculate target velocity
                        Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                        //Vector3 targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;
                         //Implement running quickly
                        Vector3 targetMovementVelocity;

                        

                        float currentVelocityMagnitudeDirty = currentVelocity.magnitude + 0.1f; // re-evaluate magnitude

                        if (currentVelocityMagnitudeDirty >= MaxMoveBoostSpeed && CanRun() && _wantsToRun) // play boosting -> boosting
                        {
                            _movementSpeedFloat = 4;
                            targetMovementVelocity = reorientedInput * MaxMoveStableSpeed;
                        }
                        else if ((currentVelocityMagnitudeDirty > MaxMoveRunSpeed && CanRun() && _wantsToRun) || (CanRun() && _wantsToRun)) //play running -> boosting
                        {
                            _movementSpeedFloat = 3 + (currentVelocityMagnitudeDirty - MaxMoveJogSpeed) / (MaxMoveRunSpeed - MaxMoveJogSpeed); // we want an extra state here this code is
                            targetMovementVelocity = reorientedInput * MaxMoveBoostSpeed;
                        }
                        else if (currentVelocityMagnitudeDirty > MaxMoveJogSpeed) //Play jogging -> running
                        {
                            _movementSpeedFloat = 2 + (currentVelocityMagnitudeDirty - MaxMoveJogSpeed) / (MaxMoveRunSpeed - MaxMoveJogSpeed);
                            targetMovementVelocity = reorientedInput * MaxMoveRunSpeed;
                        }
                        else if (currentVelocityMagnitudeDirty > MaxMoveWalkSpeed) //Play walking -> jogging
                        {
                            _movementSpeedFloat = 1 + (currentVelocityMagnitudeDirty - MaxMoveWalkSpeed)/(MaxMoveJogSpeed - MaxMoveWalkSpeed);
                            targetMovementVelocity = reorientedInput * MaxMoveJogSpeed;
                        }
                        else if (_moveInputVector.magnitude > 0f) //Play idle -> walking //Need to check for input or else cant move
                        {
                            _movementSpeedFloat = currentVelocityMagnitudeDirty/MaxMoveWalkSpeed;
                            targetMovementVelocity = reorientedInput * MaxMoveJogSpeed;
                        }
                        else //Play idle
                        {
                            _movementSpeedFloat = 0f;
                            targetMovementVelocity = Vector3.zero;
                        }

                        if (_wantsToDash)
                        {
                            Vector3 dashDirection;
                            if (_moveInputVector.magnitude > 0)
                            {
                                dashDirection = _moveInputVector.normalized * DashForce;
                            }
                            else
                            {
                                dashDirection = Motor.CharacterForward.normalized * DashForce;
                            }
                            AddVelocity(dashDirection);
                            SetAnimatorDashTriggerAndDirection(dashDirection);
                        }

                        if (_wantsToRun && CanRun())
                        {
                            DrainRunCharge(deltaTime);
                            
                        }
                        else
                        {
                            RechargeRunCharge(deltaTime);
                            
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
                        Vector3 gravityVelocity = Vector3.zero;

                        //HandleWallTransitionAndVelocity(deltaTime); //I don't think we even need this. The physics engine should handle transitional velocity already. We should just be sure to convert it initially and just add to current velocity
                        //if (_hasTransitionalVelocity)
                        //{
                        //    AddVelocity(_transitionalVelocity);
                        //    Debug.Log(_transitionalVelocity);
                        //}

                        if (_isTouchingWall)
                        {
                            Vector3 addedWallRunVelocity = Vector3.zero;
                            Vector3 wallRunBoostAdditive = Vector3.zero;
                            Vector3 wallClimbAdditive = Vector3.zero;
                            switch (CurrentCharacterWallLookState)
                            {
                                case CharacterWallLookState.Forward: //Start climbing wall
                                    {
                                        float inputAgainstWall = Vector3.Dot(_moveInputVector, -_wallNormal);
                                        
                                        if (_wantsToClimb && CanClimb()) //If we are above a certain velocity slide on the wall instead of climb. Uncontrollable velocity
                                        {
                                            Vector3 wallClimbDirectionNormalized;
                                            DrainClimbCharge(deltaTime);

                                            currentVelocity *= (1f / (1f + (WallDrag * deltaTime))); //Experimental

                                            if (_moveInputRawXZY.sqrMagnitude > 0f)
                                            {
                                                //if (inputAgainstWall >= 0f) 

                                                wallClimbDirectionNormalized = Vector3.ProjectOnPlane(_moveInputRawXZY, _wallNormal).normalized;

                                                gravityVelocity = Vector3.zero;
                                            }
                                            else
                                            {
                                                wallClimbDirectionNormalized = Vector3.zero;
                                                gravityVelocity = 0.05f * deltaTime * Gravity;
                                            }

                                            wallClimbAdditive = wallClimbDirectionNormalized * WallClimbSpeedMax;


                                            currentVelocity += wallClimbAdditive;
                                            currentVelocity += gravityVelocity;
                                            //    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-WallClimbMovementSharpness * deltaTime));
                                            //}
                                            //else
                                            //{
                                            //    currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, 1f - Mathf.Exp(-WallClimbMovementSharpness * deltaTime));
                                            //}

                                        }
                                        else
                                        {
                                            //We need to add input controls here
                                            if (inputAgainstWall >= 0f) //More input towards wall results in more resistance
                                            {
                                                gravityVelocity = (1f - (inputAgainstWall*WallInputControlledTractionMax)) * deltaTime * Gravity; //The higher input against wall, the closer () is to 0 * gravity
                                            }
                                            else
                                            {
                                                gravityVelocity = Gravity * deltaTime;
                                            }
                                            //Do sliding off wall

                                            currentVelocity += GetAddedAirVelocity(ref currentVelocity, deltaTime);
                                            currentVelocity += gravityVelocity;

                                        }
                                        //addedWallVelocity = GetAddedWallRunVelocity(ref currentVelocity, deltaTime); //Need to change moveinput so that it takes into account x input- Holden

                                        AddVelocity(_wallNormal * -WallAttractForce * deltaTime);

                                        break;
                                    }
                                case CharacterWallLookState.Away: //Start sliding off wall
                                    {
                                        break;
                                    }
                                case CharacterWallLookState.Right:
                                case CharacterWallLookState.Left:
                                    {
                                        float wallDrag = 0f;
                                        Vector3 wallRunDirectionNormalized = Vector3.ProjectOnPlane(Motor.CharacterForward, _wallNormal).normalized;                                        
                                        float inputTowardsMovement = Vector3.Dot(wallRunDirectionNormalized, _moveInputVector); //Between 1 and -1
                                        Vector3 addedInputVector;
                                        float inputAgainstWall = Vector3.Dot(_moveInputVector, -_wallNormal);

                                        if (_wantsToClimb && CanClimb())
                                        {
                                            DrainClimbCharge(deltaTime);

                                            wallDrag = (1f / (1f + (ForcedWallDrag * deltaTime)));
                                            gravityVelocity = 0.05f * deltaTime * Gravity;

                                        }//Next time we'll implement magnet controls for wall running
                                        else // I want to add a little boost to forward momentum if we're running along a wall even without magnet. -H
                                        {

                                            if (inputAgainstWall > 0f) //More input towards wall results in more resistance
                                            {
                                                gravityVelocity = (1f - (inputAgainstWall * WallInputControlledTractionMax)) * deltaTime * Gravity; //The higher input against wall, the closer () is to 0 * gravity
                                            }
                                            else
                                            {
                                                gravityVelocity = Gravity * deltaTime;
                                            }
                                            //Do sliding off wall

                                            if (inputTowardsMovement > 0f)
                                            {
                                                wallRunBoostAdditive = wallRunDirectionNormalized * WallInputControlledSpeedMax * (inputTowardsMovement/1f) * deltaTime;
                                            }
                                            else
                                            {
                                                wallRunBoostAdditive = Vector3.zero;
                                            }


                                            //currentVelocity += GetAddedAirVelocity(ref currentVelocity, deltaTime); //This is causing issues. Reverse gravity thing ////////////////
                                            

                                            if (_moveInputVector.sqrMagnitude > 0f)
                                            {
                                                //addedInputVector = wallRunDirectionNormalized * inputTowardsMovement * deltaTime;
                                                addedWallRunVelocity = wallRunDirectionNormalized * WallRunAccelerationSpeed * deltaTime;
                                                addedWallRunVelocity += wallRunBoostAdditive;

                                                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                                                // Limit air velocity from inputs
                                                if (currentVelocityOnInputsPlane.magnitude < MaxWallRunSpeed)
                                                {
                                                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                                                    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedWallRunVelocity, MaxWallRunSpeed); //This returns a negative vector we can't allow that -H
                                                    addedWallRunVelocity = newTotal - currentVelocityOnInputsPlane;

                                                    if (Vector3.Dot(newTotal, wallRunDirectionNormalized) < -0.7f)
                                                    {
                                                        //AddVelocity(_wallNormal * WallPushForce * deltaTime);
                                                    }
                                                }
                                                else //This is the air strafing stuff
                                                {
                                                    //// Make sure added vel doesn't go in the direction of the already-exceeding velocity. This make it so you can only input right and left I think -H
                                                    //if (Vector3.Dot(currentVelocityOnInputsPlane, addedWallRunVelocity) > 0f)
                                                    //{
                                                    //    addedWallRunVelocity = Vector3.ProjectOnPlane(addedWallRunVelocity, currentVelocityOnInputsPlane.normalized);
                                                    //} //Pointless. Need to add left and right

                                                    //if (Vector3.Dot(currentVelocity+addedWallRunVelocity, wallRunDirectionNormalized) < -0.7f)
                                                    //{
                                                    //    AddVelocity(_wallNormal * WallPushForce * deltaTime);
                                                    //}
                                                }
                                            }
                                            else
                                            {
                                                //What do we do if no inputs.
                                            }

                                            wallDrag = (1f / (1f + (WallDrag * deltaTime)));
                                        }

                                        currentVelocity += addedWallRunVelocity;
                                        currentVelocity *= wallDrag;

                                        AddVelocity(_wallNormal * -WallAttractForce * deltaTime);

                                        

                                        break;
                                    }                               
                            }//End of switch

                            
                            // Drag
                            

                            
                            //currentVelocity += wallRunBoostAdditive;
                            currentVelocity += gravityVelocity;


                        }
                        else
                        {
                            // Apply added velocity
                            currentVelocity += GetAddedAirVelocity(ref currentVelocity, deltaTime);

                            // Gravity
                            currentVelocity += Gravity * deltaTime;

                            // Drag
                            currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                        }
                        break;
                    }
                

            }

                // Handle jumping
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested && (_jumpCountCurrent > 0))
            {
                // See if we actually are allowed to jump _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))

                Vector3 jumpDirection = GetJumpDirection();
                
                // Makes the character skip ground probing/snapping on its next update. 
                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                Motor.ForceUnground();
                GroundStateTransitionTo(CharacterGroundedState.Airborne);
                SetAnimatorJumpTrigger();
                ResetAnimatorLandStableTrigger();

                // Add to the return velocity and reset jump state
                //currentVelocity = Quaternion.LookRotation(_moveInputVector, Motor.CharacterUp) * currentVelocity;
                currentVelocity += (jumpDirection * JumpUpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                //currentVelocity += (_moveInputVector * JumpForwardMomentumScalar);
                _jumpCountCurrent--;
                _jumpRequested = false;
                _jumpConsumed = true;
                _jumpedThisFrame = true;
                


            }

            if (_attack1Requested)
            {
                bool canAttack = true;

                if (canAttack)
                {

                }
            }

            // Take into account additive velocity (add a force)
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }

            _previousVelocity = currentVelocity;


            UpdateAnimatorIsClimbingBool();
            UpdateAnimatorIsTouchingWallBool();
            UpdateAnimatorGroundedStateEnum();
            UpdateAnimatorWallLookStateEnum();
            UpdateAnimatorSpeedFloat();
            SetAnimatorVelocityVector(ref currentVelocity);
        }

        




        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            if (!_wallHitCheckTimer.IsActive()) //Turns out rotation comes before velocity. Need to figure out how to do update this before.
            {
                StoppedTouchingWall();
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
                GroundStateTransitionTo(CharacterGroundedState.GroundedStable);
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
                                GroundStateTransitionTo(CharacterGroundedState.GroundedUnstable);
                            }
                            else
                            {
                                GroundStateTransitionTo(CharacterGroundedState.Airborne);
                            }
                        }
                        break;
                    }
                case CharacterGroundedState.GroundedUnstable:
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            GroundStateTransitionTo(CharacterGroundedState.GroundedStable);
                        }
                        else if (!Motor.GroundingStatus.FoundAnyGround)
                        {
                            GroundStateTransitionTo(CharacterGroundedState.Airborne);
                        }
                        break;
                    }
                case CharacterGroundedState.Airborne:
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            GroundStateTransitionTo(CharacterGroundedState.GroundedStable);
                        }
                        else if (Motor.GroundingStatus.FoundAnyGround)
                        {
                            GroundStateTransitionTo(CharacterGroundedState.GroundedUnstable);
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
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            _currentWallCollider = hitCollider;
            if (_currentWallCollider.gameObject.layer == 6) //Wall layer
            {
                _wallNormal = hitNormal;

                _wallHitCheckTimer.ResetTimer();
                StartedTouchingWall();
            }

            //This is where we will handle wall angle transitions. This is also where we may handle enum transitions.
        }

        private void StartedTouchingWall()
        {
            _isTouchingWall = true;
            _isWallTransition = true;
            EvalaluateWallLookState(); //Update CharacterWallLookState
        }

        private void StoppedTouchingWall()
        {
            _isTouchingWall = false;
            _isWallTransition = false;
            ResetWallLookState();
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
            SetAnimatorEnumStateChangeTrigger();
            CurrentCharacterState = newState;
        }

        protected void GroundStateTransitionTo(CharacterGroundedState newState)
        {

            switch (CurrentCharacterGroundedState) //Exiting old(current) state
            {
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
                        ResetAnimatorLandStableTrigger();
                        break;
                    }
                case CharacterGroundedState.GroundedStable:
                    {
                        SetAnimatorLandStableTrigger();
                        _currentClimbCharge = MaxClimbCharge;
                        
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            SetAnimatorEnumStateChangeTrigger();
            CurrentCharacterGroundedState = newState;
        }

        private void EvalaluateWallLookState() 
        {
            float inputWallNormalDotProduct = Vector3.Dot(-_wallNormal.normalized, Motor.CharacterForward);
            Vector3 wallRightDirection = Vector3.Cross(_wallNormal, Motor.CharacterUp);
            float inputWallRightDotProduct = Vector3.Dot(wallRightDirection, Motor.CharacterForward);
            //float CharForwardWallDotProduct = Vector3.Dot(_wallNormal, Motor.CharacterForward);
            if (0.8f < inputWallNormalDotProduct)
            {
                CurrentCharacterWallLookState = CharacterWallLookState.Forward;
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
            else if (inputWallNormalDotProduct < -0.8f) //We are faced away from the wall
            {
                CurrentCharacterWallLookState = CharacterWallLookState.Away;
            }
            else
            {
                Debug.Log("No state reached. Returning default.");
                CurrentCharacterWallLookState = CharacterWallLookState.Default;
            }
            SetAnimatorEnumStateChangeTrigger();
        }

        #endregion


        protected void OnLeaveStableGround()
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        protected Vector3 GetAddedWallRunVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
             
            Vector3 addedWallRunVelocity = Vector3.zero;
            Vector3 wallRunDirection = Vector3.ProjectOnPlane(Motor.CharacterForward, _wallNormal).normalized;
            float inputTowardsMovement = Vector3.Dot(wallRunDirection, _moveInputVector); //Between 1 and -1
            Vector3 addedInputVector;
            if (_moveInputVector.sqrMagnitude > 0f) 
            {
                addedInputVector = wallRunDirection * inputTowardsMovement * deltaTime;
                addedWallRunVelocity = wallRunDirection * WallRunAccelerationSpeed * deltaTime;

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < MaxWallRunSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedWallRunVelocity, MaxWallRunSpeed);
                    addedWallRunVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity. > is wrong way, this is makign sure it is facing exceeding velo.
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedWallRunVelocity) < 0f)
                    {

                        addedWallRunVelocity = Vector3.ProjectOnPlane(addedWallRunVelocity, currentVelocityOnInputsPlane.normalized);
                    }
                }
            }
            return addedWallRunVelocity;
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

        protected Vector3 GetAddedWallRunVelocityOld(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 addedVelocity;
            if (_moveInputVector.sqrMagnitude > 0f)
            {
                addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime * (WallInputControlledTractionMax);

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

        protected Vector3 GetJumpDirection()
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
                case CharacterGroundedState.Airborne:
                default:
                    {
                        jumpDirection = Motor.CharacterUp;
                        break;
                    }
            }

            return jumpDirection;
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

        public bool IsTouchingWall()
        {
            return _isTouchingWall;
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
            if (_isTouchingWall && (_currentClimbCharge > 0f))
            {
                return true;
            }
            return false;
        }

        private bool CanRun()
        {
            if ((_currentRunCharge > 0f))
            {
                return true;
            }
            return false;
        }

        private void UpdateAnimatorIsClimbingBool()
        {
            _animationParameterWrapperScript.SetIsClimbingBool(_wantsToClimb && CanClimb());
        }

        private void UpdateAnimatorIsTouchingWallBool()
        {
            _animationParameterWrapperScript.SetIsTouchingWallBool(_isTouchingWall);
        }

        private void UpdateAnimatorGroundedStateEnum()
        {
            //int index = (int)CurrentCharacterGroundedState;
            _animationParameterWrapperScript.SetGroundedStateEnum((int)CurrentCharacterGroundedState);
        }

        private void UpdateAnimatorWallLookStateEnum()
        {            
            _animationParameterWrapperScript.SetWallLookStateEnum(((int)CurrentCharacterWallLookState));
        }

        private void SetAnimatorIsRunningBool(bool value)
        {
            _animationParameterWrapperScript.SetIsRunningBool(value);
        }

        private void SetAnimatorEnumStateChangeTrigger()
        {
            _animationParameterWrapperScript.SetEnumStateTrigger();
        }

        private void SetAnimatorJumpTrigger()
        {
            _animationParameterWrapperScript.SetJumpTrigger();
        }

        private void SetAnimatorLandStableTrigger()
        {
            _animationParameterWrapperScript.SetLandStableTrigger();
        }

        public void SetAnimatorDashTriggerAndDirection(Vector3 dashDirection)
        {
            _animationParameterWrapperScript.SetDashTrigger();
            _animationParameterWrapperScript.SetDashX(dashDirection.x);
            _animationParameterWrapperScript.SetDashY(dashDirection.y);
            _animationParameterWrapperScript.SetDashZ(dashDirection.z);
        }

        private void ResetAnimatorLandStableTrigger()
        {
            _animationParameterWrapperScript.ResetLandStableTrigger();
        }

        private void UpdateAnimatorSpeedFloat()
        {
            _animationParameterWrapperScript.SetSpeedFloat(_movementSpeedFloat);
        }

        public bool isRTPushed()
        {
            return false;
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
            _animationParameterWrapperScript.SetVelocityX(velocityX);
            _animationParameterWrapperScript.SetVelocityY(velocityY);
            _animationParameterWrapperScript.SetVelocityZ(velocityZ);
        }
    }
}