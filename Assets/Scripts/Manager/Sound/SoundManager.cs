using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// SoundManager (Singleton) - Supports ScriptableObject-based sound data with multiple audio clips per event.
/// Sound files are paired with a key-enum, which can be used to call stored files. At least, that's the theory.
/// </summary>
public class SoundManager : MonoBehaviour
{
    #region singleton
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }
    #endregion

    #region audio sources
    [Header("Audio Source Pools")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private int musicPoolSize = 2;
    
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private List<AudioSource> musicPool = new List<AudioSource>();
    private AudioSource currentMusic;
    private AudioSource fadingMusic;
    #endregion

    #region volume settings
    [Header("Volume Settings")]
    [Range(0.0f, 1.0f)] public float masterVolume = 1.0f;
    [Range(0.0f, 1.0f)] public float musicVolume = 0.7f;
    [Range(0.0f, 1.0f)] public float sfxVolume = 1.0f;
    
    private float MusicVolume => masterVolume * musicVolume;
    private float SFXVolume => masterVolume * sfxVolume;
    #endregion

    #region audio variation
    [Header("Audio Variation")]
    [SerializeField] private bool enablePitchVariation = true;
    [SerializeField] private float pitchVariationAmount = 0.1f;
    #endregion

    #region Initialization
    private void Initialize()
    {
        // Create SFX audio source pool
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Add(source);
        }

        // Create music audio source pool
        for (int i = 0; i < musicPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            musicPool.Add(source);
        }
    }
    #endregion

    #region Play Sound Effects
    /// <summary>
    /// Plays a sound effect from a SoundData ScriptableObject.
    /// Randomly selects from available clips if multiple are defined.
    /// </summary>
    /// <param name="soundDataSo">The SoundData ScriptableObject containing the sound</param>
    /// <param name="soundEvent">The specific event to play</param>
    /// <param name="position">Optional 3D position for spatial audio</param>
    public void PlaySound(SoundDataSO soundDataSo, System.Enum soundEvent, Vector3? position = null)
    {
        if (soundDataSo == null)
        {
            Debug.LogWarning("SoundData is null!");
            return;
        }

        AudioClip clip = soundDataSo.GetRandomClip(soundEvent);
        if (clip == null)
        {
            Debug.LogWarning($"No audio clip found for event: {soundEvent}");
            return;
        }

        PlayClip(clip, SFXVolume, position);
    }

    /// <summary>
    /// Plays a single audio clip directly.
    /// </summary>
    public void PlayClip(AudioClip clip, float volumeScale = 1.0f, Vector3? position = null)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        if (source == null)
        {
            Debug.LogWarning("No available audio sources in pool!");
            return;
        }

        source.clip = clip;
        source.volume = SFXVolume * volumeScale;
        source.pitch = enablePitchVariation ? 1.0f + Random.Range(-pitchVariationAmount, pitchVariationAmount) : 1.0f;
        
        // if (position.HasValue)
        // {
        //     source.spatialBlend = 1.0f; // 3D sound
        //     source.transform.position = position.Value;
        // }
        // else
        // {
        //     source.spatialBlend = 0.0f; // 2D sound
        // }

        source.Play();
    }

    /// <summary>
    /// Plays a sound at a specific point in 3D space and destroys the AudioSource when done.
    /// Useful for one-shot sounds at specific locations.
    /// </summary>
    public void PlaySoundAtPoint(SoundDataSO soundDataSo, System.Enum soundEvent, Vector3 position)
    {
        if (soundDataSo == null) return;
        
        AudioClip clip = soundDataSo.GetRandomClip(soundEvent);
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, SFXVolume);
        }
    }
    #endregion

    #region Play Music
    /// <summary>
    /// Plays background music from a SoundData ScriptableObject.
    /// Supports crossfading if fadeTime > 0.
    /// </summary>
    public void PlayMusic(SoundDataSO soundDataSo, System.Enum musicEvent, float fadeTime = 0.0f)
    {
        if (soundDataSo == null) return;

        AudioClip clip = soundDataSo.GetRandomClip(musicEvent);
        if (clip == null)
        {
            Debug.LogWarning($"No music clip found for event: {musicEvent}");
            return;
        }

        PlayMusicClip(clip, fadeTime);
    }

    /// <summary>
    /// Plays a music clip directly with optional crossfading.
    /// </summary>
    public void PlayMusicClip(AudioClip clip, float fadeTime = 0f)
    {
        if (clip == null) return;

        if (fadeTime > 0.0f && currentMusic != null && currentMusic.isPlaying)
        {
            StartCoroutine(CrossfadeMusic(clip, fadeTime));
        }
        else
        {
            if (currentMusic != null)
            {
                currentMusic.Stop();
            }

            currentMusic = GetAvailableMusicSource();
            currentMusic.clip = clip;
            currentMusic.volume = MusicVolume;
            currentMusic.Play();
        }
    }

    /// <summary>
    /// Stops the currently playing music with optional fade out.
    /// </summary>
    public void StopMusic(float fadeTime = 0.0f)
    {
        if (currentMusic == null) return;

        if (fadeTime > 0.0f)
        {
            StartCoroutine(FadeOutMusic(currentMusic, fadeTime));
        }
        else
        {
            currentMusic.Stop();
        }
    }
    #endregion

    #region Music Crossfading
    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip, float fadeTime)
    {
        fadingMusic = currentMusic;
        currentMusic = GetAvailableMusicSource();
        
        currentMusic.clip = newClip;
        currentMusic.volume = 0.0f;
        currentMusic.Play();

        float elapsed = 0.0f;
        float startVolume = fadingMusic.volume;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            if (fadingMusic != null)
                fadingMusic.volume = Mathf.Lerp(startVolume, 0.0f, t);
            
            currentMusic.volume = Mathf.Lerp(0.0f, MusicVolume, t);

            yield return null;
        }

        if (fadingMusic != null)
        {
            fadingMusic.Stop();
            fadingMusic = null;
        }
    }

    private System.Collections.IEnumerator FadeOutMusic(AudioSource source, float fadeTime)
    {
        float startVolume = source.volume;
        float elapsed = 0.0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0.0f, elapsed / fadeTime);
            yield return null;
        }

        source.Stop();
    }
    #endregion

    #region Audio Source Management
    private AudioSource GetAvailableSFXSource()
    {
        // Find a source that's not playing
        AudioSource source = sfxPool.FirstOrDefault(s => !s.isPlaying);
        
        // If all are busy, use the one that's been playing the longest
        if (source == null)
        {
            source = sfxPool[0];
        }

        return source;
    }

    private AudioSource GetAvailableMusicSource()
    {
        // Find a music source that's not the current one
        AudioSource source = musicPool.FirstOrDefault(s => s != currentMusic && !s.isPlaying);
        
        if (source == null)
        {
            source = musicPool[0];
        }

        return source;
    }
    #endregion

    #region Volume Control Methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateAllVolumes()
    {
        if (currentMusic != null)
        {
            currentMusic.volume = MusicVolume;
        }
    }
    #endregion
}
