using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PscyhogunArmOverhaul;

[HarmonyPatch]
public static class FixEpisode1Level2Patch
{
    private const string VideoNameToPutArmBackOn = "CS2D_V_1_2_3";

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    public static void PatchLevel(LevelController __instance)
    {
        // This patch works for level 1-2 only
        if (__instance.level != LevelController.Level.EP01_LVL02_Casino_BossVaiken)
        {
            return;
        }

        // If level 1-2:
        var allTriggers = Object.FindObjectsOfType<GenericTrigger>(true);
        foreach (GenericTrigger trigger in allTriggers)
        {
            if (trigger == null) continue;
            // if (!trigger.gameObject.name.Equals("NormalWalkTrigger", StringComparison.OrdinalIgnoreCase)) continue;
            int matches = 0;
            foreach (var parameter in trigger.listParameter)
            {
                if (parameter.type == GenericTrigger.Type.TokenSetValueOperator &&
                    parameter.comment == "ForceProtheseOff")
                {
                    parameter.type = GenericTrigger.Type.None;
                    matches++;
                }
                if (parameter.type == GenericTrigger.Type.TokenSetValueOperator &&
                    parameter.comment == "ForceProtheseOn")
                {
                    parameter.type = GenericTrigger.Type.None;
                    matches++;
                }
            }

            if (matches > 0)
            {
                Plugin.Logger.LogInfo(
                    $"Patched {matches} token sets on level 1-2 to disable the psychogun enablement trigger");   
            }
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraVideoPlayer), nameof(CobraVideoPlayer.Stop))]
    private static void PutArmBackOnAfterSecondCutsceneOnEp1Lvl2(CobraVideoPlayer __instance)
    {
        if (!__instance.videoName.Equals(VideoNameToPutArmBackOn, StringComparison.OrdinalIgnoreCase))
            return;

        Plugin.StartCoroutineOnPlugin(PutArmBackOnAfterSecondCutsceneOnEp1Lvl2Coroutine());
    }

    private static IEnumerator PutArmBackOnAfterSecondCutsceneOnEp1Lvl2Coroutine()
    {
        Plugin.Logger.LogInfo("Forcing Cobra to put arm back on");
        yield return new WaitForSeconds(1);
        var armBehavior = NewArmBehaviour.Instance;
        if (armBehavior == null)
        {
            Plugin.Logger.LogError("Failed to find arm behavior");
            yield break;
        }
        armBehavior.PutOnArmForDialogue();
    }
}