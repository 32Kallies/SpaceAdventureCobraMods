using System.Collections.Generic;
using HarmonyLib;
using MusicReplacer.Data;
using UnityEngine;

namespace MusicReplacer.LevelMusic;

[HarmonyPatch]
public static class LevelMusicPatcher
{
    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    [HarmonyPrefix]
    public static void PatchLevelMusic(LevelController __instance)
    {
        var levelDef = GameController.Instance.GetCurrentLevelDefinition();
        if (levelDef == null)
        {
            Plugin.Logger.LogWarning("No level definition found. Skipping.");
            return;
        }
        var overrideMusic = LevelOverrideManager.Data.GetLevelData(levelDef.level);
        
        if (overrideMusic.defaultMusic != 0)
        {
            levelDef.defaultMusic.EnumValue = (audioSelectionData.eCLIP)overrideMusic.defaultMusic;
        }

        if (overrideMusic.arenaMusic != 0)
        {
            levelDef.arenaMusic.EnumValue = (audioSelectionData.eCLIP)overrideMusic.arenaMusic;
        }

        PatchTriggers();
    }

    private static void PatchTriggers()
    {
        var triggers = new Dictionary<int, audioForceMusicTrigger>();
        var musicTriggers = Object.FindObjectsOfType<audioForceMusicTrigger>();
        foreach (var trigger in musicTriggers)
        {
            var dimensions = LevelRipper.GetColliderDimensions(trigger.gameObject);
            var hash = LevelRipper.GenerateTriggerHash(dimensions.center, dimensions.size);
            triggers.Add(hash, trigger);
            Plugin.Logger.LogMessage($"{hash}\t{trigger}");
        }

        foreach (var overrideTrigger in LevelOverrideManager.Data.GetLevels())
        {
            foreach (var (hashString, clip) in overrideTrigger.Value.GetTriggerReplacements())
            {
                if (clip <= 0)
                {
                    continue;
                }
                
                if (!int.TryParse(hashString, out var hash))
                {
                    Plugin.Logger.LogError($"Failed to parse hash for trigger '{hash}' in level {hashString}.");
                    continue;
                }
                
                Plugin.Logger.LogMessage($"Patching level music trigger '{hash}'.");

                if (!triggers.TryGetValue(hash, out var trigger))
                {
                    Plugin.Logger.LogError($"Failed to find trigger by hash '{hash}'.");
                    continue;
                }

                trigger.musicClip.EnumValue = (audioSelectionData.eCLIP)clip;
            }
        }
    }
}