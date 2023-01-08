using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletLogic : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //transform.position = transform.position + 
        //Vector3 transformDifference = transform.position - transform.up;
        //transformDifference *= 0.1f;
        transform.position += transform.forward*bulletSpeed;

        //Have bullets listen for events that call the bullets to mvoe towards spcefici points
    }
}
