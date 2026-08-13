using System;
using Cobra1982AnimeSoundPatches.Utility;
using HarmonyLib;

namespace Cobra1982AnimeSoundPatches.Patches.Bosses;

[HarmonyPatch]
public static class ZigovaDisableMusicPatch
{
    private const string VideoNameToDisableMusic = "CS2D_V_2_3_2";
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraVideoPlayer), nameof(CobraVideoPlayer.Stop))]
    private static void OnVideoEnd(CobraVideoPlayer __instance)
    {
        if (!__instance.videoName.Equals(VideoNameToDisableMusic, StringComparison.OrdinalIgnoreCase))
            return;

        DisableMusicTriggerUtils.DisableMusicTriggersAroundCobra();
    }
}