using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NoAudioMuffling;

[HarmonyPatch(typeof(CAudio), nameof(CAudio.play), typeof(ushort), typeof(float), typeof(float),
    typeof(CAudio.eVolumeType), typeof(bool), typeof(audioSelectionData.eCLIP))]
public static class AudioPlayTranspiler
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeMatcher = new CodeMatcher(instructions);

        codeMatcher.Start()
            .MatchForward(false,
                new CodeMatch(instruction => instruction.opcode == OpCodes.Ldc_R4 &&
                                             instruction.operand is float f && Mathf.Approximately(f, 0.1f)))
            .ThrowIfInvalid("Failed to find first instance of 0.1f")
            .SetOperandAndAdvance(float.MaxValue)
            .MatchForward(false,
                new CodeMatch(instruction => instruction.opcode == OpCodes.Ldfld &&
                                             instruction.LoadsField(AccessTools.Field(typeof(CAudio),
                                                 nameof(CAudio.voxPlayingCnt)))))
            .ThrowIfInvalid("Failed to find point of access to voxPlayingCnt")
            .SetInstruction(new CodeInstruction(OpCodes.Pop))
            .Advance(1)
            .Insert(CodeInstruction.Call(typeof(AudioPlayTranspiler), nameof(GetZero)))
            .ThrowIfInvalid("Failed to replace with GetZero!");

        return codeMatcher.Instructions();
    }

    private static ushort GetZero()
    {
        return 0;
    }
}