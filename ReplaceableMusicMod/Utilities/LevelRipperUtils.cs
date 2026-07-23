using System;
using System.Collections.Generic;
using System.IO;
using MusicReplacer.Arenas;
using MusicReplacer.Data;
using MusicReplacer.LevelMusic.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MusicReplacer.Utilities;

public static class LevelRipperUtils
{
    private static RippedGameLevelData _data;
    
    public static void LoadLevelData()
    {
        var modFolder = Path.GetDirectoryName(Plugin.Assembly.Location);
        var levelMusicDataFolder = Path.Combine(modFolder, "Ripped Level Data");
        var files = Directory.GetFiles(levelMusicDataFolder, "*.json");
        var dictionary = new Dictionary<int, RippedLevelMusicData>();
        foreach (var file in files)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<RippedLevelMusicData>(File.ReadAllText(file));
                RegenerateHashes(obj);
                dictionary.Add(obj.LevelId, obj);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }

        _data = new RippedGameLevelData
        {
            Levels = dictionary
        };
    }

    [Obsolete("Technically no longer required since all level rip data was regenerated with proper hashes")]
    private static void RegenerateHashes(RippedLevelMusicData data)
    {
        foreach (var trigger in data.LevelTriggers)
        {
            trigger.Value.Hash = TriggerUtils.GenerateTriggerHash(trigger.Value.Center.ToVector3(), trigger.Value.Size.ToVector3());
        }
    }

    public static RippedLevelMusicData GetLevelMusicData(LevelController.Level level)
    {
        var asInt = (int)level;
        return GetLevelMusicData(asInt);
    }
    
    public static RippedLevelMusicData GetLevelMusicData(int levelId)
    {
        return _data.Levels[levelId];
    }
    
    public static void DumpCurrentLevel()
    {
        var data = GenerateLevelMusicData();
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        File.WriteAllText(Path.Combine("Ripped Level Data", data.LevelName + ".json"), json);
    }
    
    public static RippedLevelMusicData GenerateLevelMusicData()
    {
        var data = new RippedLevelMusicData();
        
        var definition = GameController.Instance.GetCurrentLevelDefinition();

        data.LevelName = definition.levelName;
        data.LevelId = (int)definition.level;
        data.DefaultMusic = (int)definition.defaultMusic.EnumValue;
        data.ArenaMusic = (int)definition.arenaMusic.EnumValue;
        
        var triggers = new Dictionary<long, LevelTrigger>();
        // 'includeInactive = true' currently throws exceptions on many levels! some levels may be outdated due to this
        var musicTriggers = Object.FindObjectsOfType<audioForceMusicTrigger>(true);
        foreach (var trigger in musicTriggers)
        {
            var triggerData = new LevelTrigger();
            var dimensions = TriggerUtils.GetColliderDimensions(trigger.gameObject);
            long hash = TriggerUtils.GenerateTriggerHash(dimensions.center, dimensions.size);
            triggerData.Type = dimensions.type;
            triggerData.Center = new SimpleVector3(dimensions.center);
            triggerData.Size = new SimpleVector3(dimensions.size);
            triggerData.Radius = dimensions.size.x / 2;
            triggerData.Hash = hash;
            triggerData.MusicClip = (int)trigger.musicClip.EnumValue;
            triggers.Add(triggerData.Hash, triggerData);
        }

        data.LevelTriggers = triggers;
        
        var arenaTriggers = new Dictionary<long, LevelArenaTrigger>();
        var arenaInstances = Object.FindObjectsOfType<NmiArena>(true);
        foreach (var arena in arenaInstances)
        {
            if (IsResettableInstance(arena.transform))
            {
                continue;
            }
            
            var triggerData = new LevelArenaTrigger();
            var dimensions = TriggerUtils.GetColliderDimensions(arena.gameObject);
            long hash = ArenaIdentifier.GetArenaId(arena);
            triggerData.Type = dimensions.type;
            triggerData.Center = new SimpleVector3(GetBestPositionForArena(arena));
            triggerData.Size = new SimpleVector3(dimensions.size);
            triggerData.Radius = dimensions.size.x / 2;
            triggerData.Hash = hash;
            triggerData.MusicClip = (int)arena.arenaMusic.EnumValue;
            triggerData.PlayArenaJingle = arena.playArenaJingle;
            triggerData.PlayArenaVictory = arena.playArenaVictory;
            triggerData.PlayArenaWallIn = arena.playArenaWallIn;
            triggerData.PlayArenaWallOut = arena.playArenaWallOut;
            triggerData.DestroyAtEnd = arena.isDestroyAtEnd;
            
            if (arenaTriggers.ContainsKey(hash))
            {
                Plugin.Logger.LogWarning($"Two arenas with same ID ({hash}) found.");
                continue;
            }

            arenaTriggers.Add(triggerData.Hash, triggerData);
        }
        

        data.ArenaTriggers = arenaTriggers;
        
        return data;
    }

    public static bool IsResettableInstance(Transform transform)
    {
        if (transform.gameObject.name.Contains("Resettable_Instance(", StringComparison.OrdinalIgnoreCase)) return true;
        if (transform.parent == null) return false;
        return IsResettableInstance(transform.parent);
    }

    private static Vector3 GetBestPositionForArena(NmiArena arena)
    {
        var defaultPos = arena.transform.position;
        
        var waves = arena.transform.Find("Waves");
        if (waves == null || waves.childCount == 0)
        {
            return defaultPos;
        }

        var firstWave = waves.GetChild(0);
        if (firstWave == null || firstWave.childCount == 0)
        {
            return defaultPos;
        }

        var firstEnemy = firstWave.GetChild(0);

        if (firstEnemy != null && firstEnemy.name.Equals("label", StringComparison.OrdinalIgnoreCase))
        {
            if (firstWave.childCount < 2)
            {
                return defaultPos;
            }
            
            firstEnemy = firstWave.GetChild(1);
        }
        
        if (firstEnemy == null)
        {
            return defaultPos;
        }

        return firstEnemy.position;
    }
}