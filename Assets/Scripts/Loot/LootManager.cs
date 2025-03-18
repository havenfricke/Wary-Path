using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;
    public bool debug = false;

    [Header("GameObject WeaponItem Database")]
    public List<GameObject> weapons;
    [Header("GameObject TalismanItem Database")]
    public List<GameObject> talismans;
    [Header("GameObject OwnedWeaponItem Database")]
    public List<GameObject> ownedWeapons;
    [Header("GameObject OwnedTalismanItem Database")]
    public List<GameObject> ownedTalismans;



    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if(debug)
        {
            RollWeapon();
            RollTalisman();
            
            debug = false;
        }
    }

    public Weapon RollWeapon()
    {
        int rarity = Random.Range(0, 101);
        Weapon weapon = ScriptableObject.CreateInstance<Weapon>();


        if(rarity >= 0 && rarity <= 69)
        {
            weapon.itemRarity = "common";

            weapon.baseDamage = Random.Range(5, 20);

            weapon.attackSpeedModifier = 1;

            weapon.damageModifier = 1;

            Debug.Log($"{weapon.itemRarity}, {weapon.baseDamage}, {weapon.damageModifier}, {weapon.attackSpeedModifier}");

            return weapon;
        }
        else if (rarity >= 70 && rarity <= 94)
        {
            weapon.itemRarity = "magic";

            weapon.baseDamage = Random.Range(10, 30);

            weapon.attackSpeedModifier  = Random.Range(1.01f, 1.03f);

            weapon.damageModifier = Random.Range(1.01f, 1.12f);

            Debug.Log($"{weapon.itemRarity}, {weapon.baseDamage}, {weapon.damageModifier}, {weapon.attackSpeedModifier}");

            return weapon;
        }
        else if (rarity >= 95 && rarity <= 99)
        {
            weapon.itemRarity = "rare";

            weapon.baseDamage = Random.Range(25, 40);

            weapon.attackSpeedModifier = Random.Range(1.12f, 1.2f);

            weapon.damageModifier = Random.Range(1.03f, 1.12f);

            Debug.Log($"{weapon.itemRarity}, {weapon.baseDamage}, {weapon.damageModifier}, {weapon.attackSpeedModifier}");

            return weapon;
        }
        else if (rarity == 100)
        {
            weapon.itemRarity = "unique";

            weapon.baseDamage = Random.Range(40, 50);

            weapon.attackSpeedModifier = Random.Range(1.2f, 1.22f);

            weapon.damageModifier = Random.Range(1.03f, 1.12f);

            Debug.Log($"{weapon.itemRarity}, {weapon.baseDamage}, {weapon.damageModifier}, {weapon.attackSpeedModifier}");

            return weapon;
        }
        else
        {
            return null;
        }
    }

    public Talisman RollTalisman()
    {
        int rarity = Random.Range(0, 101);
        Talisman talisman = ScriptableObject.CreateInstance<Talisman>();

        if (rarity >= 0 && rarity <= 69)
        {
            talisman.itemRarity = "common";

            talisman.damageReduction = Random.Range(1.01f, 1.02f);

            talisman.damageModifier = 1;

            Debug.Log($"{talisman.itemRarity}, {talisman.damageReduction}, {talisman.damageModifier}");

            return talisman;
        }
        else if (rarity >= 70 && rarity <= 94)
        {
            talisman.itemRarity = "magic";

            talisman.damageReduction = Random.Range(1.02f, 1.08f);

            talisman.damageModifier = Random.Range(1.03f, 1.08f);

            Debug.Log($"{talisman.itemRarity}, {talisman.damageReduction}, {talisman.damageModifier}");

            return talisman;
        }
        else if (rarity >= 95 && rarity <= 99)
        {
            talisman.itemRarity = "rare";

            talisman.damageReduction = Random.Range(1.08f, 1.1f);

            talisman.damageModifier = Random.Range(1.03f, 1.08f);

            Debug.Log($"{talisman.itemRarity}, {talisman.damageReduction}, {talisman.damageModifier}");

            return talisman;
        }
        else if (rarity == 100)
        {
            talisman.itemRarity = "unique";

            talisman.damageReduction = Random.Range(1.1f, 1.15f);

            talisman.damageModifier = Random.Range(1.1f, 1.12f);

            Debug.Log($"{talisman.itemRarity}, {talisman.damageReduction}, {talisman.damageModifier}");

            return talisman;
        }
        else
        {
            return null;
        }
    }

}
