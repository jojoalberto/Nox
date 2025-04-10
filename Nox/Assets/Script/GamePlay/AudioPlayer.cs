using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioPlayer : MonoBehaviour
{
    [Header("Audio Clip to Play")]
    public AudioClip clip;
    public UnityEvent OnPlayAudio;
    private AudioSource audioSource;
    void Start()
    {
        // Add or get existing AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
    }
    public void PlaySound()
    {
        if (clip != null)
        {
            audioSource.Play();
            OnPlayAudio.Invoke();
        }
    }

}
