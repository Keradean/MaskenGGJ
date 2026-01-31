using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject that stores sound effects mapped to specific events - Supports multiple audio clips per event for variety.
/// </summary>
[CreateAssetMenu(fileName = "New SoundData SO", menuName = "SoundData")]
public class SoundDataSO : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        [Tooltip("The event that triggers this sound")]
        public string eventName;
        
        [Tooltip("Multiple clips can be added for variety - one will be randomly selected")]
        public AudioClip[] clips;

        [Tooltip("Volume multiplier for this specific sound (0-1)")]
        [Range(0.0f, 1.0f)]
        public float volumeMultiplier = 1.0f;
    }

    [SerializeField] private List<SoundEntry> soundEntries = new List<SoundEntry>();

    /// <summary>
    /// Gets a random clip for the given event - Returns null if no clips are found.
    /// </summary>
    public AudioClip GetRandomClip(System.Enum soundEvent)
    {
        return GetRandomClip(soundEvent.ToString());
    }

    /// <summary>
    /// Gets a random clip for the given event name.
    /// Returns null if no clips are found.
    /// </summary>
    public AudioClip GetRandomClip(string eventName)
    {
        SoundEntry entry = soundEntries.FirstOrDefault(e => e.eventName == eventName);
        
        if (entry == null || entry.clips == null || entry.clips.Length == 0)
        {
            return null;
        }

        // Filter out null clips
        AudioClip[] validClips = entry.clips.Where(c => c != null).ToArray();
        
        if (validClips.Length == 0)
        {
            return null;
        }

        // Return random clip
        return validClips[Random.Range(0, validClips.Length)];
    }

    /// <summary>
    /// Gets all clips for a given event.
    /// </summary>
    public AudioClip[] GetAllClips(System.Enum soundEvent)
    {
        return GetAllClips(soundEvent.ToString());
    }

    /// <summary>
    /// Gets all clips for a given event name.
    /// </summary>
    public AudioClip[] GetAllClips(string eventName)
    {
        SoundEntry entry = soundEntries.FirstOrDefault(e => e.eventName == eventName);
        return entry?.clips ?? new AudioClip[0];
    }

    /// <summary>
    /// Gets the volume multiplier for a specific event.
    /// </summary>
    public float GetVolumeMultiplier(System.Enum soundEvent)
    {
        SoundEntry entry = soundEntries.FirstOrDefault(e => e.eventName == soundEvent.ToString());
        return entry?.volumeMultiplier ?? 1f;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Helper method for the custom editor to populate entries from an enum.
    /// </summary>
    public void PopulateFromEnum(System.Type enumType)
    {
        if (!enumType.IsEnum)
        {
            Debug.LogError("Provided type is not an enum!");
            return;
        }

        string[] enumNames = System.Enum.GetNames(enumType);
        
        foreach (string enumName in enumNames)
        {
            // Check if entry already exists
            if (!soundEntries.Any(e => e.eventName == enumName))
            {
                soundEntries.Add(new SoundEntry { eventName = enumName });
            }
        }

        // Sort entries alphabetically for easier management
        soundEntries = soundEntries.OrderBy(e => e.eventName).ToList();
    }
#endif
}
