using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    private IEnumerator Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not assigned in PlayerSpawner.");
            yield break;
        }

        // Wait until the active scene is MainScene.
        while (SceneManager.GetActiveScene().name != "MainScene")
        {
            yield return null;
        }

        // Then wait until Photon is connected and in a room.
        while (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            yield return null;
        }

        // Slight additional delay for latency.
        yield return new WaitForSeconds(0.1f);

        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        // Only spawn a local player if one does not already exist.
        bool localPlayerExists = FindObjectsOfType<PhotonView>()
            .Any(p => p.IsMine && p.gameObject.CompareTag("Player"));

        if (localPlayerExists)
        {
            Debug.Log("Local player already exists on this client; not spawning again.");
            return;
        }

        // Calculate a proper spawn Y position based on the CharacterController's offset.
        CharacterController cc = playerPrefab.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("Player prefab does not have a CharacterController!");
            return;
        }
        // Calculate how far the bottom of the controller is from the transform's position.
        float bottomOffset = cc.center.y - cc.height * 0.5f;
        float desiredGroundY = 0f; // Assumes your ground is at Y = 0.
        float spawnY = desiredGroundY - bottomOffset;

        Vector3 spawnPosition = new Vector3(Random.Range(-10f, 10f), spawnY, Random.Range(-10f, 10f));
        Quaternion spawnRotation = Quaternion.identity;

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);

        if (player == null)
        {
            Debug.LogError("Failed to spawn player!");
        }
        else
        {
            Debug.Log("Player instantiated successfully on client " + PhotonNetwork.NickName);
        }
    }

    // Helper method to find the local player instance.
    private GameObject FindLocalPlayer()
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in photonViews)
        {
            if (pv.IsMine && pv.gameObject.CompareTag("Player"))
                return pv.gameObject;
        }
        return null;
    }

    // Public method to be called by the AnimatorManager to respawn the player.
    public void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayerRoutine());
    }

    private IEnumerator RespawnPlayerRoutine()
    {
        // Destroy the local player instance, if it exists.
        GameObject localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            PhotonNetwork.Destroy(localPlayer);
            // Allow a brief delay to ensure cleanup.
            yield return new WaitForSeconds(0.5f);
        }

        // Spawn a new player instance.
        SpawnPlayer();
    }
}
