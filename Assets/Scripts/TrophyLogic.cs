using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophyLogic : MonoBehaviour
{
    [SerializeField] float _rotationSpeed = 0f;
    [SerializeField] ParticleSystem confettiParticles;
    [SerializeField] GameObject _meshObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0f, Time.deltaTime * _rotationSpeed, 0f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.layer == 3)
        {
            confettiParticles.Play();
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Ended");
        _meshObject.SetActive(false);
    }
}