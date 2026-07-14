using System.Collections;
using GameOverScreen.UI;
using HarmonyLib;
using UnityEngine;

namespace GameOverScreen.Patches;

[HarmonyPatch]
public static class DeathPatch
{
    private static bool _setAllCobraDead;
    private static float _timeCanShowGameOverScreenAgain;

    private static int c;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    public static void StartPostfix()
    {
        c = 0;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.areAllCobraDead))]
    public static void Postfix(CobraCharacter __instance, ref bool __result)
    {
        // If multiplayer, use normal behavior
        if (Utilities.IsMultiplayer())
        {
            return;
        }

        if (Time.time > _timeCanShowGameOverScreenAgain)
        {
            GameOverScreenBuilder.ShowScreen();
        }
        
        if (c < 3)
            __result = _setAllCobraDead;
        c++;
    }

    public static void RestartFromCheckpoint()
    {
        Plugin.RunCoroutineOnPlugin(RestartFromCheckpointCoroutine());
    }
    
    private static IEnumerator RestartFromCheckpointCoroutine()
    {
        c = 0;
        _timeCanShowGameOverScreenAgain = Time.time + 0.5f;
        _setAllCobraDead = true;
        yield return null;
        _setAllCobraDead = false;
    }
}