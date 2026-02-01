using UnityEngine;

// ==================================================
// WEAPONS MANAGER CLASS
// ==================================================
// This class manages all weapon-related functionality in the game
// It handles: shooting, reloading, weapon switching, ammo management, and visual effects
// This is the central system that connects weapon data (from Weapon.cs) with gameplay logic
//
// RESPONSIBILITIES:
// - Shooting mechanics (raycasting to hit enemies)
// - Automatic vs semi-automatic fire
// - Ammunition tracking and reloading
// - Weapon switching (cycling through multiple weapons)
// - Visual effects (muzzle flash, impact effects)
// - UI updates (ammo counter, ammo bar)
public class WeaponsManager : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - WEAPON STATISTICS
    // ==================================================

    // Maximum shooting range of the current weapon (in Unity units)
    public float Range;

    // Reference to the camera transform 
    // Weapons shoot in the direction the camera is looking
    public Transform Cam;

    // LayerMask that determines which objects can be hit by bullets
    // Example: only hit enemies and environment, not UI or player
    public LayerMask ValidLayers;

    // Visual effect prefabs for bullet impacts
    public GameObject ImpactEffect; // Effect for hitting non-damageable objects (walls, ground, etc.)
    public GameObject DamageEffect; // Effect for hitting damageable objects (enemies, destructibles)

    // Reference to the active muzzle flash effect GameObject
    //public GameObject MuzzleFlare;

    // How long the muzzle flash stays visible (in seconds)
    public float FlareDisplayTime;

    // Timer that counts down the muzzle flash display time
    private float FlareCounter;

    // ==================================================
    // VARIABLE DECLARATION - FIRE RATE CONTROL
    // ==================================================

    // Is this weapon automatic (holds trigger = continuous fire)?
    // true = automatic (machine gun), false = semi-automatic (pistol)
    public bool AutoFire;

    // Delay between shots (fire rate) in seconds
    // Example: 0.1f = 10 shots per second
    public float TimeBtwShots;

    // Timer that counts down between shots (prevents shooting too fast)
    private float ShotCounter;

    // ==================================================
    // VARIABLE DECLARATION - AMMUNITION SYSTEM
    // ==================================================

    // Current ammunition in the magazine/clip (ready to shoot)
    public int CurrentAmmo;

    // Maximum capacity of the magazine/clip
    public int ClipSize;

    // Total reserve ammunition available for reloading
    public int RemainingAmmo;

    // Amount of ammo gained when picking up ammo packs
    public int pickUpValue;

    // Damage dealt per bullet hit
    public float damage;

    // ==================================================
    // VARIABLE DECLARATION - WEAPON SWITCHING
    // ==================================================

    // Array of all available weapons the player can use
    public Weapon[] Weapons;

    // Reference to UIManager for updating ammo displays
    public UIManager UIManager;

    // Index of currently equipped weapon in the Weapons array
    // Index of previously equipped weapon (for saving ammo state)
    private int CurrentWeapon, previouWeapons;

    // ================= RANGED STATE =================
    private bool isRangedWeapon = false;
    private GameObject projectilePrefab;
    private float projectileSpeed = 20f;
    private float projectileLifeTime = 5f;

    // ================= MELEE STATE =================
    private bool isMeleeWeapon = false;
    private float meleeRange = 2f;
    private float meleeRadius = 1f;
    private float meleeDamage = 25f;
    private float meleeCooldown = 1f;
    private float meleeCooldownTimer = 0f;
    private GameObject meleeEffectPrefab;

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called when the script instance is being loaded (before Start)
    // Used for initialization before the game starts
    void Awake()
    {
        // If UIManager reference is not assigned, find it in the scene
        if (UIManager == null)
            UIManager = FindFirstObjectByType<UIManager>();

        // Equip the first weapon (index 0) at game start
        if (Weapons.Length > 0) SetWeapon(0);
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Handles timers and continuous updates
    void Update()
    {
        // ==================================================
        // MUZZLE FLASH TIMER
        // ==================================================
        // Count down the muzzle flash display timer
        if (FlareCounter > 0)
        {
            FlareCounter -= Time.deltaTime; // Decrease timer by frame time
            /*
            // When timer reaches 0, hide the muzzle flash
            if (FlareCounter <= 0 && MuzzleFlare != null)
            {
                MuzzleFlare.SetActive(false); // Disable the muzzle flash GameObject
            }
            */
        }

        // ==================================================
        // FIRE RATE TIMER
        // ==================================================
        // Count down the shot cooldown timer (time between shots)
        if (ShotCounter > 0)
            ShotCounter -= Time.deltaTime; // Decrease timer by frame time

        // ==================================================
        // MELEE TIMER
        // ==================================================
        // Count down the melee cooldown timer (time between melee attacks)
        if (meleeCooldownTimer > 0f)
            meleeCooldownTimer -= Time.deltaTime;

        // Update the ammo display in the UI every frame
       // UpdateAmmoUI();
    }

    // ==================================================
    // SHOOT METHOD
    // ==================================================
    // This public method handles a single shot
    // Called when player presses fire button (for semi-automatic weapons)
    // Also called by ShootHeld() for automatic weapons
    public void Shoot()
    {
        // Priorität: Ranged → Melee → default Shoot
        if (isRangedWeapon)
        {
            RangedShoot();
            return;
        }
    
        if (isMeleeWeapon)
        {
            MeleeAttack();
            return;
        }

        // fallback: existierender Shoot-Code
        if (CurrentAmmo > 0 && ShotCounter <= 0f)
        {
            RaycastHit hit;
            if (Physics.SphereCast(Cam.position, 0.5f, Cam.forward, out hit, Range, ValidLayers))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage);
                        Instantiate(DamageEffect, hit.point, Quaternion.identity);
                    }
                    else
                    {
                        Instantiate(ImpactEffect, hit.point, Quaternion.identity);
                    }
                }
                else
                {
                    Instantiate(ImpactEffect, hit.point, Quaternion.identity);
                }
            }

           // if (MuzzleFlare != null) MuzzleFlare.SetActive(true);
            FlareCounter = FlareDisplayTime;
            CurrentAmmo--;
            //UpdateAmmoUI();
            ShotCounter = TimeBtwShots;
        }
    }

    private void RangedShoot()
    {
        // Munition prüfen (falls du Munition verwenden willst)
        if (CurrentAmmo <= 0) return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("projectilePrefab fehlt für die aktuelle Waffe.");
            return;
        }

        // Spawnposition: kurz vor Kamera (oder ein Muzzle-Transform, falls vorhanden)
        Vector3 spawnPos = Cam.position + Cam.forward * 0.5f;
        Quaternion spawnRot = Quaternion.LookRotation(Cam.forward);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, spawnRot);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Cam.forward * projectileSpeed;
        }

        // Init Projectile (falls Script vorhanden)
        var ink = proj.GetComponent<InkProjectile>();
        if (ink != null)
        {
            ink.Init(projectileLifeTime, damage);
        }

        //if (MuzzleFlare != null) MuzzleFlare.SetActive(true);
        FlareCounter = FlareDisplayTime;

        CurrentAmmo--;
       // UpdateAmmoUI();
        ShotCounter = TimeBtwShots;
    }

    // ==================================================
    // SHOOT HELD METHOD
    // ==================================================
    // This public method handles automatic fire when holding the trigger
    // Called every frame when fire button is held down
    public void ShootHeld()
    {
        // Only proceed if this weapon is automatic
        if (AutoFire == true)
        {
            // Count down the shot timer
            ShotCounter -= Time.deltaTime;

            // When timer reaches 0, shoot and reset timer
            if (ShotCounter <= 0f)
            {
                Shoot(); // Fire a shot (which will reset ShotCounter)
            }
        }
    }

    // ==================================================
    // RELOAD METHOD
    // ==================================================
    // This public method handles weapon reloading
    // Called when player presses reload button
    public void Reload()
    {
        // Don't reload if: already full OR no reserve ammo
        if (CurrentAmmo >= ClipSize || RemainingAmmo <= 0) return;

        // Print reload message to console (for debugging)
        Debug.Log("Lad nach!!");

        // Return current ammo to reserve pool (simulate removing partially-full magazine)
        RemainingAmmo += CurrentAmmo;

        // Check if we have enough reserve ammo to fill a full clip
        if (RemainingAmmo >= ClipSize)
        {
            // Fill the clip completely
            CurrentAmmo = ClipSize;
            // Subtract clip size from reserve
            RemainingAmmo -= ClipSize;
        }
        else // Not enough ammo for a full clip
        {
            // Load all remaining ammo into the clip
            CurrentAmmo = RemainingAmmo;
            // Reserve is now empty
            RemainingAmmo = 0;
        }

       // UpdateAmmoUI();
    }

    // ==================================================
    // ADD AMMO METHOD
    // ==================================================
    // This public method adds ammunition to reserve
    // Called when player picks up ammo packs
    //
    // PARAMETERS:
    // int pickUpValue - Amount of ammo to add to reserve
    public void AddAmmo(int pickUpValue)
    {
        // Add ammo to reserve pool
        RemainingAmmo += pickUpValue;
    }

    // ==================================================
    // SET WEAPON METHOD
    // ==================================================
    // This public method switches to a specific weapon
    // Saves current weapon's ammo state and loads new weapon's data
    //
    // PARAMETERS:
    // int weaponToSet - Index of weapon in Weapons array to equip
    public void SetWeapon(int weaponToSet)
    {
        // Make sure weapons array is not empty and index is valid
        if (Weapons == null || Weapons.Length == 0) return;
        if (weaponToSet < 0 || weaponToSet >= Weapons.Length) return;

        // Save current weapon's ammo state before switching
        // Only if we're actually switching weapons (not initial setup)
        if (previouWeapons != CurrentWeapon && Weapons.Length > 0)
        {
            // Save current ammo to the previous weapon's data
            Weapons[previouWeapons].CurrentAmmo = CurrentAmmo;
            Weapons[previouWeapons].RemainingAmmo = RemainingAmmo;
        }

        // ==================================================
        // LOAD NEW WEAPON'S DATA
        // ==================================================
        // Copy all stats from the new weapon to active variables
        var w = Weapons[weaponToSet];

        // Kopiere Ranged-Felder
        isRangedWeapon = w.IsRanged;
        projectilePrefab = w.projectilePrefab;
        projectileSpeed = w.projectileSpeed;
        projectileLifeTime = w.projectileLifeTime;

        // Kopiere Ranged/Ranged-Defaults in aktive Werte
        Range = w.Range;
        FlareDisplayTime = w.FlareDisplayTime;
        AutoFire = w.AutoFire;
        TimeBtwShots = w.TimeBtwShots;
        CurrentAmmo = w.CurrentAmmo;
        ClipSize = w.ClipSize;
        RemainingAmmo = w.RemainingAmmo;
        pickUpValue = w.pickUpValue;
        damage = w.damage;
        //MuzzleFlare = w.MuzzleFlare;

        // Kopiere Melee-Felder
        isMeleeWeapon = w.IsMelee;
        meleeRange = w.MeleeRange;
        meleeRadius = w.MeleeRadius;
        meleeDamage = w.MeleeDamage;
        meleeCooldown = w.MeleeCooldown;
        //meleeEffectPrefab = w.MeleeEffect;
        meleeCooldownTimer = 0f;

        // ==================================================
        // UPDATE VISUAL WEAPON MODELS
        // ==================================================
        // Hide all weapon GameObjects first
        foreach (Weapon ww in Weapons)
        {
            ww.gameObject.SetActive(false); // Disable each weapon's 3D model
        }

        // Show only the newly equipped weapon's GameObject
        Weapons[weaponToSet].gameObject.SetActive(true);

        // Update UI to show new weapon's ammo
        //UpdateAmmoUI();

        // Remember this weapon as the previous weapon for next switch
        previouWeapons = CurrentWeapon;
    }

    // ==================================================
    // MELEE IMPLEMENTATION
    // ==================================================
    // Aufruf z.B. aus Input: weaponsManager.MeleeAttack()
    public void MeleeAttack()
    {
        // Nur ausführen wenn es eine Nahkampfwaffe ist
        if (!isMeleeWeapon) return;

        // Prüfen ob Cooldown abgelaufen ist
        if (meleeCooldownTimer > 0f) return;

        // Zielpunkt vor Kamera
        Vector3 center = Cam.position + Cam.forward * meleeRange;

        // Trefferbereich prüfen
        Collider[] hits = Physics.OverlapSphere(center, meleeRadius, ValidLayers);
        bool hitSomething = false;

        foreach (var col in hits)
        {
            if (col == null) continue;
            var go = col.transform;
            if (go.CompareTag("Enemy"))
            {
                IDamageable dmg = go.GetComponent<IDamageable>();
                if (dmg != null)
                {
                    dmg.TakeDamage(meleeDamage);
                    hitSomething = true;
                    
                    // Spawn effekt am Kollisionspunkt (falls vorhanden)
                    Vector3 hitPoint = col.ClosestPoint(center);
                    if (meleeEffectPrefab != null)
                        Instantiate(meleeEffectPrefab, hitPoint, Quaternion.identity);
                    
                }
            }
        }
        
        // Optional: Feedback wenn nichts getroffen wurde (sound, animation)
        if (!hitSomething)
        {
            // spawn a small impact at center for feedback (optional)
            if (meleeEffectPrefab != null)
                Instantiate(meleeEffectPrefab, center, Quaternion.identity);
        }
        
        // set cooldown
        meleeCooldownTimer = meleeCooldown;
    }

    // ==================================================
    // UPDATE AMMO UI METHOD
    // ==================================================
    // This public method updates the ammo display in the UI
    // Shows current ammo / reserve ammo and updates the ammo bar fill
    /*
    public void UpdateAmmoUI()
    {
        // Only update if UIManager exists
        if (UIManager != null)
        {
            // Update ammo text (e.g. "15 / 120")
           // UIManager.ammoTMP.text = $"{CurrentAmmo} / {RemainingAmmo}";

            // Update ammo bar fill amount (0.0 to 1.0)
            if (ClipSize > 0) // Prevent division by zero
                // Cast to float for proper division (15/30 = 0.5 = 50% filled)
                UIManager.ammoBar.fillAmount = (float)CurrentAmmo / (float)ClipSize;
            else
                UIManager.ammoBar.fillAmount = 0; // Empty bar if no clip size
        }
    }
    */
    // ==================================================
    // NEXT WEAPON METHOD
    // ==================================================
    // This public method switches to the next weapon in the array
    // Called when player presses "next weapon" button (e.g. mouse wheel up)
    
    /*
    public void NextWeapon()
    {
        // Increment weapon index
        CurrentWeapon++;

        // If we've gone past the last weapon, loop back to first weapon
        if (CurrentWeapon >= Weapons.Length)
        {
            CurrentWeapon = 0; // Reset to index 0 (first weapon)
        }

        // Equip the new weapon
        SetWeapon(CurrentWeapon);
    }

    // ==================================================
    // PREVIOUS WEAPON METHOD
    // ==================================================
    // This public method switches to the previous weapon in the array
    // Called when player presses "previous weapon" button (e.g. mouse wheel down)
    public void PreviousWeapon()
    {
        // Decrement weapon index
        CurrentWeapon--;

        // If we've gone below the first weapon, loop to last weapon
        if (CurrentWeapon < 0)
        {
            CurrentWeapon = Weapons.Length - 1; // Set to last index in array
        }

        // Equip the new weapon
        SetWeapon(CurrentWeapon);
    }
    */
}