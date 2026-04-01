using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    void Awake() {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if(!musicSource) musicSource = GameObject.Find("Music").GetComponent<AudioSource>();
        if(!sfxSource) sfxSource = GameObject.Find("SFX").GetComponent<AudioSource>();
    }

    public void PlayMusic(string name){
        Sound s = Array.Find(musicSounds, x=> x.name == name);
        
        if(s == null){
            Debug.Log("Music Not Found");
        }

        else{
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }
    public void PauseMusic(){
        musicSource.Pause();
    }

    public void ResumeMusic(){
        musicSource.Play();
    }

    public void PlaySFX(string name){
        Sound s = Array.Find(sfxSounds, x=> x.name == name);

        if(s == null){
            Debug.Log("SFX Not Found");
        }
        else{
            sfxSource.PlayOneShot(s.clip);
        }
    }
    
    public void MusicVolume(float volume){
        musicSource.volume = volume;
    }
    
    public void SFXVolume(float volume){
        sfxSource.volume = volume;
    }
}
