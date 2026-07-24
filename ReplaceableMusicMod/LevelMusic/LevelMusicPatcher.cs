using System;
using System.Collections.Generic;
using CobraSoundReplacer.API;
using HarmonyLib;
using MusicReplacer.Arenas;
using MusicReplacer.CustomTriggers;
using MusicReplacer.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MusicReplacer.LevelMusic;

[HarmonyPatch]
public static class LevelMusicPatcher
{
    // -1 = failed to find, 0 = found, > 0 = found (extra?)
    private static Dictionary<long, int> failedToFind;
    
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
        
        failedToFind = new Dictionary<long, int>();

        PatchTriggers();
    }

    [HarmonyPatch(typeof(LevelController), nameof(LevelController.Start))]
    [HarmonyPostfix]
    public static void PatchLevelMusicPostfixPatch(LevelController __instance)
    {
        var levelDef = GameController.Instance.GetCurrentLevelDefinition();
        if (levelDef == null)
        {
            Plugin.Logger.LogWarning("No level definition found. Skipping.");
            return;
        }

        failedToFind ??= new Dictionary<long, int>();
        PatchArenas(__instance);

        foreach (var failed in failedToFind)
        {
            if (failed.Value == -1)
                Plugin.Logger.LogError("Failed to find trigger or arena by hash: " + failed.Key);
        }

        AddCustomTriggers(levelDef);
    }

    private static void PatchTriggers()
    {
        var triggersByHash = new Dictionary<long, List<audioForceMusicTrigger>>();
        var musicTriggers = Object.FindObjectsOfType<audioForceMusicTrigger>(true);
        foreach (var trigger in musicTriggers)
        {
            var dimensions = TriggerUtils.GetColliderDimensions(trigger.gameObject);
            var hash = TriggerUtils.GenerateTriggerHash(dimensions.center, dimensions.size);
            Plugin.Logger.LogDebug($"{hash}\t{trigger}");

            List<audioForceMusicTrigger> listToAddTo;
            if (triggersByHash.TryGetValue(hash, out var triggersList))
            {
                listToAddTo = triggersList;
            }
            else
            {
                listToAddTo = new List<audioForceMusicTrigger>();
                triggersByHash.Add(hash, listToAddTo);
            }

            listToAddTo.Add(trigger);
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
                
                if (!long.TryParse(hashString, out var hash))
                {
                    Plugin.Logger.LogError($"Failed to parse hash for trigger '{hash}' in level {hashString}.");
                    continue;
                }
                
                Plugin.Logger.LogInfo($"Checking level music trigger '{hash}'.");

                if (!triggersByHash.TryGetValue(hash, out var triggers))
                {
                    failedToFind[hash] = failedToFind.GetValueOrDefault(hash) - 1;
                    continue;
                }

                Plugin.Logger.LogInfo($"Patching {triggers.Count} music triggers by '{hash}'.");

                foreach (var trigger in triggers)
                {
                    Plugin.Logger.LogInfo($"Patching level music trigger '{hash}'.");
                
                    var eClip = (audioSelectionData.eCLIP)clip;
                    trigger.musicClip.EnumValue = eClip;
                    trigger.m_Clip = eClip;
                    failedToFind[hash] = failedToFind.GetValueOrDefault(hash) + 1;
                }
            }
        }
    }
    
    private static void PatchArenas(LevelController level)
    {
        Plugin.Logger.LogInfo("Patching arena music for level: " + level.level);
        var arenasByID = new Dictionary<long, List<NmiArena>>();
        var arenasInScene = Object.FindObjectsOfType<NmiArena>(true);
        foreach (var arena in arenasInScene)
        {
            var hash = ArenaIdentifier.GetArenaId(arena);
            List<NmiArena> listToAddTo;
            if (arenasByID.TryGetValue(hash, out var arenasList))
            {
                listToAddTo = arenasList;
            }
            else
            {
                listToAddTo = new List<NmiArena>();
                arenasByID.Add(hash, listToAddTo);
            }

            listToAddTo.Add(arena); 
        }

        foreach (var levelData in LevelOverrideManager.Data.GetLevels())
        {
            if (levelData.Key != level.level)
            {
                continue;
            }
            
            foreach (var (hashString, clip) in levelData.Value.BuildTriggerReplacementsDictionary())
            {
                if (clip <= 0)
                {
                    continue;
                }
                
                if (!long.TryParse(hashString, out var hash))
                {
                    Plugin.Logger.LogError($"Failed to parse hash for trigger '{hash}' in level {hashString}.");
                    continue;
                }

                if (!arenasByID.TryGetValue(hash, out var arenas))
                {
                    failedToFind[hash] = failedToFind.GetValueOrDefault(hash) - 1;
                    continue;
                }

                foreach (var arena in arenas)
                {
                    if (arena == null)
                    {
                        Plugin.Logger.LogError("Null arena found; skipping");
                        continue;
                    }
                    audioSelectionData.eCLIP oldClip = arena.arenaMusic.EnumValue;
                    var eClip = (audioSelectionData.eCLIP)clip;
                    arena.arenaMusic.EnumValue = eClip;
                    failedToFind[hash] = failedToFind.GetValueOrDefault(hash) + 1;
                    Plugin.Logger.LogInfo($"Patching level arena trigger by hash '{hash}' (original music was: {oldClip}).");
                }
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

        gameObject.AddComponent<CustomTriggerTag>();
        
        gameObject.SetActive(true);
    }
}