using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Cobra1982AnimeSoundPatches.Patches.Bosses;

[HarmonyPatch(typeof(NmiBossJane))]
[HarmonyPatch(nameof(NmiBossJane.shootManage))]
public static class JaneShootSoundTranspiler
{
    [UsedImplicitly]
    private static void Postfix(ref bool __result)
    {
        if (!__result)
            return;
        
        if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip("jane_sniper_shot", out var clip))
        {
            AudioController.Instance.PlayEnemySound(clip);
        }
        else
        {
            Plugin.Logger.LogError("Failed to find 'jane_sniper_shot' eClip");
        }
    }
    
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool replacedLaser = false;
        bool replacedMummy = false;
        bool foundLaser = false;
        bool foundMummy = false;
        foreach (var instruction in instructions)
        {
            if (!replacedMummy)
            {
                if (foundMummy)
                {
                    // Set volume of the mummy shoot sound to 0
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    replacedMummy = true;
                    continue;
                }

                foundMummy = instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 305;
            }
            
            if (!replacedLaser)
            {
                if (foundLaser)
                {
                    // Set volume of the laser shoot sound to 0
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    replacedLaser = true;
                    continue;
                }

                foundLaser = instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 188;
            }
            
            yield return instruction;
        }
    }
}