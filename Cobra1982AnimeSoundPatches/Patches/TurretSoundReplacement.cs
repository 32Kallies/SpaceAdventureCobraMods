using CobraSoundReplacer.API;
using HarmonyLib;
using JetBrains.Annotations;

namespace Cobra1982AnimeSoundPatches.Patches;

[HarmonyPatch(typeof(NmiTurret), nameof(NmiTurret.Start))]
public static class TurretSoundReplacement
{
    [UsedImplicitly]
    private static void Prefix(NmiTurret __instance)
    {
        if (CustomSoundUtils.TryGetEClip("new_drone_shot_sound", out var clip))
        {
            __instance.specificShootProjectileClip.EnumValue = clip;
            __instance.m_SpecificShootProjectileClip = clip;
        }
        else
        {
            Plugin.Logger.LogError("Failed to find 'new_drone_shot_sound' eClip for replacing turret sounds");
        }
    }
}