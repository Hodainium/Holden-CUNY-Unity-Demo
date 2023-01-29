using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController.Examples;

public class FruitLogicController : MonoBehaviour
{
    public enum FruitType
    {
        Apple,
        Pear,
        Strawberry
    }

    [SerializeField] FruitType _fruitType;
    //[SerializeField] Transform _rootTransform;
    [SerializeField] float _rotationSpeed = 1f;
    [SerializeField] float _respawnTime = 1f;
    private Timer _respawnTimer;
    private bool _isOnCooldown = false;

    // Start is called before the first frame update
    void Start()
    {
        _respawnTimer = new Timer(_respawnTime);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0f, Time.deltaTime * _rotationSpeed, 0f);        
    }

    private void FixedUpdate()
    {
        if (_isOnCooldown)
        {
            if(!_respawnTimer.IsActive())
            {
                EndFruitCooldown();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HoldenExampleCharacterController charController = other.GetComponent<HoldenExampleCharacterController>();

        if (charController)
        {
            switch(_fruitType)
            {
                case FruitType.Apple:
                    {
                        charController.AddExtraJump();
                        break;
                    }
                case FruitType.Strawberry:
                    {
                        //Debug.Log("Added dash");
                        charController.AddExtraDash();
                        break;
                    }
            }
            StartFruitCooldown();
        }

    }

    private void StartFruitCooldown()
    {
        DisableFruit();
        _respawnTimer.ResetTimer();
        _isOnCooldown = true;
    }

    private void EndFruitCooldown()
    {
        _isOnCooldown = false;
        EnableFruit();
    }

    private void EnableFruit()
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
    }

    private void DisableFruit()
    {
        this.transform.GetChild(0).gameObject.SetActive(false);
    }
}
