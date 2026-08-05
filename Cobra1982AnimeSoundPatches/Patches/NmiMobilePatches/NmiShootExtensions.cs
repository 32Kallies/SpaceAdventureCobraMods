using System.Runtime.CompilerServices;

namespace Cobra1982AnimeSoundPatches.Patches.NmiMobilePatches;

public static class NmiShootExtensions
{
    private static readonly ConditionalWeakTable<NmiShoot, NmiShootSound> ShootClipsBackingField = new();

    extension(NmiShoot instance)
    {
        public audioSelectionData.eCLIP ShootClip
        {
            get
            {
                if (!ShootClipsBackingField.TryGetValue(instance, out NmiShootSound sound))
                {
                    return audioSelectionData.eCLIP.NMI_SHOOT_LASER_LAUNCHED;
                }
                
                return sound.Clip;
            }
        }

        public void SetShootClip(NmiShootSound sound)
        {
            ShootClipsBackingField.AddOrUpdate(instance, sound);
        }
    }
}