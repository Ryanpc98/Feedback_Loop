using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    // From https://www.youtube.com/watch?v=DU7cgVsU2rM

    public static SoundEffectsManager instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        // Spawn in game object
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // Assign the clip
        audioSource.clip = audioClip;

        // Assign the volume
        audioSource.volume = volume;

        // Play sound
        audioSource.Play();

        // Get length of clip
        float clipLength = audioSource.clip.length;

        // Destroy game object
        Destroy(audioSource.gameObject, clipLength);
    }
}
