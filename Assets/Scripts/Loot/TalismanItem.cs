using System;
using UnityEngine;

public class TalismanItem : MonoBehaviour
{
    private Talisman talisman; // The rolled talisman (this will be set)

    public string itemRarity;
    public float damageReduction;
    public float damageModifier;
    public MeshFilter itemModel;
    public string itemId;

    private void Awake()
    {
        // Roll for a talisman from LootManager
        talisman = LootManager.instance.RollTalisman();

        // Apply the rolled talisman data to the current GameObject
        if (talisman != null)
        {
            itemRarity = talisman.itemRarity;
            damageReduction = talisman.damageReduction;
            damageModifier = talisman.damageModifier;
            itemId = Guid.NewGuid().ToString();

            Destroy(talisman);
        }
        else
        {
            Debug.LogError("LootManager did not provide a talisman.");
        }
    }
}
