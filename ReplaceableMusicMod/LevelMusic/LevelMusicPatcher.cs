using System;
using System.Collections.Generic;
using CobraSoundReplacer.API;
using HarmonyLib;
using MusicReplacer.CustomTriggers;
using MusicReplacer.Data;
using MusicReplacer.ReplacementSystem;
using UnityEngine;
using Object = UnityEngine.Object;

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

        var defaultMusic = overrideMusic.AccessDefaultMusic().GetEClip();
        if (defaultMusic != 0)
        {
            levelDef.defaultMusic.EnumValue = (audioSelectionData.eCLIP)defaultMusic;
        }

        var arenaMusic = overrideMusic.AccessArenaMusic().GetEClip();
        if (arenaMusic != 0)
        {
            levelDef.arenaMusic.EnumValue = (audioSelectionData.eCLIP)arenaMusic;
        }

        PatchTriggers();
        AddCustomTriggers(levelDef);
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
            foreach (var (hashString, clip) in overrideTrigger.Value.BuildTriggerReplacementsDictionary())
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

                var eClip = (audioSelectionData.eCLIP)clip;
                trigger.musicClip.EnumValue = eClip;
                trigger.m_Clip = eClip;
            }
        }
    }

    private static void AddCustomTriggers(LevelDefinition level)
    {
        var levelName = level.levelName;
        
        var customTriggers = CustomTriggerParser.ParseAllFiles(FileManagement.GetCustomTriggersFolder());
        if (customTriggers.Count == 0)
        {
            Plugin.Logger.LogMessage("No custom triggers found!");
        }

        int added = 0;
        
        foreach (CustomTrigger trigger in customTriggers)
        {
            if (!string.Equals(trigger.Level, levelName, StringComparison.OrdinalIgnoreCase))
            {
                // trigger is meant for another level, therefore skip
                continue;
            }

            CreateCustomTrigger(trigger);
            added++;
        }
        
        Plugin.Logger.LogMessage($"Added {added} custom triggers to level '{levelName}'.");
    }

    private static void CreateCustomTrigger(CustomTrigger trigger)
    {
        var gameObject = new GameObject("Trigger-" + trigger.Name);
        gameObject.SetActive(false);
        gameObject.layer = 12;
        var collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        var behavior = gameObject.AddComponent<audioForceMusicTrigger>();
        behavior.musicPriority = trigger.Priority;
        if (!CustomSoundUtils.TryGetEClip(trigger.Music, out var clip))
        {
            Plugin.Logger.LogWarning("Failed to find clip by name " + trigger.Music);
        }
        behavior.m_Clip = clip;
        behavior.musicClip = new audioSelectionData.CeCLIP
        {
            EnumValue = clip
        };

        var bounds = CustomTriggerUtils.GetCustomTriggerBounds(trigger);
        gameObject.transform.position = bounds.Center;
        collider.size = bounds.Size;
        
        gameObject.SetActive(true);
    }
}