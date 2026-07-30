using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Cobra1982AnimeSoundPatches.Patches;

[HarmonyPatch(typeof(NmiSniper))]
[HarmonyPatch(nameof(NmiSniper.makeAShoot))]
public static class SniperPatches
{
    // Play a custom sound on top of the previous muted sound
    [UsedImplicitly]
    private static void Postfix()
    {
        if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip("sniper_shot_sound", out var clip))
        {
            AudioController.Instance.PlayEnemySound(clip);
        }
        else
        {
            Plugin.Logger.LogError("Failed to find 'sniper_shot_sound' eClip for replacing sniper sound");
        }
    }
    
    // Silences the original shoot volume
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool replacedVolume = false;
        bool foundVolume = false;
        foreach (var instruction in instructions)
        {
            if (!replacedVolume)
            {
                if (foundVolume)
                {
                    // Set volume of the shoot sound to 0
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    replacedVolume = true;
                    continue;
                }

                // 188 = audioSelectionData.eCLIP.NMI_SHOOT_LASER_LAUNCHED
                foundVolume = instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 188;
            }
            
            yield return instruction;
        }
    }
}