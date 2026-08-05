using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Cobra1982AnimeSoundPatches.Patches.Bosses;

[HarmonyPatch(typeof(NmiBossJaneV2))]
[HarmonyPatch(nameof(NmiBossJaneV2.makeAShoot))]
public static class JaneV2ShootSoundTranspiler
{
    // Play the new shoot sound
    [UsedImplicitly]
    private static void Postfix()
    {
        if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip("jane_sniper_shot", out var clip))
        {
            AudioController.Instance.PlayEnemySound(clip);
        }
        else
        {
            Plugin.Logger.LogError("Failed to find 'jane_sniper_shot' eClip");
        }
    }
    
    // Mute the old shoot sound
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool replacedVolume = false;
        bool foundAnchor = false;
        foreach (var instruction in instructions)
        {
            if (!replacedVolume)
            {
                if (foundAnchor)
                {
                    // Set volume of the original shoot sound to 0
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f).MoveLabelsFrom(instruction);
                    replacedVolume = true;
                    continue;
                }

                
                // 191 (eClip.NMI_SHOOT_RIFLE) is the eClip right before the volume is defined (which is 0.5f by default)
                foundAnchor = instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == (int)audioSelectionData.eCLIP.NMI_SHOOT_RIFLE;
            }
            
            yield return instruction;
        }
    }
}