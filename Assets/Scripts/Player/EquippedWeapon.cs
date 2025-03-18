using Photon.Pun;
using UnityEngine;
using System;

public class EquippedWeapon : MonoBehaviourPun
{
    public GameObject weapon = null; // The networked weapon instance.
    public bool debugEquip = false;
    public GameObject fakeDebugWeapon; // For testing; assumed to have an OwnedWeaponItem component.

    // Store the persistent data used to initialize the equipped weapon.
    private WeaponData currentWeaponData;

    private void Update()
    {
        if (debugEquip)
        {
            WeaponData data = ConvertToWeaponData(fakeDebugWeapon);
            EquipWeapon(data);
            debugEquip = false;
        }
    }

    private WeaponData ConvertToWeaponData(GameObject inventoryWeapon)
    {
        OwnedWeaponItem owned = inventoryWeapon.GetComponent<OwnedWeaponItem>();
        if (owned == null)
        {
            Debug.LogError("Weapon is missing an OwnedWeaponItem component!");
            return null;
        }
        WeaponData data = new WeaponData();
        data.itemId = owned.itemId;
        data.prefabName = owned.gameObject.name.Replace("(Clone)", "").Replace("_Owned", "").Trim();
        data.itemRarity = owned.itemRarity;
        data.baseDamage = owned.baseDamage;
        data.attackSpeedModifier = owned.attackSpeedModifier;
        data.damageModifier = owned.damageModifier;
        return data;
    }

    /// <summary>
    /// Equips a weapon using the persistent data from inventory.
    /// </summary>
    public void EquipWeapon(WeaponData data)
    {
        if (weapon != null)
        {
            PhotonNetwork.Destroy(weapon);
            weapon = null;
        }
        if (data == null)
        {
            Debug.LogError("EquipWeapon received null WeaponData!");
            return;
        }
        Debug.Log("Instantiating networked weapon prefab: " + data.prefabName);
        GameObject spawnedWeapon = null;
        try
        {
            spawnedWeapon = PhotonNetwork.Instantiate(data.prefabName, transform.position, transform.rotation);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to instantiate weapon prefab: " + data.prefabName + ". Exception: " + ex);
            return;
        }
        spawnedWeapon.transform.SetParent(transform);
        spawnedWeapon.transform.localPosition = Vector3.zero;
        spawnedWeapon.transform.localRotation = Quaternion.identity;
        WeaponInstance instance = spawnedWeapon.GetComponent<WeaponInstance>();
        if (instance != null)
        {
            instance.Initialize(data); // This should overwrite any prefab defaults.
        }
        else
        {
            Debug.LogError("Spawned weapon is missing a WeaponInstance component.");
        }
        weapon = spawnedWeapon;
        currentWeaponData = data;
    }

    /// <summary>
    /// Stows the equipped weapon and returns its persistent data.
    /// </summary>
    public WeaponData StowWeapon()
    {
        if (weapon != null)
        {
            WeaponInstance instance = weapon.GetComponent<WeaponInstance>();
            WeaponData dataToReturn = null;
            if (instance != null && instance.weaponData != null)
            {
                dataToReturn = instance.weaponData;
            }
            else
            {
                Debug.LogWarning("Equipped weapon is missing WeaponInstance data; using stored data.");
                dataToReturn = currentWeaponData;
            }
            PhotonNetwork.Destroy(weapon);
            weapon = null;
            return dataToReturn;
        }
        return null;
    }

    // Animator events for collider management.
    public void OpenColldier()
    {
        if (weapon != null)
        {
            BoxCollider collider = weapon.GetComponent<BoxCollider>();
            if (collider != null)
                collider.enabled = true;
            else
                Debug.LogWarning("No BoxCollider found on equipped weapon.");
        }
        else
        {
            Debug.LogWarning("No weapon equipped; cannot open collider.");
        }
    }

    public void CloseColldier()
    {
        if (weapon != null)
        {
            BoxCollider collider = weapon.GetComponent<BoxCollider>();
            if (collider != null)
                collider.enabled = false;
            else
                Debug.LogWarning("No BoxCollider found on equipped weapon.");
        }
        else
        {
            Debug.LogWarning("No weapon equipped; cannot close collider.");
        }
    }
}
