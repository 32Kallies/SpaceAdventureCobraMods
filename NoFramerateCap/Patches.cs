using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NoFramerateCap;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FrameRateLimiter), nameof(FrameRateLimiter.Start))]
    public static void OverrideFramerateCapPatch(FrameRateLimiter __instance)
    {
        int framerate = CustomFramerateUtils.GetNewFramerateInt();
        __instance.targetFrameRate = framerate;
        UpdateFixedDeltaTime(framerate);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(QualityController), nameof(QualityController.Update))]
    public static void OverrideFrameRateCapInUpdate()
    {
        int framerate = CustomFramerateUtils.GetNewFramerateInt();
        Application.targetFrameRate = framerate;
        if (Plugin.DisableVsync.Value)
            QualitySettings.vSyncCount = 0;
        UpdateFixedDeltaTime(framerate);
    }

    private static void UpdateFixedDeltaTime(float targetFrameRate)
    {
        Time.fixedDeltaTime = 1f / Mathf.Clamp(targetFrameRate, 60, 240);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CFrame), nameof(CFrame.reset))]
    public static void CFrameResetPostfix()
    {
        CFrame.fps = (ushort)CustomFramerateUtils.GetNewFramerateInt();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ProjectileRocket), nameof(ProjectileRocket.RotateTowards))]
    public static bool RotateTowardsReplacement(ProjectileRocket __instance, in Vector3 direction, in Vector3 idealDirection, in float degPerFrame, in float frameDuration, ref Vector3 __result)
    {
        __result = Vector3.RotateTowards(direction, idealDirection, degPerFrame * frameDuration * (MathF.PI / 180f) * CustomFramerateUtils.GetNewFramerateFloat() / 60, 0f);
        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CutscenePlayer), nameof(CutscenePlayer.Start))]
    public static void FixZigobaBossTriggerForAfterCutscene(CutscenePlayer __instance)
    {
        if (LevelController.Instance == null ||
            LevelController.Instance.level != LevelController.Level.EP02_LVL03_ZigobaBase_BossZigoba)
            return;
        
        if (!__instance.gameObject.name.Contains("Prefab_CS_Sequence_CobraArrivalLeft",
                StringComparison.OrdinalIgnoreCase)) return;
        
        if (__instance.tokensToSetOnEnd == null)
            __instance.tokensToSetOnEnd = new List<TokenToSet>();
        
        var tokens = __instance.tokensToSetOnEnd;
        
        if (tokens.Count == 0)
        {
            tokens.Add(GetZigobaTriggerEnablementToken());
            return;
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].token is { id: 0, type: Token.TokenType.System })
            {
                tokens[i] = GetZigobaTriggerEnablementToken();
                return;
            }
        }

        TokenToSet GetZigobaTriggerEnablementToken()
        {
            Plugin.Logger.LogMessage("Fixing Zigoba fight trigger");
            return new TokenToSet(Token.TokenType.Level, 53, 1);
        }
    }
}

[HarmonyPatch(typeof(NmiPatrouille), nameof(NmiPatrouille.Update))]
public static class NmiPatrouilleTimeDependencePatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TranspilerUtils.ReplaceSixty("Nmi patrouille update", instructions);
    }
}

[HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.SpeedUpdate))]
public static class CobraCharacterSpeedUpdatePatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TranspilerUtils.ReplaceSixty("Cobra speed update", instructions);
    }
}

[HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.HandleGravity))]
public static class CobraCharacterHandleGravityPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return TranspilerUtils.ReplaceSixty("Cobra handle gravity", instructions);
    }
}

[HarmonyPatch(typeof(ProjectileRocket), nameof(ProjectileRocket.futureCurve))]
public static class RocketProjectilePatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool firstFourFound = false;
        foreach (var code in instructions)
        {
            if (code.opcode == OpCodes.Ldc_R4 && code.operand is float operand && Mathf.Approximately(operand, 0.016666668f))
            {
                code.operand = 1f / CustomFramerateUtils.GetNewFramerateFloat();
                yield return code;
            }
            else
            {
                yield return code;
            }
        }
    }
}

public static class TranspilerUtils
{
    public static IEnumerable<CodeInstruction> ReplaceSixty(string name, IEnumerable<CodeInstruction> instructions)
    {
        bool found = false;
        foreach (var code in instructions)
        {
            if (code.opcode == OpCodes.Ldc_R4 && code.operand is float operandFloat &&
                (Mathf.Approximately(operandFloat, 60f) || Mathf.Approximately(operandFloat, 0.016666668f)))
            {
                code.operand = CustomFramerateUtils.GetNewFramerateFloat();
                found = true;
                yield return code;
            }
            else if (code.opcode == OpCodes.Ldc_I4 && code.operand is int operandInt && operandInt == 60)
            {
                code.operand = CustomFramerateUtils.GetNewFramerateInt();
                found = true;
                yield return code;
            }
            else if (code.opcode == OpCodes.Ldc_I4_S && code.operand is sbyte operandByte && operandByte == 60)
            {
                code.operand = CustomFramerateUtils.GetNewFramerateInt();
                found = true;
                yield return code;
            }
            else
            {
                yield return code;
            }
        }

        if (!found)
        {
            Plugin.Logger.LogError("Failed to locate any instance of 60 or 1/60 in transpiler: " + name);
        }
    }
}