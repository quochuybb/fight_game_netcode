using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Button Click Sound")]
    [Tooltip("Assign your UI button 'click' AudioClip here")]
    public AudioClip buttonClickClip;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError("AudioController requires an AudioSource on the same GameObject.");
            return;
        }

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f; // Fully 2D
    }
    public void PlayClickSound()
    {
        if (buttonClickClip == null)
        {
            Debug.LogWarning("AudioController: buttonClickClip is not assigned!");
            return;
        }

        _audioSource.PlayOneShot(buttonClickClip);
    }
}

