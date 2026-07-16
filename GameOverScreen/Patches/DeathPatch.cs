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
    
    // This WILL generally throw an exception, causing DeathPart2 to be cut off early, hence SimulateRespawn below
    private static IEnumerator RestartFromCheckpointCoroutine()
    {
        c = 0;
        _timeCanShowGameOverScreenAgain = Time.time + 1.5f; // delay comes from CobraCharacter.DieThenTeleportBack
        _setAllCobraDead = true;
        yield return null;
        _setAllCobraDead = false;
        yield return null;
        if (CobraCharacter.Instance != null)
            CobraCharacter.Instance.isDieThenTeleportBackStarted = false;
        CameraController.Instance.SetOverrideZDelta(-1f, instant: false);
        yield return null;
        SimulateRespawn(CobraCharacter.Instance);
        c = 0;
    }

    // Simulates some of the behavior of DeathPart2
    private static void SimulateRespawn(CobraCharacter @this)
    {
        // isTokenDeath is a parameter in DeathPart2
        const bool isTokenDeath = false;
        
        if (@this == null)
        {
            Plugin.Logger.LogError("Cobra instance not found!");
            return;
        }
        
        TokenController.SetTokenValue(Token.HardCodedTokens.NbAllCobraDeath, 1, Token.ValueOperator.Add);
        CameraController.Instance.SetOverrideYDelta(0f);
        CameraController.Instance.SetOverrideZDelta(-1f, instant: false);
        @this.cameraController.UpdateWithSplineProperties(-1f, @this.gameObject, Vector3.up, isCameraDamp: false);
        @this.ResetTokensToLastCheckpointValue();
        if (LevelController.Instance != null)
        {
            LevelController.Instance.ChangeState(LevelController.State.Respawn, @this.isLogMain ? "DieThenTeleportBack" : null);
        }
        @this.AnimatorUpdate(1f, force: true);
        @this.AnimatorUpdate(1f, force: true);
        if (UIFade.Instance != null && !isTokenDeath)
        {
            UIFade.Instance.FadeIn(0.7f, 0.2f, @this.isLogMain ? "DieThenTeleportBack" : null);
        }
        /* Commented out to remove an annoying warning mainly
        if (isTokenDeath)
        {
            UIFade.Instance.ForceFadeIn(0.7f, 0.2f, @this.isLogMain ? "DieThenTeleportBack" : null);
        }
        */
    }
}