using System;
using HarmonyLib;

namespace Cobra1982AnimeSoundPatches.Patches;

[HarmonyPatch]
public static class SoundReplacementPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiPatrouille), nameof(NmiPatrouille.Start))]
    public static void ReplacePatrouilleSound(NmiPatrouille __instance)
    {
        audioSelectionData.eCLIP shootClip = __instance.m_SpecificShootProjectileClip;
        
        if (shootClip == audioSelectionData.eCLIP.NONE)
            return;

        if (!SoundReplacementData.Enemies.Replacements.TryGetValue(shootClip,
                out SoundReplacementData.SoundClipReplacements clipReplacements)) return;
        
        foreach (SoundReplacementData.PrefixReplacement replacement in clipReplacements.Replacements)
        {
            if (!__instance.gameObject.name.StartsWith(replacement.NamePrefix, StringComparison.OrdinalIgnoreCase))
                continue;
            
            if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip(replacement.NewCustomEClipName, out var eClip))
                __instance.m_SpecificShootProjectileClip = eClip;
            else
                Plugin.Logger.LogError($"Failed to find replacement EClip by name {replacement.NewCustomEClipName} on object {replacement.NamePrefix}");
        }
    }
}