using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public float baseDamage = 0;
    public float attackSpeedModifier = 0;
    public float damageModifier = 0;
    public string itemRarity = null;
}
