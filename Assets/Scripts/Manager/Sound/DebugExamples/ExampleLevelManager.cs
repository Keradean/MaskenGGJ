using UnityEngine;

/// <summary>
/// Example level manager showing how to handle level-specific audio.
/// Attach this to a manager GameObject in your scene.
/// </summary>
public class ExampleLevelManager : MonoBehaviour
{
    [Header("Level Sound Configuration")]
    [SerializeField] private SoundDataSO levelSounds;

    [Header("Music Settings")]
    [SerializeField] private float musicCrossfadeTime = 2f;

    [Header("Ambient Sound Settings")]
    [SerializeField] private bool playAmbientSounds = true;
    [SerializeField] private float ambientSoundInterval = 5f; // How often to play ambient sounds
    
    private bool isBossFight = false;
    private bool hadPreviousMusic = false; // Track if there was music before boss fight
    private bool areAmbientSoundsPlaying = false;
    private float ambientSoundTimer = 0f;

    private void Start()
    {
        // Start playing background music when level loads (if available)
        PlayBackgroundMusic();
        
        // Start ambient sounds (if available)
        if (playAmbientSounds)
        {
            StartAmbientSounds();
        }
    }

    private void Update()
    {
        // Periodically play ambient sounds if enabled
        if (areAmbientSoundsPlaying && playAmbientSounds)
        {
            ambientSoundTimer += Time.deltaTime;
            if (ambientSoundTimer >= ambientSoundInterval)
            {
                PlayAmbientSounds();
                ambientSoundTimer = 0f;
            }
        }
    }

    private void PlayBackgroundMusic()
    {
        if (levelSounds == null)
        {
            Debug.LogWarning("Level sounds not assigned!");
            return;
        }

        // Check if background music clip exists
        AudioClip bgMusic = levelSounds.GetRandomClip(LevelSoundEvent.BackgroundMusic);
        if (bgMusic != null)
        {
            SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.BackgroundMusic, musicCrossfadeTime);
            hadPreviousMusic = true;
        }
        else
        {
            Debug.Log("No background music assigned for this level.");
            hadPreviousMusic = false;
        }
    }

    private void StartAmbientSounds()
    {
        areAmbientSoundsPlaying = true;
        PlayAmbientSounds(); // Play immediately on start
    }

    private void PlayAmbientSounds()
    {
        if (levelSounds == null) return;

        // Check if ambient sound clip exists before playing
        AudioClip ambientClip = levelSounds.GetRandomClip(LevelSoundEvent.AmbientEnvironment);
        if (ambientClip != null)
        {
            SoundManager.Instance.PlaySound(levelSounds, LevelSoundEvent.AmbientEnvironment);
        }
    }

    private void StopAmbientSounds()
    {
        areAmbientSoundsPlaying = false;
        ambientSoundTimer = 0f;
    }

    // Call this when boss fight starts
    public void StartBossFight()
    {
        if (!isBossFight)
        {
            isBossFight = true;
            
            // Stop ambient sounds during boss fight
            StopAmbientSounds();
            
            // Check if boss music exists before trying to play it
            if (levelSounds != null)
            {
                AudioClip bossMusic = levelSounds.GetRandomClip(LevelSoundEvent.BossFightMusic);
                if (bossMusic != null)
                {
                    // Crossfade from background music to boss music
                    SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.BossFightMusic, musicCrossfadeTime);
                }
                else
                {
                    Debug.Log("No boss fight music assigned.");
                }
            }
        }
    }

    // Call this when boss is defeated
    public void OnBossDefeated()
    {
        if (isBossFight)
        {
            isBossFight = false;
            
            // Resume ambient sounds after boss fight
            if (playAmbientSounds)
            {
                StartAmbientSounds();
            }
            
            if (levelSounds != null)
            {
                // Check if victory music exists
                AudioClip victoryMusic = levelSounds.GetRandomClip(LevelSoundEvent.VictoryMusic);
                
                if (victoryMusic != null)
                {
                    // Play victory music
                    SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.VictoryMusic, musicCrossfadeTime);
                }
                else if (hadPreviousMusic)
                {
                    // No victory music, return to background music if it was playing before
                    PlayBackgroundMusic();
                }
                else
                {
                    Debug.Log("No victory music or background music to return to.");
                }
            }
        }
    }

    // Optional: Call this to manually toggle ambient sounds
    public void ToggleAmbientSounds(bool enable)
    {
        playAmbientSounds = enable;
        
        if (enable && !isBossFight)
        {
            StartAmbientSounds();
        }
        else
        {
            StopAmbientSounds();
        }
    }

    // Call this when player opens a door
    public void OnDoorOpen(Vector3 doorPosition)
    {
        if (levelSounds == null) return;
        
        // Check if door sound exists
        AudioClip doorSound = levelSounds.GetRandomClip(LevelSoundEvent.DoorOpen);
        if (doorSound != null)
        {
            // Play door sound at the door's position (3D sound)
            SoundManager.Instance.PlaySoundAtPoint(levelSounds, LevelSoundEvent.DoorOpen, doorPosition);
        }
    }

    // Call this when player finds a secret
    public void OnSecretFound()
    {
        if (levelSounds == null) return;
        
        AudioClip secretSound = levelSounds.GetRandomClip(LevelSoundEvent.SecretFound);
        if (secretSound != null)
        {
            SoundManager.Instance.PlaySound(levelSounds, LevelSoundEvent.SecretFound);
        }
    }

    // Call this when player opens a chest
    public void OnChestOpen(Vector3 chestPosition)
    {
        if (levelSounds == null) return;
        
        AudioClip chestSound = levelSounds.GetRandomClip(LevelSoundEvent.ChestOpen);
        if (chestSound != null)
        {
            SoundManager.Instance.PlaySoundAtPoint(levelSounds, LevelSoundEvent.ChestOpen, chestPosition);
        }
    }

    // Example: Change to cutscene music
    public void StartCutscene()
    {
        if (levelSounds == null) return;
        
        AudioClip cutsceneMusic = levelSounds.GetRandomClip(LevelSoundEvent.CutsceneMusic);
        if (cutsceneMusic != null)
        {
            // Stop ambient sounds during cutscene
            StopAmbientSounds();
            SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.CutsceneMusic, musicCrossfadeTime);
        }
    }

    // Example: Return to normal background music after cutscene
    public void EndCutscene()
    {
        // Resume ambient sounds after cutscene
        if (playAmbientSounds && !isBossFight)
        {
            StartAmbientSounds();
        }
        
        // Return to background music
        PlayBackgroundMusic();
    }

    private void OnDestroy()
    {
        // Optional: Stop music when leaving the level
        SoundManager.Instance.StopMusic(1f);
    }
}







// using UnityEngine;
//
// public class ExampleLevelManager : MonoBehaviour
// {
//     [Header("Level Sound Configuration")]
//     [SerializeField] private SoundDataSO levelSounds;
//
//     [Header("Music Settings")]
//     [SerializeField] private float musicCrossfadeTime = 2f;
//
//     private bool isBossFight = false;
//
//     private void Start()
//     {
//         // Start playing background music when level loads
//         PlayBackgroundMusic();
//         
//         // Start ambient sounds
//         PlayAmbientSounds();
//     }
//
//     private void PlayBackgroundMusic()
//     {
//         SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.BackgroundMusic, musicCrossfadeTime);
//     }
//
//     private void PlayAmbientSounds()
//     {
//         // Play ambient sounds on a loop or trigger them periodically
//         SoundManager.Instance.PlaySound(levelSounds, LevelSoundEvent.AmbientEnvironment);
//     }
//
//     // Call on boss fights
//     public void StartBossFight()
//     {
//         if (!isBossFight)
//         {
//             isBossFight = true;
//             // Crossfade from background music to boss music
//             SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.BossFightMusic, musicCrossfadeTime);
//         }
//     }
//
//     // Call this when boss is defeated
//     public void OnBossDefeated()
//     {
//         if (isBossFight)
//         {
//             isBossFight = false;
//             // Play victory music
//             SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.VictoryMusic, musicCrossfadeTime);
//         }
//     }
//
//     // Call this when player opens a door
//     public void OnDoorOpen(Vector3 doorPosition)
//     {
//         // Play door sound at the door's position (3D sound)
//         SoundManager.Instance.PlaySoundAtPoint(levelSounds, LevelSoundEvent.DoorOpen, doorPosition);
//     }
//
//     // Call this when player finds a secret
//     public void OnSecretFound()
//     {
//         SoundManager.Instance.PlaySound(levelSounds, LevelSoundEvent.SecretFound);
//     }
//
//     // Call this when player opens a chest
//     public void OnChestOpen(Vector3 chestPosition)
//     {
//         SoundManager.Instance.PlaySoundAtPoint(levelSounds, LevelSoundEvent.ChestOpen, chestPosition);
//     }
//
//     // Example: Change to cutscene music
//     public void StartCutscene()
//     {
//         SoundManager.Instance.PlayMusic(levelSounds, LevelSoundEvent.CutsceneMusic, musicCrossfadeTime);
//     }
//
//     // Example: Return to normal background music after cutscene
//     public void EndCutscene()
//     {
//         PlayBackgroundMusic();
//     }
//
//     private void OnDestroy()
//     {
//         // Optional: Stop music when leaving the level
//         SoundManager.Instance.StopMusic(1f);
//     }
// }
