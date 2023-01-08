using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SensorObject : MonoBehaviour
{
    public SensorLogic sensorLogic;
    public abstract void InitializeSensors();

    public abstract void Play();
}
