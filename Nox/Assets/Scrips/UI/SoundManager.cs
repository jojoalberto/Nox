using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(PlayerPrefs.HasKey("musicVolume"))
        {
            LoadPlayerSoundSettings();
        }

        SetMusicVolume();
        SetSFXVolume();
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    private void LoadPlayerSoundSettings()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SetMusicVolume();

        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        SetSFXVolume();
    }
    
}
