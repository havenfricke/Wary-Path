using System;
using UnityEngine;

public class QuitGameManager : MonoBehaviour
{
    private SaveGameManager saveManager;

    private void Awake()
    {
        saveManager = GetComponent<SaveGameManager>();
    }

    public async void QuitApplication()
    {
        try
        {
            await saveManager.SaveGame(saveManager.SaveGameData());
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
        }
        catch (Exception error)
        {
            Debug.LogError(error);
        }
    }
}
