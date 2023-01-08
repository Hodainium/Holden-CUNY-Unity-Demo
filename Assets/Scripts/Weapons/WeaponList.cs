using UnityEngine;

[CreateAssetMenu(fileName = "WeaponListData", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class WeaponList : ScriptableObject
{
    private Weapon[] weaponList;
}