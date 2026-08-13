using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace SilenceStackedShotSounds;

[HarmonyPatch(typeof(GuidedProjectile), nameof(GuidedProjectile.OnTrigger), typeof(Collider), typeof(Vector3), typeof(Vector3), typeof(string))]
public static class GuidedShotPatch
{
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldc_I4, 255),
                new CodeMatch(OpCodes.Ldc_R4, 10f),
                new CodeMatch(OpCodes.Ldc_R4, 30f)
            )
            .ThrowIfInvalid("Failed to match")
            .Advance(1)
            .Insert(CodeInstruction.Call(typeof(GuidedShotPatch), nameof(QuietenSounds)))
            .InstructionEnumeration();
    }

    private static void QuietenSounds()
    {
        ShotSilencer.QuietenSounds();
    }
}