using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun; 

public class LootSpawner : MonoBehaviour
{
    public bool debug = false; // this will need to trigger on a death event
    private float lootDropRadius = 1f;

    // TODO - Temporary, weapon or talisman needs an assigned gameobject as a property.
    private void Update()
    {
        if (debug)
        { 
            int numberOfItemsToSpawn = Random.Range(1, 4); // Max 3 items per loot spawn
            for (int i = 0; i < numberOfItemsToSpawn; i++)
            {
                int itemSelection = Random.Range(0, 2);

                switch (itemSelection)
                {
                    case 0:
                        int randomWeaponModelIndex = Random.Range(0, LootManager.instance.weapons.Count);
                        // Use PhotonNetwork.Instantiate instead of Instantiate?
                        GameObject instantiatedWeapon = PhotonNetwork.InstantiateRoomObject(LootManager.instance.weapons[randomWeaponModelIndex].name, GetRandomPointOnCircle(), GetRandomRotation());
                        WeaponItem newWeapon = instantiatedWeapon.GetComponent<WeaponItem>();
                        newWeapon.itemModel = instantiatedWeapon.GetComponent<MeshFilter>();

                        break;

                    case 1:
                        // ONCE A TALISMAN MODEL IS PRESENT IN GAME ITEM DB THEN UNCOMMENT
                        //int randomTalismanModelIndex = Random.Range(0, LootManager.instance.talismans.Count);
                        //GameObject instantiatedTalisman = PhotonNetwork.Instantiate(LootManager.instance.talismans[randomTalismanModelIndex].name, GetRandomPointOnCircle(), GetRandomRotation());
                        //TalismanItem newTalisman = instantiatedTalisman.GetComponent<TalismanItem>();
                        //newTalisman.itemModel = instantiatedTalisman.GetComponent<MeshFilter>();

                        Debug.Log("Dropped Talisman");
                        break;
                }
            }
        
            debug = false;
        }
    }

    public Vector3 GetRandomPointOnCircle()
    {
        // Random angle in radians between 0 and 2pi
        float angle = Random.Range(0f, Mathf.PI * 2);
        Vector3 circleCenter = transform.position;

        // Calculate x and z coordinates on the circle
        float x = circleCenter.x + Mathf.Cos(angle) * lootDropRadius;
        float z = circleCenter.z + Mathf.Sin(angle) * lootDropRadius;

        return new Vector3(x, circleCenter.y + 0.5f, z);
    }

    public Quaternion GetRandomRotation()
    {
        // Choose a random angle in degrees between 0 and 360 for the Y-axis
        float randomXAngle = Random.Range(0f, 360f);
        return Quaternion.Euler(randomXAngle, 0f, 0f);
    }
}


