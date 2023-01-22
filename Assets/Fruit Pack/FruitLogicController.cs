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
    [SerializeField] Transform _meshTransform;
    [SerializeField] float _rotationSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _meshTransform.Rotate(0, Time.deltaTime * _rotationSpeed, 0);
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
                        charController.ResetJumpCount();
                        break;
                    }
                case FruitType.Strawberry:
                    {
                        Debug.Log("Reset dashes");
                        charController.ResetDashCount();
                        break;
                    }
            }
            this.gameObject.SetActive(false);
        }

    }
}
