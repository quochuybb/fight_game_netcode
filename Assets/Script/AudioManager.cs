using UnityEngine;
using UnityEngine.Audio; // Required for audio mixer

public class AudioManager : MonoBehaviour

{
    [Header(" ------ Audio Source ------")] 
    [SerializeField] AudioSource musicSource; 
    [SerializeField] AudioSource SFXSource; 
    [Header("------ Audio Clip ------")] 
    public AudioClip clickbutton; 
    public AudioClip soundtrack1;
    private void Start()
    {
        musicSource.clip=soundtrack1;
        musicSource.Play();
    }
    public void PlaySFX (AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}