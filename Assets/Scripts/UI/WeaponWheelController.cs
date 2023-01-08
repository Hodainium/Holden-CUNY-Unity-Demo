using UnityEngine;
using System;

class WeaponWheelController : MonoBehaviour
{
    //implement rotate function which is in the movement script already. Just take the angle of that and point towards thing
    [System.Serializable]
    public enum WeaponWheelCount
    {
        Five = 5
    }

    [SerializeField] static WeaponWheelCount _buttonCount;

    [SerializeField] Weapon[] weaponArray = new Weapon[0];

    

    //[SerializeField] List<>

    private void Awake()
    {
        switch(_buttonCount)
        {
            case WeaponWheelCount.Five:
            {
                
                break;
            }
            default:
            {
                break;
            }
        }
    }

    [System.Serializable]
    public struct WeaponWheelButtonStats
    {

    }
}

