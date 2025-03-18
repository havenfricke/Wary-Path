[System.Serializable]
public class WeaponData
{
    public string itemId;         // Unique ID for this weapon
    public string prefabName;     // Name of the prefab for PhotonNetwork.Instantiate
    public string itemRarity;     // e.g., "Common", "Magic", "Rare", etc.
    public float baseDamage;
    public float attackSpeedModifier;
    public float damageModifier;
    // Add additional fields as needed.
}
