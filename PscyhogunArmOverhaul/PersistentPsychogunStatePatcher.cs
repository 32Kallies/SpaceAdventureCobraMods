using HarmonyLib;
using UnityEngine;

namespace PscyhogunArmOverhaul;

[HarmonyPatch]
public static class PersistentPsychogunStatePatcher
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    private static void CreateStateRememberer(LevelController __instance)
    {
        __instance.gameObject.AddComponent<PsychogunStateRememberer>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.OnLevelQuitFromPause))]
    private static void OnQuit()
    {
        var state = PsychogunStateRememberer.GetInstance(true);
        if (state != null)
        {
            state.SetToken(Token.HardCodedTokens.ForcePsychogunOn, false);
            state.SetToken(Token.HardCodedTokens.ForcePsychogunOff, false);
        }
    }
}