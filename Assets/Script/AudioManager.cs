using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // Make the AudioManager persist between scenes
        Init(); // Initialize audio sources immediately in Awake
    }

    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] int audioSourceCount;
    [Header("Default Audio Settings")]
    [SerializeField][Range(0f, 1f)] float defaultVolume = 1f;
    [SerializeField][Range(0.1f, 3f)] float defaultPitch = 1f;

    List<AudioSource> audioSources;

    private void Init()
    {
        // Clear the list first to prevent duplicates when reinitializing
        if (audioSources != null)
        {
            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                {
                    Destroy(source.gameObject);
                }
            }
        }

        audioSources = new List<AudioSource>();
        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject go = Instantiate(audioSourcePrefab);
            go.transform.SetParent(transform); // Parent to AudioManager so it persists
            go.transform.localPosition = Vector3.zero;
            AudioSource audioSource = go.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSources.Add(audioSource);
                DontDestroyOnLoad(go); // Make the audio source persist between scenes
            }
        }
    }

    // Original method - maintains backward compatibility
    public void Play(AudioClip audioClip)
    {
        Play(audioClip, defaultVolume, defaultPitch);
    }

    // Overloaded method with volume control
    public void Play(AudioClip audioClip, float volume)
    {
        Play(audioClip, volume, defaultPitch);
    }

    // Full control method with volume and pitch
    public void Play(AudioClip audioClip, float volume, float pitch)
    {
        if (audioClip == null) return;

        AudioSource audioSource = GetFreeAudioSource();
        if (audioSource != null)
        {
            audioSource.clip = audioClip;
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
            audioSource.Play();
        }
    }

    // Alternative method name for clarity (matches the door script)
    public void PlayWithSettings(AudioClip audioClip, float volume, float pitch)
    {
        Play(audioClip, volume, pitch);
    }

    // Method to play with random pitch variation
    public void PlayWithVariation(AudioClip audioClip, float volume, float basePitch, float pitchVariation)
    {
        float randomPitch = basePitch + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
        Play(audioClip, volume, randomPitch);
    }

    // Method to stop all currently playing audio
    public void StopAll()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    // Method to pause all currently playing audio
    public void PauseAll()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Pause();
            }
        }
    }

    // Method to resume all paused audio
    public void ResumeAll()
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
    }

    private AudioSource GetFreeAudioSource()
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            // Check if the AudioSource still exists and is not playing
            if (audioSources[i] != null && !audioSources[i].isPlaying)
            {
                return audioSources[i];
            }
        }

        // Clean up null references and reinitialize if needed
        audioSources.RemoveAll(source => source == null);
        if (audioSources.Count == 0)
        {
            Init(); // Reinitialize if all audio sources are gone
        }

        return audioSources.Count > 0 ? audioSources[0] : null;
    }
}