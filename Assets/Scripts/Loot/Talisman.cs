using UnityEngine;

[CreateAssetMenu(fileName = "NewTalisman", menuName = "Talisman")]
public class Talisman : ScriptableObject
{
    public string itemRarity = null;
    public float damageReduction = 0;
    public float damageModifier = 0;
    public MeshFilter itemModel;
    public string itemId;

}
