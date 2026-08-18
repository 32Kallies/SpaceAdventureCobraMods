using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace PscyhogunArmOverhaul.Patches;

[HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.ProtheseOn))]
public static class DisableLeftArmOnSound
{
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Ldc_I4),
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ldc_I4),
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Pop)
                )
            .RemoveInstructions(8)
            .InstructionEnumeration();
    }
}