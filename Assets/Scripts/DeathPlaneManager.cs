using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

public class DeathPlaneManager : MonoBehaviour
{
    public static RespawnPoint CurrentRespawnPoint;
    private int currentSpawnPointNumber = 0;
    public HoldenPlayerManager PlayerHandlerObject;
    [SerializeField] private KinematicCharacterMotorState _resetMotorState;
    [SerializeField] private RespawnPoint _defaultRespawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        EnterNewRespawnPoint(_defaultRespawnPoint);
        RespawnPoint.OnCheckPointEnter += HandleSpawnPoint;
        _resetMotorState = new KinematicCharacterMotorState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleSpawnPoint(GameObject spawnPointObject)
    {
        RespawnPoint respawnPoint = spawnPointObject.GetComponent<RespawnPoint>();
        if (respawnPoint.GetSpawnOrderedInt() >= currentSpawnPointNumber)
        {
            ExitCurrentRespawnPoint();

            EnterNewRespawnPoint(respawnPoint);
        }
    }

    public void OnTriggerDeathPlane()
    {
        //_resetMotorState = PlayerHandlerObject.CharacterController.Motor.GetState();
        //_resetMotorState.BaseVelocity = Vector3.zero;
        //_resetMotorState.Rotation = Quaternion.identity;
        PlayerHandlerObject.CharacterController.Motor.ApplyState(_resetMotorState);
        PlayerHandlerObject.CharacterController.Motor.SetPosition(CurrentRespawnPoint.GetRespawnPosition());
    }

    public void EnterNewRespawnPoint(RespawnPoint respawnPoint)
    {
        foreach (DeathPlane deathPlane in respawnPoint.LinkedDeathPlanes)
        {
            deathPlane.OnLinkedRespawnPointEnter();
        }
        currentSpawnPointNumber = respawnPoint.GetSpawnOrderedInt();
        CurrentRespawnPoint = respawnPoint;
    }

    public void ExitCurrentRespawnPoint()
    {
        if (CurrentRespawnPoint != null)
        {
            foreach (DeathPlane deathPlane in CurrentRespawnPoint.LinkedDeathPlanes)
            {
                deathPlane.OnLinkedRespawnPointExit();
            }
        }  
    }
}
