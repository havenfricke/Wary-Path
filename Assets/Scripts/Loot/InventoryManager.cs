using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System;

public class InventoryManager : MonoBehaviour
{
    private PhotonView photonView;

    [Header("Lootables in Range")]
    public List<WeaponItem> lootableWeaponsInRange;
    public List<TalismanItem> lootableTalismansInRange;

    [Header("Inventory (Owned Items)")]
    public List<WeaponData> weaponInventory;

    [Header("Detection Settings")]
    public float detectionRadius = 0.2f;

    // Event that is triggered whenever the inventory is updated.
    public event Action OnInventoryUpdated;

    private void Awake()
    {
        lootableWeaponsInRange = new List<WeaponItem>();
        lootableTalismansInRange = new List<TalismanItem>();
        weaponInventory = new List<WeaponData>();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // Add a starting weapon for testing if the inventory is empty.
        if (weaponInventory.Count == 0)
        {
            WeaponData startingWeapon = new WeaponData();
            startingWeapon.itemId = "starting_weapon";
            // Change the prefab name to match the one in your Resources folder.
            startingWeapon.prefabName = "Weapon_Owned";
            startingWeapon.itemRarity = "Common";
            startingWeapon.baseDamage = 10f;
            startingWeapon.attackSpeedModifier = 1.0f;
            startingWeapon.damageModifier = 1.0f;

            weaponInventory.Add(startingWeapon);
            OnInventoryUpdated?.Invoke();
            Debug.Log("Added starting weapon to inventory.");
        }
    }


    private void Update()
    {
        DetectLootable();
    }

    public void DetectLootable()
    {
        // Clear previous lists.
        lootableWeaponsInRange.Clear();
        lootableTalismansInRange.Clear();

        // Find nearby colliders.
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<WeaponItem>(out WeaponItem weapon))
            {
                lootableWeaponsInRange.Add(weapon);
            }
            else if (col.TryGetComponent<TalismanItem>(out TalismanItem talisman))
            {
                lootableTalismansInRange.Add(talisman);
            }
        }
    }



    public void AddLootableToInventory()
    {
        Debug.Log("Attempting to add item to inventory");
        GameObject closestLootable = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        // Check weapons.
        foreach (WeaponItem weapon in lootableWeaponsInRange)
        {
            float distance = Vector3.Distance(currentPos, weapon.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestLootable = weapon.gameObject;
            }
        }

        // Check talismans.
        foreach (TalismanItem talisman in lootableTalismansInRange)
        {
            float distance = Vector3.Distance(currentPos, talisman.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestLootable = talisman.gameObject;
            }
        }

        if (closestLootable != null)
        {
            // WEAPON PICKUP
            if (closestLootable.TryGetComponent<WeaponItem>(out WeaponItem dropWeapon))
            {
                string baseWeaponName = dropWeapon.name.Replace("(Clone)", string.Empty).Trim();
                WeaponData newWeaponData = new WeaponData();

                // Ensure a unique ID if needed.
                if (string.IsNullOrEmpty(dropWeapon.itemId) || weaponInventory.Exists(w => w.itemId == dropWeapon.itemId))
                {
                    newWeaponData.itemId = Guid.NewGuid().ToString();
                }
                else
                {
                    newWeaponData.itemId = dropWeapon.itemId;
                }

                // Use the convention: inventory prefab names are base name + "_Owned"
                newWeaponData.prefabName = baseWeaponName + "_Owned";
                newWeaponData.itemRarity = dropWeapon.itemRarity;
                newWeaponData.baseDamage = dropWeapon.baseDamage;
                newWeaponData.attackSpeedModifier = dropWeapon.attackSpeedModifier;
                newWeaponData.damageModifier = dropWeapon.damageModifier;

                weaponInventory.Add(newWeaponData);
                RequestDestroyLoot(closestLootable);
            }

            if (closestLootable.TryGetComponent<TalismanItem>(out TalismanItem dropTalisman))
            {
                Debug.LogWarning("Talisman pickup not implemented in the new system.");
            }
            OnInventoryUpdated?.Invoke();
        }
    }


private void RequestDestroyLoot(GameObject loot)
    {
        PhotonView lootPV = loot.GetComponent<PhotonView>();
        if (lootPV == null)
        {
            Debug.LogWarning("Loot object does not have a PhotonView.");
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(loot);
        }
        else
        {
            photonView.RPC("RPC_RequestDestroy", RpcTarget.MasterClient, lootPV.ViewID);
        }
    }

    public void DestroyLootableFromInventoryById(string itemId)
    {
        WeaponData found = weaponInventory.Find(data => data.itemId == itemId);
        if (found != null)
        {
            weaponInventory.Remove(found);
        }
    }

    [PunRPC]
    public void RPC_RequestDestroy(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            PhotonNetwork.Destroy(targetView);
        }
        else
        {
            Debug.LogWarning("Unable to find PhotonView with viewID: " + viewID);
        }
    }

    // Helper method to add a weapon back to inventory.
    public void AddWeapon(WeaponData data)
    {
        weaponInventory.Add(data);
        OnInventoryUpdated?.Invoke();
    }
}
