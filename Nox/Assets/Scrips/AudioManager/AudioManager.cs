using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioManager : MonoBehaviourPun
{
    public AudioClip soundTrapAudioClip;
    [Header("Audio Settings")]
    public List<AudioClip> sfxAudioClips = new List<AudioClip>();
    public List<AudioClip> musicAudioClips = new List<AudioClip>();
    [SerializeField] private AudioSource sfxAudioSource;
    public UnityEvent onAudioEnd;

    [SerializeField] private AudioSource musicAudioSourceA;
    [SerializeField] private AudioSource musicAudioSourceB;

    private AudioSource currentMusicSource;
    private AudioSource nextMusicSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }

        currentMusicSource = musicAudioSourceA;
        nextMusicSource = musicAudioSourceB;

        if (!currentMusicSource.isPlaying && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Music Attempt");
            RequestPlayMusicClipByName("General");
        }
    }

    [PunRPC]
    public void PlayAudioByIndex(int index)
    {
        if (index >= 0 && index < sfxAudioClips.Count)
        {
            Debug.Log("Playing AudioClip at index: " + index);
            sfxAudioSource.PlayOneShot(sfxAudioClips[index]);
            StartCoroutine(WaitForSFXToEnd(sfxAudioSource));
        }
        else
        {
            Debug.Log("Invalid audio index: " + index);
        }
    }

    [PunRPC]
    public void PlayAudioByName(string clipName)
    {
        AudioClip clip = sfxAudioClips.Find(c => c.name == clipName);
        if (clip != null)
        {
            Debug.Log("Playing AudioClip named: " + clipName);
            sfxAudioSource.PlayOneShot(clip);
            StartCoroutine(WaitForSFXToEnd(sfxAudioSource));
        }
        else
        {
            Debug.LogWarning("AudioClip not found with name: " + clipName);
        }
    }

    [PunRPC]
    public void PlayDefaultAudio()
    {
        if (sfxAudioClips.Count > 0)
        {
            sfxAudioSource.PlayOneShot(sfxAudioClips[0]);
            StartCoroutine(WaitForSFXToEnd(sfxAudioSource));
        }
    }

    public void RPCPlayAudioByIndex(int index)
    {
        photonView.RPC("PlayAudioByIndex", RpcTarget.All);
    }

    public void RPCPlayAudioByName(string clipName)
    {
        photonView.RPC("PlayAudioByName", RpcTarget.All, clipName);
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

    public void RequestPlayMusicClipByName(string name)
    {
        Debug.Log("Requesting music:" + name);
        photonView.RPC("RPC_PlayMusicClipByName", RpcTarget.All, name);
    }

    [PunRPC]
    public void RPC_PlayMusicClipByName(string name)
    {
        Debug.Log("RPC_PlayMusicClipByName:" + name);
        StartCoroutine(CrossfadeToNewMusic(name));
    }

    private IEnumerator CrossfadeToNewMusic(string newClipName)
    {
        float crossfadeTime = 2.0f;

        AudioClip newClip = musicAudioClips.Find(c => c.name == newClipName);
        if (newClip == null)
        {
            Debug.LogWarning("Music clip not found: " + newClipName);
            yield break;
        }
        if (currentMusicSource.clip == newClip && currentMusicSource.isPlaying)
        {
            // The clip is already playing, no need to change
            Debug.Log("The clip is already playing.");
            yield break;
        }

        // Set up the next music source
        nextMusicSource.clip = newClip;
        nextMusicSource.volume = 0f;
        nextMusicSource.loop = true;
        nextMusicSource.Play();

        float timeElapsed = 0f;
        float startVolume = currentMusicSource.volume;

        while (timeElapsed < crossfadeTime)
        {
            float t = timeElapsed / crossfadeTime;
            currentMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            nextMusicSource.volume = Mathf.Lerp(0f, startVolume, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure perfect end values
        currentMusicSource.volume = 0f;
        nextMusicSource.volume = startVolume;

        currentMusicSource.Stop();

        // Swap sources
        AudioSource temp = currentMusicSource;
        currentMusicSource = nextMusicSource;
        nextMusicSource = temp;
    }



}
