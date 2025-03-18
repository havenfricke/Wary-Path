using UnityEngine;
using System.Collections.Generic;

public class PlayerAudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Player Movement Audio Database")]
    public List<AudioClip> movementAudioClip;

    [Header("Player Combat Audio Database")]
    public List<AudioClip> combatAudioClips;

    [Header("Player Evade Audio Database")]
    public List<AudioClip> evadeAudioClips;

    [Header("Player Dash Audio Database")]
    public List<AudioClip> dashAudioClips;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Movement Audio
    public void PlayRandomFootstepSound()
    {
        if (movementAudioClip.Count > 0)
        {
            AudioClip chosenClip = movementAudioClip[Random.Range(0, movementAudioClip.Count)];
            // Play the clip once using PlayOneShot.
            audioSource.PlayOneShot(chosenClip);
        }
        else
        {
            Debug.LogWarning("Footstep audio database is empty.");
        }
    }



    // Combat Audio
    public void PlayCombatSoundAlpha()
    {
        List<AudioClip> validClips = combatAudioClips.FindAll(clip => clip.name.Contains("Alpha"));

        if (validClips.Count > 0)
        {
            AudioClip chosenClip = validClips[Random.Range(0, validClips.Count)];
            audioSource.clip = chosenClip;
            audioSource.PlayOneShot(chosenClip);
        }
        else
        {
            Debug.LogWarning("No combat audio clips found for Alpha");
        }
    }

    public void PlayCombatSoundBravo()
    {
        List<AudioClip> validClips = combatAudioClips.FindAll(clip => clip.name.Contains("Bravo"));

        if (validClips.Count > 0)
        {
            AudioClip chosenClip = validClips[Random.Range(0, validClips.Count)];
            audioSource.clip = chosenClip;
            audioSource.PlayOneShot(chosenClip);
        }
        else
        {
            Debug.LogWarning("No combat audio clips found for Bravo");
        }
    }

    public void PlayCombatSoundCharlie()
    {
        List<AudioClip> validClips = combatAudioClips.FindAll(clip => clip.name.Contains("Charlie"));

        if (validClips.Count > 0)
        {
            AudioClip chosenClip = validClips[Random.Range(0, validClips.Count)];
            audioSource.clip = chosenClip;
            audioSource.PlayOneShot(chosenClip);
        }
        else
        {
            Debug.LogWarning("No combat audio clips found for Charlie");
        }
    }

    public void PlayDashSound()
    {
        AudioClip chosenClip = dashAudioClips[Random.Range(0, dashAudioClips.Count)];

        if (chosenClip != null)
        {
            audioSource.clip = chosenClip;
            audioSource.PlayOneShot(chosenClip);
        }
        else
        {
            Debug.LogWarning("No combat audio clips found for Dash");
        }
    }

    public void PlayEvadeSound()
    {
        AudioClip chosenClip = evadeAudioClips[Random.Range(0, evadeAudioClips.Count)];

        if (chosenClip != null)
        {
            audioSource.clip = chosenClip;
            audioSource.PlayOneShot(chosenClip);
        }
        else
        {
            Debug.LogWarning("No combat audio clips found for Evade");
        }
    }
}
