using System.Collections.Generic;

namespace Cobra1982AnimeSoundPatches.Patches.NmiMobilePatches;

public static class NmiShootSoundDatabase
{
    private static bool _fetchedSounds;

    private static Dictionary<NmiShoot.WeaponType, NmiShootSound> _soundsByWeaponType;

    public static NmiShootSound Default { get; } = new(audioSelectionData.eCLIP.NMI_SHOOT_LASER_LAUNCHED);
    
    private static void FetchSoundsWithDefaults()
    {
        _soundsByWeaponType = new Dictionary<NmiShoot.WeaponType, NmiShootSound>();
        _fetchedSounds = true;
        
        // DEFAULTS
        TryFetchSound(NmiShoot.WeaponType.Drone, "new_drone_shot_sound");
    }

    private static void TryFetchSound(NmiShoot.WeaponType weaponType, string soundName)
    {
        if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip(soundName, out var newDroneSound))
        {
            _soundsByWeaponType[weaponType] = new NmiShootSound(newDroneSound);
        }
        else
        {
            Plugin.Logger.LogError($"Failed to get custom eClip by name: '{soundName}'");
        }
    }

    public static bool TryGetNmiShootSoundForWeaponType(NmiShoot.WeaponType type, out NmiShootSound sound)
    {
        if (!_fetchedSounds) FetchSoundsWithDefaults();
        return _soundsByWeaponType.TryGetValue(type, out sound);
    }
}