using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

class PlayerControlScript : MonoBehaviour //Weird animation issue with topleft or right. Dolphin walk. Next up do running and jumping. Then weapons.
{
    //private PlayerMovementInputAction playerMovementIA;
    //private InputAction playerMovement;
    //[SerializeField] PlayerAttackScript _playerAttackScript;
    //[SerializeField] PlayerMovementScript _playerMovementScript;

    //private void Awake()
    //{
    //    playerMovementIA = new PlayerMovementInputAction();
    //}

    //private void OnEnable()
    //{
    //    playerMovement = playerMovementIA.player.movement;

    //    playerMovementIA.player.movement.Enable();
    //    playerMovementIA.player.shoot.Enable();
    //    playerMovementIA.player.look.Enable();

    //    playerMovementIA.player.shoot.performed += OnHeldAttack; //making event
    //    playerMovementIA.player.shoot.canceled += OnReleaseAttack;
    //}

    //private void OnDisable()
    //{
    //    playerMovementIA.player.movement.Disable();
    //    playerMovementIA.player.shoot.Disable();
    //    playerMovementIA.player.look.Disable();

    //    playerMovementIA.player.shoot.performed -= OnHeldAttack; //making event
    //    playerMovementIA.player.shoot.canceled -= OnReleaseAttack;
    //}

    //private void OnHeldAttack(InputAction.CallbackContext obj) => _playerAttackScript.AttackHeld();

    //private void OnReleaseAttack(InputAction.CallbackContext obj) => _playerAttackScript.AttackReleased();

    //private Vector2 GetMouseScreenPosition() => playerMovementIA.player.look.ReadValue<Vector2>();

    //private Vector2 GetRawMovementInput() => playerMovement.ReadValue<Vector2>();

    //// Update reads controller inputs
    //void Update()
    //{
    //    //Update mouse screen position.
    //    _playerMovementScript.HandleMouseScreenPosition(GetMouseScreenPosition());
    //    //Update movement input position.
    //    _playerMovementScript.HandleMovementInput(GetRawMovementInput());
    //}    
}

