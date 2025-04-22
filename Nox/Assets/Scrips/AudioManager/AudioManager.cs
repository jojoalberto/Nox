using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioManager : MonoBehaviourPun
{
    public AudioClip soundTrapAudioClip;
    [Header("Audio Settings")]
    public List<AudioClip> audioClips = new List<AudioClip>();
    [SerializeField] private AudioSource audioSource;
    public UnityEvent onAudioEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    [PunRPC]
    public void PlayAudioByIndex(int index)
    {
        if (index >= 0 && index < audioClips.Count)
        {
            Debug.Log("Playing AudioClip at index: " + index);
            audioSource.PlayOneShot(audioClips[index]);
            StartCoroutine(WaitForSFXToEnd(audioSource));
        }
        else
        {
            Debug.Log("Invalid audio index: " + index);
        }
    }

    [PunRPC]
    public void PlayAudioByName(string clipName)
    {
        AudioClip clip = audioClips.Find(c => c.name == clipName);
        if (clip != null)
        {
            Debug.Log("Playing AudioClip named: " + clipName);
            audioSource.PlayOneShot(clip);
            StartCoroutine(WaitForSFXToEnd(audioSource));
        }
        else
        {
            Debug.LogWarning("AudioClip not found with name: " + clipName);
        }
    }

    [PunRPC]
    public void PlayDefaultAudio()
    {
        if (audioClips.Count > 0)
        {
            audioSource.PlayOneShot(audioClips[0]);
            StartCoroutine(WaitForSFXToEnd(audioSource));
        }
    }

    public void RPCPlayAudioByIndex(int index)
    {
        photonView.RPC("PlayAudioByIndex", RpcTarget.All);
    }

    public void RPCPlayAudioByName(string clipName)
    {
        photonView.RPC("PlayAudioByName", RpcTarget.All);
    }

    public void RPCPlayDefaultAudio()
    {
        photonView.RPC("PlayDefaultAudio", RpcTarget.All);
    }

    private IEnumerator WaitForSFXToEnd(AudioSource sfxSource)
    {
        yield return new WaitWhile(() => sfxSource.isPlaying);
        onAudioEnd.Invoke();
    }

}
