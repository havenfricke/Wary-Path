using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public string gameVersion = "1";
    public string roomName = "MainSceneRoom";
    public bool autoJoinRoom = true;

    // Optional: assign your custom prefab pool (if you are not putting your prefabs in a Resources folder)
    public CustomPrefabPool customPrefabPool;

    private void Start()
    {
        // Register custom prefab pool with Photon if one is assigned.
        if (customPrefabPool != null)
        {
            PhotonNetwork.PrefabPool = customPrefabPool;
        }

        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Already connected to Photon");
            JoinOrCreateRoom();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server!");
        if (autoJoinRoom)
        {
            JoinOrCreateRoom();
        }
    }

    void JoinOrCreateRoom()
    {
        Debug.Log("Trying to join or create the room...");
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 20 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined the room successfully!");
        PhotonNetwork.LoadLevel("MainScene");
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} has entered the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} has left the room.");
    }
}
