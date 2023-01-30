using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.InputSystem;

namespace KinematicCharacterController
{
    public class HoldenPlayerManager : MonoBehaviour
    {
        [Header("Controller sensitivity")]
        public float xSensitivity = 10f;
        public float ySensitivity = 10f;

        public HoldenCharacterController CharacterController;
        public HoldenCharacterCamera CharacterCamera;

        //private const string MouseXInput = "Mouse X";
        //private const string MouseYInput = "Mouse Y";
        //private const string MouseScrollInput = "Mouse ScrollWheel";
        //private const string HorizontalInput = "Horizontal";
        //private const string VerticalInput = "Vertical";

        //private PlayerMovementInputAction playerMovementIA;
        private PlayerControlMap _playerControlMapping;
        private InputAction _playerMovementIA, _playerAttack1IA, _playerLookIA, _playerJumpIA, _playerCrouchIA, _playerClimbIA, _playerRunIA, _playerDashIA, _playerAimDashIA;

        private void Awake()
        {
            _playerControlMapping = new PlayerControlMap();
        }

        private void OnEnable()
        {
            _playerMovementIA = _playerControlMapping.Player.Move;
            _playerAttack1IA = _playerControlMapping.Player.Attack1;
            _playerLookIA = _playerControlMapping.Player.Look;
            _playerJumpIA = _playerControlMapping.Player.Jump;
            _playerCrouchIA = _playerControlMapping.Player.Crouch;
            _playerClimbIA = _playerControlMapping.Player.Climb;
            _playerRunIA = _playerControlMapping.Player.Run;
            _playerDashIA = _playerControlMapping.Player.Dash;
            _playerAimDashIA = _playerControlMapping.Player.AimDash;

            _playerMovementIA.Enable();
            _playerAttack1IA.Enable();
            _playerLookIA.Enable();
            _playerJumpIA.Enable();
            //_playerCrouchIA.Enable();
            _playerClimbIA.Enable();
            _playerRunIA.Enable();
            _playerDashIA.Enable();
            _playerAimDashIA.Enable();

            //Debug.Log(_playerLookIA.bindings[0].processors);
            string overrideProcessors = "ScaleVector2(x=" + xSensitivity + ",y=" + ySensitivity + ")";
            var binding = _playerLookIA.bindings[0];
            binding.overrideProcessors = overrideProcessors;
            Debug.Log("Processors set to " + binding.overrideProcessors);
            _playerLookIA.ChangeBindingWithGroup("Gamepad").To(binding);
            
        }

        private void OnDisable()
        {
            _playerMovementIA.Disable();
            _playerAttack1IA.Disable();
            _playerLookIA.Disable();
            _playerJumpIA.Disable();
            //_playerCrouchIA.Disable();
            _playerClimbIA.Disable();
            _playerRunIA.Disable();
            _playerDashIA.Disable();
            _playerAimDashIA.Disable();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            CharacterCamera.SetFollowTransform(CharacterController.CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(CharacterController.GetComponentsInChildren<Collider>());

            
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            if (CharacterCamera.RotateWithPhysicsMover && CharacterController.Motor.AttachedRigidbody != null)
            {
                CharacterCamera.PlanarDirection = CharacterController.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
                //CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
                CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, -CharacterController.Gravity).normalized;
            }

            //Character.UpdateCameraFollowPointTransform(Time.deltaTime);
            CharacterCamera.SetCameraPlanarUp(-CharacterController.Gravity.normalized);
            HandleCameraInput();

        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            Vector2 mouseRawInput = GetMouseScreenPosition();
            Vector3 lookInputVector = new Vector3(mouseRawInput.x, mouseRawInput.y, 0f);

            float zoom = 0f; //disabled

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, zoom, lookInputVector);

            // Handle toggling zoom level
            if (Input.GetMouseButtonDown(1))
            {
                CharacterCamera.TargetDistance = CharacterCamera.DefaultDistance; //CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            }
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            ////////////////////////
            //Update mouse screen position.
            
            //Update movement input position.
            Vector2 MoveRawInput = GetRawMovementInput();
            /////////////////////////////


            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = MoveRawInput.y;
            characterInputs.MoveAxisRight = MoveRawInput.x;
            characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
            characterInputs.JumpDown = _playerJumpIA.WasPressedThisFrame();
            characterInputs.JumpUp = _playerJumpIA.WasReleasedThisFrame();
            characterInputs.CrouchPressed = _playerCrouchIA.WasPerformedThisFrame();
            //characterInputs.CrouchUp = _playerCrouchIA.WasReleasedThisFrame();
            characterInputs.Attack1Down = _playerAttack1IA.WasPressedThisFrame();
            characterInputs.Attack1Up = _playerAttack1IA.WasReleasedThisFrame();
            characterInputs.ClimbDown = _playerClimbIA.WasPressedThisFrame();
            characterInputs.ClimbUp = _playerClimbIA.WasReleasedThisFrame();
            characterInputs.RunDown = _playerRunIA.WasPerformedThisFrame();
            characterInputs.RunUp = _playerRunIA.WasReleasedThisFrame();
            characterInputs.DashPressed = _playerDashIA.WasPerformedThisFrame();
            characterInputs.AimDashDown = _playerAimDashIA.WasPressedThisFrame();
            characterInputs.AimDashUp = _playerAimDashIA.WasReleasedThisFrame();
            //characterInputs.DashUp = _playerDashIA.WasReleasedThisFrame();


            // Apply inputs to character
            CharacterController.SetInputs(ref characterInputs);
        }



        private Vector2 GetMouseScreenPosition() => _playerLookIA.ReadValue<Vector2>();

        private Vector2 GetRawMovementInput() => _playerMovementIA.ReadValue<Vector2>();

        //private void OnHeldAttack(InputAction.CallbackContext obj) => _playerAttackScript.AttackHeld();
        //private void OnReleaseAttack(InputAction.CallbackContext obj) => _playerAttackScript.AttackReleased();
        //_playerAttack1IA.performed += OnHeldAttack; //making event
        //_playerAttack1IA.canceled += OnReleaseAttack;
        //_playerAttack1IA.performed -= OnHeldAttack; //making event
        //_playerAttack1IA.canceled -= OnReleaseAttack;
    }
}