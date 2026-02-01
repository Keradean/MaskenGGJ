/// <summary>
/// Sound events for character and enemy actions.
/// </summary>
public enum CharacterSoundEvent
{
    Idle,
    Footstep,
    Jump,
    Land,
    AttackMelee,
    AttackRanged,
    TakeDamage,
    Death,
    SpawnAppear,
    VoiceGrunt,
    VoiceTaunt,
    VoiceCelebrate,
    SkillCast,
    SkillCharge,
    Dodge,
    Block,
    Parry
}

/// <summary>
/// Sound events for level/environment audio.
/// </summary>
public enum LevelSoundEvent
{
    BackgroundMusic,
    BossFightMusic,
    VictoryMusic,
    AmbientEnvironment,
    AmbientWind,
    AmbientWater,
    AmbientFire,
    DoorOpen,
    DoorClose,
    LeverPull,
    ChestOpen,
    SecretFound,
    TrapActivate,
    CutsceneMusic,
    MenuMusic
}

/// <summary>
/// Sound events specific to the player character.
/// </summary>
public enum PlayerSoundEvent
{
    Footstep,
    Jump,
    Land,
    TakeDamage,
    Death,
    Heal,
    LevelUp,
    InventoryOpen,
    InventoryClose,
    ItemPickup,
    ItemDrop,
    QuestComplete,
    UIClick,
    UIHover,
    UIError,
    Dash,
    Crouch,
    Uncrouch
}

/// <summary>
/// Sound events for weapons, tools and combat.
/// </summary>
public enum WeaponSoundEvent
{
    Draw,
    Sheathe,
    SwingLight,
    SwingHeavy,
    Hit,
    HitCritical,
    Miss,
    Block,
    Parry,
    Shoot,
    Reload,
    EmptyClick,
    ChargeStart,
    ChargeLoop,
    ChargeRelease,
    WeaponBreak,
    SpecialAttack
}

/// <summary>
/// General UI sound events that might be used across the game.
/// </summary>
public enum UISoundEvent
{
    Click,
    Hover,
    Back,
    Confirm,
    Cancel,
    Error,
    Success,
    WindowOpen,
    WindowClose,
    TabSwitch,
    SliderMove,
    Toggle,
    Notification
}