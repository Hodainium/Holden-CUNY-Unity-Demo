using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/Weapon", order = 1)]

[System.Serializable]
class Weapon : ScriptableObject
{
    //ScriptableObject.Instantiate
    public enum WeaponType
    {
        MeleeFist,
        MeleeSword,
        MeleeSpear,
        Pistol,
        AssualtRifle,
        Sniper
    }

    public enum AmmoType
    {
        None,
        Light,
        Medium,
        Haavy
    }

    [SerializeField] string _name = "NewWeapon";
    [SerializeField] float _fireRate;
    [SerializeField] float _damage;
    [SerializeField] float _clipSize;
    [SerializeField] float _reloadTime;
    [SerializeField] AmmoType _ammoType;
    [SerializeField] WeaponType _weaponType;
    [SerializeField] AnimatorControllerParameter _animationController;
    

}
