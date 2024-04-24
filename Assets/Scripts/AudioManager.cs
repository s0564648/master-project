using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioClip thriceEffectSound;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        } 
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    //Instantiates a New Audio Source and plays audio
    public void PlayAudioClip(AudioClip audioClip)
    {
        AudioSource audioSource = new GameObject("Audio Source", typeof(AudioSource)).GetComponent<AudioSource>();
        audioSource.transform.SetParent(transform);


        audioSource.clip = audioClip;
        audioSource.Play();

        Destroy(audioSource.gameObject, audioClip.length + 1f);
    }
}
