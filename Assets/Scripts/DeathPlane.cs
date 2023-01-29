using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController.Examples;
using System;

public class DeathPlane : MonoBehaviour
{
    [SerializeField] DeathPlaneManager deathPlaneManager;
    [SerializeField] private int _subscribedRespawnPoints = 0;
    [SerializeField] private Collider deathPlaneCollider;
    public bool IsDeathPlaneActive { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        //deathPlaneCollider = this.gameObject.GetComponent<Collider>();
        Debug.Log("Found coll? " + (deathPlaneCollider != null));
        if (_subscribedRespawnPoints <= 0)
        {
            DisableDeathPlane();
        }
        else
        {
            EnableDeathPlane();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLinkedRespawnPointEnter()
    {
        _subscribedRespawnPoints++;
        if (_subscribedRespawnPoints > 0)
        {
            EnableDeathPlane();
        }
    }

    public void OnLinkedRespawnPointExit()
    {
        _subscribedRespawnPoints--;
        if (_subscribedRespawnPoints <= 0)
        {
            _subscribedRespawnPoints = 0;
            DisableDeathPlane();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 3)
        {
            deathPlaneManager.OnTriggerDeathPlane();
        }
    }

    private void EnableDeathPlane()
    {
        //Debug.Log("Found coll2? " + (deathPlaneCollider != null));
        IsDeathPlaneActive = true;
        deathPlaneCollider.enabled = true;
        //DEBUG
        //Debug.LogWarning("RENDERING DEATH PLANES");
        //Renderer renderer = this.gameObject.GetComponent<Renderer>();
        //if (!(renderer == null))
        //{
        //    renderer.enabled = true;
        //}
    }

    private void DisableDeathPlane()
    {
        IsDeathPlaneActive = false;
        deathPlaneCollider.enabled = false;
        //DEBUG
        //Renderer renderer = this.gameObject.GetComponent<Renderer>();
        //if (!(renderer == null))
        //{
        //    renderer.enabled = false;
        //}
    }
}
