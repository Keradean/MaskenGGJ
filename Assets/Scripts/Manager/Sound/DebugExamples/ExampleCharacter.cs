using UnityEngine;

public class ExampleCharacter : MonoBehaviour
{
    [Header("Sound Configuration")]
    [SerializeField] private SoundDataSO characterSounds;

    private void Start()
    {
        // Play spawn sound when character appears
        if (characterSounds != null)
        {
            SoundManager.Instance.PlaySound(characterSounds, CharacterSoundEvent.SpawnAppear);
        }
    }

    // Call this from your attack animation event or attack method
    public void OnAttackMelee()
    {
        SoundManager.Instance.PlaySound(characterSounds, CharacterSoundEvent.AttackMelee);
    }

    // Call this when character takes damage
    public void OnTakeDamage()
    {
        SoundManager.Instance.PlaySound(characterSounds, CharacterSoundEvent.TakeDamage);
    }

    // Call this when character dies
    public void OnDeath()
    {
        SoundManager.Instance.PlaySound(characterSounds, CharacterSoundEvent.Death);
    }

    // Play footstep sounds at character's position (3D sound)
    public void OnFootstep()
    {
        SoundManager.Instance.PlaySound(characterSounds, CharacterSoundEvent.Footstep, transform.position);
    }

    // Example: Play a ranged attack sound
    public void OnAttackRanged()
    {
        SoundManager.Instance.PlaySound(characterSounds, CharacterSoundEvent.AttackRanged);
    }
}
