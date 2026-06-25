using System;
using System.Collections.Generic;
using System.IO;
using MusicReplacer.Data;
using MusicReplacer.LevelMusic.Data;
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
        
        var triggers = new Dictionary<int, LevelTrigger>();
        // 'includeInactive = true' currently throws exceptions on many levels! some levels are outdated due to this
        var musicTriggers = Object.FindObjectsOfType<audioForceMusicTrigger>(true);
        foreach (var trigger in musicTriggers)
        {
            var triggerData = new LevelTrigger();
            var dimensions = TriggerUtils.GetColliderDimensions(trigger.gameObject);
            var hash = TriggerUtils.GenerateTriggerHash(dimensions.center, dimensions.size);
            triggerData.Type = dimensions.type;
            triggerData.Center = new SimpleVector3(dimensions.center);
            triggerData.Size = new SimpleVector3(dimensions.size);
            triggerData.Radius = dimensions.size.x / 2;
            triggerData.Hash = hash;
            triggerData.MusicClip = (int)trigger.musicClip.EnumValue;
            triggers.Add(triggerData.Hash, triggerData);
        }

        data.LevelTriggers = triggers;
        
        var arenaTriggers = new Dictionary<int, LevelArenaTrigger>();
        var positionalHashByHash = new Dictionary<int, int>();
        var arenaInstances = Object.FindObjectsOfType<NmiArena>(true);
        foreach (var arena in arenaInstances)
        {
            var triggerData = new LevelArenaTrigger();
            var dimensions = TriggerUtils.GetColliderDimensions(arena.gameObject);
            var positionalHash = TriggerUtils.GenerateTriggerHash(dimensions.center, dimensions.size);
            var hash = arena.arenaID;
            triggerData.Type = dimensions.type;
            triggerData.Center = new SimpleVector3(dimensions.center);
            triggerData.Size = new SimpleVector3(dimensions.size);
            triggerData.Radius = dimensions.size.x / 2;
            triggerData.Hash = hash;
            triggerData.MusicClip = (int)arena.arenaMusic.EnumValue;
            triggerData.PlayArenaJingle = arena.playArenaJingle;
            triggerData.PlayArenaVictory = arena.playArenaVictory;
            triggerData.PlayArenaWallIn = arena.playArenaWallIn;
            triggerData.PlayArenaWallOut = arena.playArenaWallOut;
            triggerData.DestroyAtEnd = arena.isDestroyAtEnd;
            
            // Catch this common "mistake" from the devs (could be a mistake from my past self, who knows)
            if (arenaTriggers.ContainsKey(hash)
                && positionalHashByHash.TryGetValue(hash, out var otherPositionalHash)
                && positionalHash == otherPositionalHash)
            {
                Plugin.Logger.LogWarning(
                    $"Two arenas with same ID ({hash}) found. Since their positions are identical, this is fine.");
                continue;
            }

            positionalHashByHash[hash] = positionalHash;
            arenaTriggers.Add(triggerData.Hash, triggerData);
        }
        

        data.ArenaTriggers = arenaTriggers;
        
        return data;
    }
}