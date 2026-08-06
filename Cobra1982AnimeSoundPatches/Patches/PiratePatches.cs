using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Cobra1982AnimeSoundPatches.Patches;

public static class PiratePatchUtils
{
    internal static int GetPirateShootSoundClip()
    {
        if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip("square_vehicle_shot", out var clip))
        {
            Plugin.Logger.LogInfo("Found square vehicle shot");
            return (int)clip;
        }

        Plugin.Logger.LogError("Failed to find 'square_vehicle_shot' eClip for replacing pirate vehicle sound");
        return 188; // use the old default sound
    }
}
[HarmonyPatch(typeof(NmiPirate))]
[HarmonyPatch(nameof(NmiPirate.LateUpdate))]
public static class PirateLateUpdatePatches
{
    // Silences the original shoot volume
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool done = false;
        foreach (var instruction in instructions)
        {
            if (done)
            {
                yield return instruction;
                continue;
            }

            if (instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 188)
            {
                yield return CodeInstruction.Call(typeof(PiratePatchUtils), nameof(PiratePatchUtils.GetPirateShootSoundClip));
                done = true;
                continue;
            }
            
            yield return instruction;
        }

        if (!done)
        {
            Plugin.Logger.LogError("Failed to patch pirate shoot sound in LateUpdate");
        }
        else
        {
            Plugin.Logger.LogInfo("Patched pirate shoot sound in LateUpdate");
        }
    }
}
[HarmonyPatch(typeof(NmiPirate))]
[HarmonyPatch(nameof(NmiPirate.Update))]
public static class PirateUpdatePatches
{
    // Silences the original shoot volume
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool done = false;
        foreach (var instruction in instructions)
        {
            if (done)
            {
                yield return instruction;
                continue;
            }

            if (instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 188)
            {
                yield return CodeInstruction.Call(typeof(PiratePatchUtils), nameof(PiratePatchUtils.GetPirateShootSoundClip));
                done = true;
                continue;
            }
            
            yield return instruction;
        }

        if (!done)
        {
            Plugin.Logger.LogError("Failed to patch pirate shoot sound in update");
        }
        else
        {
            Plugin.Logger.LogInfo("Patched pirate shoot sound in update");
        }
    }
}