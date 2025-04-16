using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip soundTrapAudioClip;
    [Header("Audio Settings")]
    public List<AudioClip> audioClips = new List<AudioClip>();
    [SerializeField] private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayAudioByIndex(int index)
    {
        if (index >= 0 && index < audioClips.Count)
        {
            Debug.Log("Playing AudioClip at index: " + index);
            audioSource.PlayOneShot(audioClips[index]);
        }
        else
        {
            Debug.Log("Invalid audio index: " + index);
        }
    }

    public void PlayAudioByName(string clipName)
    {
        AudioClip clip = audioClips.Find(c => c.name == clipName);
        if (clip != null)
        {
            Debug.Log("Playing AudioClip named: " + clipName);
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("AudioClip not found with name: " + clipName);
        }
    }

    public void PlayDefaultAudio()
    {
        if (audioClips.Count > 0)
        {
            audioSource.PlayOneShot(audioClips[0]);
        }
    }
}
