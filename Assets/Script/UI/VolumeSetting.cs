using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer soundmixer;
    [SerializeField] private Slider MasterVolumeSlider;
    [SerializeField] private Slider MusicVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    private void Start()
    {
        setmastervolume();
        setmusicvolume();
        setSFXvolume();
    }
    public void setmastervolume()
    {
        float volume=MasterVolumeSlider.value;
        soundmixer.SetFloat("Master",Mathf.Log10(volume)*20);
    }
    public void setmusicvolume()
    {
        float volume=MusicVolumeSlider.value;
        soundmixer.SetFloat("Music",Mathf.Log10(volume)*20);
    }
    public void setSFXvolume()
    {
        float volume=SFXVolumeSlider.value;
        soundmixer.SetFloat("SFX",Mathf.Log10(volume)*20);
    }

}
