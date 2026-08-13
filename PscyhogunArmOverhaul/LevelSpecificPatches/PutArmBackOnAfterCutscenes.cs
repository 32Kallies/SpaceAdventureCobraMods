using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace PscyhogunArmOverhaul.LevelSpecificPatches;

[HarmonyPatch]
public class PutArmBackOnAfterCutscenes
{
    private const string Level1Episode2AmbushVideo = "CS2D_V_1_2_3";
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraVideoPlayer), nameof(CobraVideoPlayer.Stop))]
    private static void PutArmBackOnAfterSecondCutsceneOnEp1Lvl2(CobraVideoPlayer __instance)
    {
        if (__instance.videoName.Equals(Level1Episode2AmbushVideo, StringComparison.OrdinalIgnoreCase))
        {
            Plugin.StartCoroutineOnPlugin(PutArmBackOnAfterCutsceneCoroutine(1));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiZigoba), nameof(NmiZigoba.Die))]
    private static void PutArmOnAfterKillingZigova()
    {
        var armBehavior = NewArmBehaviour.Instance;
        if (armBehavior != null)
        {
            NewArmBehaviour.Instance.PutArmOnInstantly();
        }
    }
    
    private static IEnumerator PutArmBackOnAfterCutsceneCoroutine(float delay)
    {
        Plugin.Logger.LogInfo("Forcing Cobra to put arm back on");
        yield return new WaitForSeconds(delay);
        var armBehavior = NewArmBehaviour.Instance;
        if (armBehavior == null)
        {
            Plugin.Logger.LogError("Couldn't find arm behavior");
            yield break;
        }
        armBehavior.PutOnArmForDialogue();
    }
}