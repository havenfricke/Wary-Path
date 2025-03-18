using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquippedItemsAccessor : MonoBehaviour
{
    [SerializeField] public InventoryItemManager inventoryItemManager;
    [SerializeField] public InventoryManager inventoryManager;
    [SerializeField] public EquippedWeapon weaponEquip;
    [SerializeField] public PlayerUIManager playerUIManager;
    // public EquippedTalisman equippedTalisman; // NOT IMPLEMENTED

    private string selectedItemId;
    private WeaponData selectedWeaponData;  // Persistent data for the weapon.

    private void Awake()
    {
        // Look for components in children and parent as needed.
        //weaponEquip = GetComponentInChildren<EquippedWeapon>();
        //if (weaponEquip == null)
        //{
        //    Debug.LogError("EquippedWeapon not found in children of EquippedItemsAccessor!");
        //}

        //// Use GetComponentInParent for InventoryManager and PlayerUIManager.
        //inventoryManager = GetComponentInParent<InventoryManager>();
        //if (inventoryManager == null)
        //{
        //    Debug.LogError("InventoryManager not found in parent of EquippedItemsAccessor!");
        //}

        //playerUIManager = GetComponentInParent<PlayerUIManager>();
        //if (playerUIManager == null)
        //{
        //    Debug.LogError("PlayerUIManager not found in parent of EquippedItemsAccessor!");
        //}

        //// Auto-assign InventoryItemManager if not set.
        //if (inventoryItemManager == null)
        //{
        //    inventoryItemManager = GetComponentInChildren<InventoryItemManager>();
        //    if (inventoryItemManager == null)
        //    {
        //        Debug.LogError("InventoryItemManager not found in children of EquippedItemsAccessor!");
        //    }
        //}
    }

    // Directly equip an item by passing its ID.
    public void EquipItem(string itemId)
    {
        SetSelectedItemId(itemId);
        EquipSelectedItem();
    }

    public void EquipSelectedItem()
    {
        // If no item ID is selected, try to get it from the current UI element.
        if (string.IsNullOrEmpty(selectedItemId))
        {
            GameObject currentButton = EventSystem.current.currentSelectedGameObject;
            if (currentButton != null && currentButton.TryGetComponent<InventoryItemIdReference>(out InventoryItemIdReference idRef))
            {
                SetSelectedItemId(idRef.itemIdReference);
                Debug.Log("Fallback selected item ID: " + idRef.itemIdReference);
            }
        }

        Debug.Log("EquipSelectedItem called with selectedItemId: " + selectedItemId);

        // Search the persistent inventory (a list of WeaponData) for a matching item.
        selectedWeaponData = null;
        foreach (WeaponData data in inventoryManager.weaponInventory)
        {
            if (data.itemId == selectedItemId)
            {
                selectedWeaponData = data;
                Debug.Log("Found matching WeaponData with ID: " + data.itemId);
                break;
            }
        }

        // If not found, check if it's already equipped.
        if (selectedWeaponData == null)
        {
            if (weaponEquip != null && weaponEquip.weapon != null)
            {
                WeaponInstance instance = weaponEquip.weapon.GetComponent<WeaponInstance>();
                if (instance != null && instance.weaponData != null && instance.weaponData.itemId == selectedItemId)
                {
                    Debug.Log("Item already equipped: " + selectedItemId);
                    return;
                }
            }
            Debug.LogWarning("No inventory item found matching selectedItemId: " + selectedItemId + ". It may already be equipped.");
            return;
        }

        // Remove the selected weapon data from the inventory.
        inventoryManager.weaponInventory.Remove(selectedWeaponData);
        Debug.Log("Removed weapon from inventory: " + selectedItemId);

        // If a weapon is already equipped, stow it.
        if (weaponEquip != null && weaponEquip.weapon != null)
        {
            WeaponData stowedData = weaponEquip.StowWeapon();
            if (stowedData != null)
            {
                inventoryManager.AddWeapon(stowedData);
            }
        }

        // Equip the new weapon.
        if (weaponEquip != null)
        {
            weaponEquip.EquipWeapon(selectedWeaponData);
        }
        else
        {
            Debug.LogError("weaponEquip is null!");
        }

        // Refresh the inventory UI.
        if (inventoryItemManager != null)
        {
            inventoryItemManager.RefreshInventoryUI();
        }
        else
        {
            Debug.LogError("InventoryItemManager is null; cannot refresh UI.");
        }
    }

    public void SetSelectedItemId(string id)
    {
        selectedItemId = id ?? string.Empty;
        Debug.Log("Set selected item ID: " + selectedItemId);
    }
}
