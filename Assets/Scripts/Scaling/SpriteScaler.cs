using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteScaler : MonoBehaviour
{
    Vector3 _unScaledObjectSize;
    Vector3 _scaledObjectSize;
    Vector3 _scaleFactors;
    [SerializeField] float xScale = 1;
    [SerializeField] float yScale = 1;
    [SerializeField] float zScale = 1;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        _scaleFactors.Set(xScale, yScale, zScale);

        _unScaledObjectSize = this.transform.localScale;
        _scaledObjectSize = Vector3.Scale(_unScaledObjectSize, _scaleFactors);
        this.transform.localScale = _scaledObjectSize;
    }

    private void OnDisable()
    {
        this.transform.localScale = _unScaledObjectSize;
    }
}
