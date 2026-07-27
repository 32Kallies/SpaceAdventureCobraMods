using System;
using HarmonyLib;
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

            Plugin.Logger.LogInfo(
                $"Patched {matches} token sets on level 1-2 to disable the psychogun enablement trigger");
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraVideoPlayer), nameof(CobraVideoPlayer.Stop))]
    private static void PutArmBackOnAfterSecondCutsceneOnEp1Lvl2(CobraVideoPlayer __instance)
    {
        if (!__instance.videoName.Equals(VideoNameToPutArmBackOn, StringComparison.OrdinalIgnoreCase))
            return;
        
        Plugin.Logger.LogInfo("Forcing Cobra to put arm back on");
        var state = PsychogunStateRememberer.GetInstance(true);
        if (state != null)
        {
            state.SetToken(Token.HardCodedTokens.ForcePsychogunOff, true);
            state.SetToken(Token.HardCodedTokens.ForcePsychogunOn, false);
        }
    }
}