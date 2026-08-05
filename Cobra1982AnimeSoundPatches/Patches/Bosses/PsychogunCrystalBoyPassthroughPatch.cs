using Cobra1982AnimeSoundPatches.Behaviours;
using HarmonyLib;
using UnityEngine;

namespace Cobra1982AnimeSoundPatches.Patches.Bosses;

[HarmonyPatch]
public static class PsychogunCrystalBoyPassthroughPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    private static void StartPostfix(CobraCharacter __instance)
    {
        var levelController = LevelController.Instance;
        if (levelController == null || levelController.level != LevelController.Level.EP08_LVL02_BossCrystalBowie)
            return;
        
        Plugin.Logger.LogInfo("Modifying psychogun to add Crystal Boy passthrough sound");

        if (!CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip("crystal_boy_psychogun_passthrough", out var clip))
        {
            Plugin.Logger.LogError("Failed to find Crystal Boy Psychogun passthrough sound");
            return;
        }
        
        TryAddPassThroughSoundComponent(__instance.dependencies.chargedShot, 1.2f, clip);
        TryAddPassThroughSoundComponent(__instance.dependencies.psychoShot, 0.8f, clip);
    }

    private static void TryAddPassThroughSoundComponent(GameObject projectile, float radius, audioSelectionData.eCLIP clip)
    {
        if (projectile.GetComponent<CrystalBoyPassThroughSoundPlayer>() == null)
        {
            var player = projectile.AddComponent<CrystalBoyPassThroughSoundPlayer>();
            player.radius = radius;
            player.clip = clip;
        }
    }
}