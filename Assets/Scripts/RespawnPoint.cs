using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] int spawnPointOrderedNumber = 0;
    public List<DeathPlane> LinkedDeathPlanes;
    private BoxCollider _triggerCollider;
    public static event Action<GameObject> OnCheckPointEnter;

    // Start is called before the first frame update
    void Start()
    {
        _triggerCollider = this.gameObject.GetComponent<BoxCollider>();
        if (spawnPointOrderedNumber == 0)
        {
            OnCheckPointEnter?.Invoke(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == 3)
        {
            _triggerCollider.enabled = false;
            OnCheckPointEnter?.Invoke(this.gameObject);
        }
    }

    public int GetSpawnOrderedInt()
    {
        return spawnPointOrderedNumber;
    }

    public Vector3 GetRespawnPosition()
    {
        return this.transform.position;
    }
}
