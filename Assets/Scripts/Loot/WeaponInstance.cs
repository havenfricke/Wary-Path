using UnityEngine;
using Photon.Pun;

public class WeaponInstance : MonoBehaviourPun
{
    public WeaponData weaponData;  // This instance’s persistent data

    /// <summary>
    /// Initializes the networked weapon using persistent inventory data.
    /// </summary>
    public void Initialize(WeaponData data)
    {
        if (data == null)
        {
            Debug.LogError("WeaponInstance.Initialize received null data!");
            return;
        }

        Debug.Log("WeaponInstance.Initialize called with itemId: " + data.itemId);

        // Force a new WeaponData assignment that overwrites prefab defaults.
        weaponData = new WeaponData
        {
            itemId = data.itemId,
            prefabName = data.prefabName,
            itemRarity = data.itemRarity,
            baseDamage = data.baseDamage,
            attackSpeedModifier = data.attackSpeedModifier,
            damageModifier = data.damageModifier
        };

        Debug.Log("After initialization, weaponData: baseDamage=" + weaponData.baseDamage +
            ", attackSpeed=" + weaponData.attackSpeedModifier +
            ", damageModifier=" + weaponData.damageModifier);

        // Immediately update any visuals (or other properties) based on weaponData.
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Update visuals here based on weaponData.
        // For example, update a text component or material color.
        Debug.Log("Updating visuals for weapon with itemId: " + weaponData.itemId);
    }
}
