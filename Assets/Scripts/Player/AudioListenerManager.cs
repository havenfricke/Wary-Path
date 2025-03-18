using UnityEngine;

public class AudioListenerManager : MonoBehaviour
{
    // This should be assigned in the Inspector or will be set from this GameObject.
    public AudioListener designatedListener;

    void Awake()
    {
        // If not assigned, attempt to get the AudioListener on this GameObject.
        if (designatedListener == null)
        {
            designatedListener = GetComponent<AudioListener>();
        }
        if (designatedListener == null)
        {
            Debug.LogError("No designated AudioListener found on this GameObject. Please assign one.");
        }
    }

    void Update()
    {
        // Find all AudioListeners in the scene using non-deprecated code.
        AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

        foreach (AudioListener listener in allListeners)
        {
            // If this isn't the designated AudioListener and it's enabled, disable it.
            if (listener != designatedListener && listener.enabled)
            {
                listener.enabled = false;
                Debug.Log("Disabled an extra AudioListener on: " + listener.gameObject.name);
            }
        }

        Debug.Log("AudioListenerManager: Ensured only the designated AudioListener remains active.");
    }
}
