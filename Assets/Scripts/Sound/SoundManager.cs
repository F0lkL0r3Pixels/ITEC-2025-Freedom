using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Sound
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Clips")]
    [SerializeField] private Sound[] bgmClips;
    [SerializeField] private Sound[] sfxClips;

    private Dictionary<string, AudioClip> bgmDictionary;
    private Dictionary<string, AudioClip> sfxDictionary;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    private float bgmVolume = 0.7f;
    [Range(0f, 1f)]
    private float sfxVolume = 0.8f;

    private const string BGM_VOLUME_KEY = "BgmVolume";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    void Awake()
    {
        InitializeAudioSources();
        BuildDictionaries();
        LoadVolumeSettings();
    }

    void InitializeAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true; // BGM should loop
            bgmSource.playOnAwake = false;
            Debug.Log("SoundManager: Created BGM AudioSource.");
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false; // SFX shouldn't loop
            sfxSource.playOnAwake = false;
            Debug.Log("SoundManager: Created SFX AudioSource.");
        }
    }

    void BuildDictionaries()
    {
        bgmDictionary = new Dictionary<string, AudioClip>();
        foreach (Sound sound in bgmClips)
        {
            if (!bgmDictionary.ContainsKey(sound.name) && sound.clip != null)
            {
                bgmDictionary.Add(sound.name, sound.clip);
            }
            else
            {
                Debug.LogWarning($"SoundManager: Duplicate BGM name '{sound.name}' or null clip detected. Skipping.");
            }
        }

        sfxDictionary = new Dictionary<string, AudioClip>();
        foreach (Sound sound in sfxClips)
        {
            if (!sfxDictionary.ContainsKey(sound.name) && sound.clip != null)
            {
                sfxDictionary.Add(sound.name, sound.clip);
            }
            else
            {
                Debug.LogWarning($"SoundManager: Duplicate SFX name '{sound.name}' or null clip detected. Skipping.");
            }
        }
    }

    public void PlayBGM(string name)
    {
        if (bgmDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"SoundManager: BGM track '{name}' not found.");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: SFX '{name}' not found.");
        }
    }

    public void PlaySFXAtPoint(string name, Vector3 position)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            // PlayClipAtPoint creates a temporary GameObject with an AudioSource
            // Volume is managed by 3D spatialization + our master SFX volume
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"SoundManager: SFX '{name}' not found for PlayAtPoint.");
        }
    }

    private void LoadVolumeSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, sfxVolume);

        bgmSource.volume = bgmVolume;
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();

        // Optional: Play a test sound immediately to give feedback
        // PlaySFX("UITick"); // Assuming you have an SFX named "UITick"
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}