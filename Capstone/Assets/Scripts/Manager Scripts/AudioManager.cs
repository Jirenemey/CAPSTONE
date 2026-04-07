using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource, loopSFXSource, playerSource, playerFallingSource, playerHealSource;

    void Awake() {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (!musicSource) musicSource = GameObject.Find("Music").GetComponent<AudioSource>();
        if (!sfxSource) sfxSource = GameObject.Find("SFX").GetComponent<AudioSource>();

        if (!loopSFXSource) loopSFXSource = GameObject.Find("LoopSFX").GetComponent<AudioSource>();
    }

    void Start()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSource.volume = musicVol;
        sfxSource.volume = sfxVol;
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

    public void PlayLoopSFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Loop SFX Not Found");
        }
        else
        {
            loopSFXSource.clip = s.clip;
            loopSFXSource.loop = true;
            loopSFXSource.Play();
        }
    }

    public void StopLoopSFX()
    {
        loopSFXSource.Stop();
        loopSFXSource.loop = false;
    }

    public AudioSource PlayLoopSFXAtObject(string name, Transform target)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Loop SFX Not Found");
            return null;
        }

        AudioSource source = target.gameObject.AddComponent<AudioSource>();
        source.clip = s.clip;
        source.loop = true;

        //source.spatialBlend = 1f; // full 3D
        //source.minDistance = 1f;
        //source.maxDistance = 10f;
        //source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.volume = UnityEngine.Random.Range(0.9f, 1f);
        source.pitch = UnityEngine.Random.Range(0.95f, 1.05f);

        source.Play();

        return source;
    }

    public void MusicVolume(float volume){
        musicSource.volume = volume;

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    
    public void SFXVolume(float volume){
        sfxSource.volume = volume;
        LoopSFXVolume();
        AdjustPlayerVolume();

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void AdjustPlayerVolume()
    {
        if(playerSource && playerFallingSource && playerHealSource){
            playerSource.volume = sfxSource.volume;
            playerFallingSource.volume = sfxSource.volume * 0.25f;
            playerHealSource.volume = sfxSource.volume * 0.25f;
        }
    }

    public void LoopSFXVolume()
    {
        loopSFXSource.volume = sfxSource.volume;
    }
}
