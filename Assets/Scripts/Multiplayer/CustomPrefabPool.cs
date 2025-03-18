using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CustomPrefabPool : MonoBehaviour, IPunPrefabPool
{
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    public void AddPrefab(string prefabName, GameObject prefab)
    {
        if (!prefabDictionary.ContainsKey(prefabName))
        {
            prefabDictionary.Add(prefabName, prefab);
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (prefabDictionary.ContainsKey(prefabId))
        {
            Debug.Log($"Instantiating prefab: {prefabId}");
            // Use Unity's Instantiate method explicitly
            return GameObject.Instantiate(prefabDictionary[prefabId], position, rotation);
        }
        else
        {
            Debug.LogError($"Prefab with ID {prefabId} not found in custom pool!");
            return null;
        }
    }

    public void Destroy(GameObject go)
    {
        Destroy(go);
    }
}
