using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Cobra1982AnimeSoundPatches.Patches;

[HarmonyPatch(typeof(NUIScorePanel))]
[HarmonyPatch(nameof(NUIScorePanel.OnEnable))]
public static class MissionCompletionPatches
{
    // Play a custom sound on top of the previous muted sound
    [UsedImplicitly]
    private static void Postfix()
    {
        bool isSPlus = NUIScorePanel.GetRankText(NUIScorePanel.ComputeRank(LevelController.Instance.level))
            .Equals("S+", StringComparison.OrdinalIgnoreCase);

        string clipNameToUse = isSPlus ? "mission_completion_s_plus" : "mission_completion_normal";
        
        Plugin.Logger.LogInfo("Playing mission completion sound: " + clipNameToUse);
        if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip(clipNameToUse, out var clip))
        {
            AudioController.Instance.PlaySoundWithDelay(0.1f, _useunscaledtime: true, clip);
        }
        else
        {
            Plugin.Logger.LogError($"Failed to find '{clipNameToUse}' eClip for replacing mission end sound");
        }
    }
    
    // Silences the original sound volume
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
                
                // check if looking at the JINGLE_RESULTSCREEN sound (audioSelectionData.eCLIP.JINGLE_RESULTSCREEN)
                foundVolume = instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == 66;
            }
            
            yield return instruction;
        }

        if (!replacedVolume)
        {
            Plugin.Logger.LogError("Failed to mute results screen sound through transpiler");
        }
    }
}