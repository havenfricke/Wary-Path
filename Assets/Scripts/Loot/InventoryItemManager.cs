using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemManager : MonoBehaviour
{
    private InventoryManager inventoryManager;
    private PlayerUIManager playerUIManager;

    [Header("Inventory Item Button Prefabs")]
    public Button weaponItemUIPrefab;
    public Button talismanItemUIPrefab;

    private void Awake()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
        playerUIManager = GetComponentInParent<PlayerUIManager>();
    }

    private void OnEnable()
    {
        if (playerUIManager.isMenuOpen)
        {
            inventoryManager.OnInventoryUpdated += RefreshInventoryUI;
            RefreshInventoryUI();
        }
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryUpdated -= RefreshInventoryUI;
        }
    }

    public void RefreshInventoryUI()
    {
        ClearInventoryUI();
        GetInventoryItems();
    }

    private void ClearInventoryUI()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void GetInventoryItems()
    {
        // Loop through each WeaponData in the inventory.
        for (int i = 0; i < inventoryManager.weaponInventory.Count; i++)
        {
            WeaponData data = inventoryManager.weaponInventory[i];
            Button newWeaponUIItem = Instantiate(weaponItemUIPrefab);
            newWeaponUIItem.transform.SetParent(this.transform, false);

            // Set up the button's reference.
            InventoryItemIdReference idRef = newWeaponUIItem.GetComponent<InventoryItemIdReference>();
            idRef.itemIdReference = data.itemId;

            TMP_Text[] textFields = newWeaponUIItem.GetComponentsInChildren<TMP_Text>();
            foreach (TMP_Text textField in textFields)
            {
                switch (textField.gameObject.name)
                {
                    case "Item Name":
                        // Remove the "_Owned" suffix for display.
                        string displayName = data.prefabName.Replace("_Owned", "").Trim();
                        textField.text = displayName;
                        string rarity = data.itemRarity.ToLower();
                        if (rarity.Contains("common"))
                            textField.color = Color.white;
                        else if (rarity.Contains("magic"))
                            textField.color = Color.blue;
                        else if (rarity.Contains("rare"))
                            textField.color = Color.yellow;
                        else
                            textField.color = Color.red;
                        break;
                    case "Base Damage":
                        textField.text = "Base damage: " + data.baseDamage;
                        break;
                    case "Attack Speed":
                        textField.text = "Attack speed: " + data.attackSpeedModifier;
                        break;
                    case "Damage Modifier":
                        textField.text = "Damage modifier: " + data.damageModifier;
                        break;
                }
            }
        }
    }
}
