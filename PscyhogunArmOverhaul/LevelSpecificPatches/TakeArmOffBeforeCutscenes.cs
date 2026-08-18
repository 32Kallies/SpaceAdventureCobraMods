using System;
using HarmonyLib;

namespace PscyhogunArmOverhaul.LevelSpecificPatches;

[HarmonyPatch]
public static class TakeArmOffBeforeCutscenes
{
    private const string ZigovaIntroSequenceAssetName = "timeline_C2_3_2_Zig_Intro_sequence_V2";
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CutscenePlayer), nameof(CutscenePlayer.StartPlaying))]
    private static void OnStartPlayingPatch(CutscenePlayer __instance)
    {
        if (__instance.m_PlayableDirector == null) return;
        var asset = __instance.m_PlayableDirector.playableAsset;
        if (asset == null) return;

        if (string.Equals(asset.name, ZigovaIntroSequenceAssetName, StringComparison.OrdinalIgnoreCase))
        {
            TakeOffArm();
        }
    }

    private static void TakeOffArm()
    {
        var armBehavior = NewArmBehaviour.Instance;
        if (armBehavior != null)
        {
            NewArmBehaviour.Instance.TakeOffArm();
        }
    }
}