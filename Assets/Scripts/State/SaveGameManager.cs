using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    
    // these methods are for the GUI event OnClick();
    public void Save()
    {
        Helper();
    }

    private async void Helper()
    {
        try
        {
            await SaveGame(SaveGameData());
            Debug.Log("Save complete!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Save game error: " + ex);
        }
    }

    // Avoids corrupt / missing data on save or quit
    // these methods are for the quit game feature
    public Task SaveGame(IEnumerator coroutine)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(RunCoroutine(coroutine, tcs));
        return tcs.Task;
    }

    private IEnumerator RunCoroutine(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
    {
        yield return StartCoroutine(coroutine);
        tcs.SetResult(true);
    }

    public IEnumerator SaveGameData()
    {
        Debug.Log("Saving game data...");

        // SAVE GAME LOGIC GOES HERE

        yield return new WaitForSeconds(2f);
        Debug.Log("Game data saved.");
    }
}
