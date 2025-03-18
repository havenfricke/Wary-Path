using System;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponItem : MonoBehaviour
{
    private Weapon weapon; // The rolled weapon (this will be set)

    public float baseDamage;
    public float attackSpeedModifier;
    public float damageModifier;
    public string itemRarity;
    public MeshFilter itemModel;
    public string itemId;

    // When weapon GameObject is Instantiated, roll 
    private void Awake()
    {
        // Roll for a weapon from LootManager
        weapon = LootManager.instance.RollWeapon();

        // Apply the rolled weapon data to the current GameObject
        if (weapon != null)
        {
            itemRarity = weapon.itemRarity;
            baseDamage = weapon.baseDamage;
            attackSpeedModifier = weapon.attackSpeedModifier;
            damageModifier = weapon.damageModifier;
            itemId = Guid.NewGuid().ToString();

            Destroy(weapon);
        }
        else
        {
            Debug.LogError("LootManager did not provide a weapon.");
        }
    }
}

