using System;
using System.Collections;
using System.Collections.Generic;
using CobraSoundReplacer.API;
using HarmonyLib;
using MusicReplacer.CustomTriggers;
using MusicReplacer.Utilities;
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

    [HarmonyPatch(typeof(CobraCharacter), nameof(CobraCharacter.Start))]
    [HarmonyPostfix]
    public static void PatchArenaMusicPostfix()
    {
        Plugin.StartCoroutineOnPlugin(PatchArenasWithDelay());
    }

    private static void PatchTriggers()
    {
        var triggers = new Dictionary<int, audioForceMusicTrigger>();
        var musicTriggers = Object.FindObjectsOfType<audioForceMusicTrigger>();
        foreach (var trigger in musicTriggers)
        {
            var dimensions = TriggerUtils.GetColliderDimensions(trigger.gameObject);
            var hash = TriggerUtils.GenerateTriggerHash(dimensions.center, dimensions.size);
            triggers.Add(hash, trigger);
            Plugin.Logger.LogDebug($"{hash}\t{trigger}");
        }

        foreach (var levelData in LevelOverrideManager.Data.GetLevels())
        {
            if (levelData.Key != LevelController.Instance.level)
            {
                continue;
            }
            
            foreach (var (hashString, clip) in levelData.Value.BuildTriggerReplacementsDictionary())
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
                
                Plugin.Logger.LogInfo($"Patching level music trigger '{hash}'.");

                if (!triggers.TryGetValue(hash, out var trigger))
                {
                    if (!IsTriggerProbablyArena(hash))
                        Plugin.Logger.LogError($"Failed to find trigger by hash '{hash}'.");
                    continue;
                }

                var eClip = (audioSelectionData.eCLIP)clip;
                trigger.musicClip.EnumValue = eClip;
                trigger.m_Clip = eClip;
            }
        }
    }

    // Necessary to prevent patching before the level is fully setup
    private static IEnumerator PatchArenasWithDelay()
    {
        yield return new WaitForSeconds(1);
        PatchArenas();
    }
    
    private static void PatchArenas()
    {
        var arenas = new Dictionary<int, NmiArena>();
        var arenasInScene = Object.FindObjectsOfType<NmiArena>(true);
        foreach (var arena in arenasInScene)
        {
            var hash = arena.arenaID;
            if (arenas.ContainsKey(hash))
            {
                Plugin.Logger.LogWarning("Multiple arenas found in scene with identical IDs! Skipping to avoid exceptions.");
                continue;
            }
            arenas.Add(arena.arenaID, arena);
            Plugin.Logger.LogDebug($"{hash}\t{arena}");
        }

        foreach (var levelData in LevelOverrideManager.Data.GetLevels())
        {
            if (levelData.Key != LevelController.Instance.level)
            {
                continue;
            }
            
            foreach (var (hashString, clip) in levelData.Value.BuildTriggerReplacementsDictionary())
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

                if (!arenas.TryGetValue(hash, out var trigger))
                {
                    if (IsTriggerProbablyArena(hash))
                        Plugin.Logger.LogError($"Failed to find arena by hash '{hash}'.");
                    continue;
                }

                var eClip = (audioSelectionData.eCLIP)clip;
                trigger.arenaMusic.EnumValue = eClip;
                Plugin.Logger.LogInfo($"Patching level arena trigger '{hash}'.");
            }
        }
    }

    private static bool IsTriggerProbablyArena(int hash)
    {
        return hash <= 20 && hash >= 0;
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

        gameObject.AddComponent<CustomTriggerTag>();
        
        gameObject.SetActive(true);
    }
}