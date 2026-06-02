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
        const float triggerDepth = 100f;
        const float verticalTriggerHeight = 100f;
        
        var gameObject = new GameObject("Trigger-" + trigger.Name);
        gameObject.SetActive(false);
        var collider = gameObject.AddComponent<BoxCollider>();
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

        switch (trigger.Shape)
        {
            case TriggerShape.CenterBox:
                var box = trigger as CenterBoxTrigger;
                gameObject.transform.position = new Vector3(box.Center.X, box.Center.Y, 0);
                collider.size = new Vector3(box.Size.W, box.Size.H, triggerDepth);
                break;
            case TriggerShape.VerticalLine:
                var line = trigger as VerticalLineTrigger;
                gameObject.transform.position = new Vector3((line.Left + line.Right) / 2, 0);
                collider.size = new Vector3(Mathf.Abs(line.Right - line.Left), verticalTriggerHeight, triggerDepth);
                break;
            case TriggerShape.CornerBox:
                var corners = trigger as CornerBoxTrigger;
                gameObject.transform.position = GetCenterBetweenCorners(corners);
                collider.size = GetTriggerSizeBetweenCorners(corners, triggerDepth);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(trigger.Shape));
        }
        
        gameObject.SetActive(true);
    }

    private static Vector3 GetCenterBetweenCorners(CornerBoxTrigger data)
    {
        return new Vector3(
            (data.Corner1.X + data.Corner2.X) / 2,
            (data.Corner1.Y + data.Corner2.Y) / 2,
            0);
    }
    
    private static Vector3 GetTriggerSizeBetweenCorners(CornerBoxTrigger data, float depth)
    {
        return new Vector3(
            Mathf.Abs(data.Corner1.X - data.Corner2.X),
            Mathf.Abs(data.Corner1.Y - data.Corner2.Y),
            depth);
    }
}