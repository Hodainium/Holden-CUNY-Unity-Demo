using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteScaler))]
public class scaleOnPlay : MonoBehaviour
{
    [SerializeField] SpriteScaler spriteScaler;




    // Start is called before the first frame update
    void Start()
    {
        //
        if (!spriteScaler.isActiveAndEnabled)
        {
            spriteScaler.enabled = true;
        }
    }
}
