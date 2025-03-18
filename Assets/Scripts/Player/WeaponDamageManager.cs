using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class WeaponDamageManager : MonoBehaviourPun
{
    private float damageAmount = 100f;
    private List<int> damagedPlayers = new List<int>();
    private float damageCooldown = 0.1f;

    // Instead of using Awake, we wait until Start to allow WeaponInstance to be initialized.
    private IEnumerator Start()
    {
        // Wait one frame so that WeaponInstance.Initialize() is likely done.
        yield return null;

        // Try to get the WeaponInstance (which holds persistent WeaponData).
        WeaponInstance instance = GetComponent<WeaponInstance>();
        if (instance != null && instance.weaponData != null)
        {
            damageAmount *= instance.weaponData.baseDamage;
            Debug.Log("Damage amount calculated from WeaponInstance: " + damageAmount);
        }
        else
        {
            Debug.LogWarning("WeaponInstance or weaponData not found. Using default damage: " + damageAmount);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("WHACK");

            PhotonView targetPhotonView = other.GetComponentInParent<PhotonView>();

            if (targetPhotonView != null)
            {
                // Prevent self-damage.
                if (targetPhotonView.Owner == photonView.Owner)
                {
                    return;
                }

                if (damagedPlayers.Contains(targetPhotonView.ViewID))
                {
                    Debug.Log("Target recently damaged, skipping.");
                    return;
                }

                targetPhotonView.RPC("RPC_TakeDamage", targetPhotonView.Owner, damageAmount);

                damagedPlayers.Add(targetPhotonView.ViewID);

                StartCoroutine(RemoveFromDamageList(targetPhotonView.ViewID));
            }
            else
            {
                Debug.LogWarning("No PhotonView found on target or its parents.");
            }
        }
    }

    private IEnumerator RemoveFromDamageList(int viewID)
    {
        yield return new WaitForSeconds(damageCooldown);
        damagedPlayers.Remove(viewID);
    }
}
