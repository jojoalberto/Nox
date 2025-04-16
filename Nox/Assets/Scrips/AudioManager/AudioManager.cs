using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip soundTrapAudioClip;
    [SerializeField] private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAudio(GameObject game)
    {
        Debug.Log("Play Audio Called" + game);
        audioSource.PlayOneShot(soundTrapAudioClip);
    }
}
