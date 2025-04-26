using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioManager : MonoBehaviourPun
{
    public AudioClip soundTrapAudioClip;
    [Header("Audio Settings")]
    public List<AudioClip> sfxAudioClips = new List<AudioClip>();
    public List<AudioClip> musicAudioClips = new List<AudioClip>();
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    public UnityEvent onAudioEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
        }

        if (!musicAudioSource.isPlaying && PhotonNetwork.IsMasterClient)
        {
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

    private void RequestPlayMusicClipByName(string name)
    {
        Debug.Log("Requesting music:" + name);
        photonView.RPC("RPC_PlayMusicClipByName", RpcTarget.All, name);
    }

    [PunRPC]
    public void RPC_PlayMusicClipByName(string name)
    {
        Debug.Log("RPC_PlayMusicClipByName:" + name);
        StartCoroutine(FadeOutAndSwitchMusic(name));
    }

    private IEnumerator FadeOutAndSwitchMusic(string newClipName)
    {

        Debug.Log("Changing music to :" + newClipName);
        float fadeOutTime = 1.5f; // Duration of fade out
        float fadeInTime = 1.5f;  // Duration of fade in

        // Fade out
        float startVolume = musicAudioSource.volume;
        while (musicAudioSource.volume > 0)
        {
            musicAudioSource.volume -= startVolume * Time.deltaTime / fadeOutTime;
            yield return null;
        }

        musicAudioSource.Stop();
        musicAudioSource.volume = startVolume; // Reset volume immediately after stopping

        // Switch clip
        AudioClip newClip = musicAudioClips.Find(c => c.name == newClipName);
        if (newClip != null)
        {
            musicAudioSource.clip = newClip;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Music clip not found: " + newClipName);
            yield break; // Exit early if clip not found
        }

        // Fade in
        musicAudioSource.volume = 0f;
        while (musicAudioSource.volume < startVolume)
        {
            musicAudioSource.volume += startVolume * Time.deltaTime / fadeInTime;
            yield return null;
        }

        musicAudioSource.volume = startVolume; // Ensure exact volume at the end
    }


}
