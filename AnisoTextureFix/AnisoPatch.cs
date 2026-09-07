using HarmonyLib;
using UnityEngine;

namespace AnisoTextureFix;

[HarmonyPatch]
public static class AnisoPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIMainMenu), nameof(NUIMainMenu.Start))]
    private static void UpdateQualitySettings()
    {
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        Plugin.Logger.LogDebug("Forced anisotropic filtering on");
    }
}