using UnityEngine;

public class ExampleWeapon : MonoBehaviour
{
    [Header("Weapon Sound Configuration")]
    [SerializeField] private SoundDataSO weaponSounds;

    [Header("Weapon Settings")]
    [SerializeField] private int currentAmmo = 30;
    [SerializeField] private int maxAmmo = 30;

    private bool isCharging = false;

    // Call when weapon is equipped
    public void OnEquip()
    {
        SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.Draw);
    }

    // Call when weapon is unequipped
    public void OnUnequip()
    {
        SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.Sheathe);
    }

    // Call when attacking (light attack)
    public void OnLightAttack()
    {
        SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.SwingLight);
    }

    // Call when attacking (heavy attack)
    public void OnHeavyAttack()
    {
        SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.SwingHeavy);
    }

    // Call when weapon hits an enemy
    public void OnHit(bool isCritical = false)
    {
        if (isCritical)
        {
            SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.HitCritical);
        }
        else
        {
            SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.Hit);
        }
    }

    // Example: Ranged weapon shooting
    public void OnShoot()
    {
        if (currentAmmo > 0)
        {
            SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.Shoot);
            currentAmmo--;
        }
        else
        {
            // Play empty click sound
            SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.EmptyClick);
        }
    }

    // Example: Reload weapon
    public void OnReload()
    {
        SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.Reload);
        currentAmmo = maxAmmo;
    }

    // Example: Charging attack
    public void StartCharging()
    {
        if (!isCharging)
        {
            isCharging = true;
            SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.ChargeStart);
        }
    }

    public void ReleaseCharge()
    {
        if (isCharging)
        {
            isCharging = false;
            SoundManager.Instance.PlaySound(weaponSounds, WeaponSoundEvent.ChargeRelease);
        }
    }
}
