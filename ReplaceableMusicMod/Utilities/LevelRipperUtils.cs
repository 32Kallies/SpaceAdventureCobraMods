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
        File.WriteAllText(Path.Combine("Level Music Data", data.LevelName + ".json"), json);
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
        var musicTriggers = Object.FindObjectsOfType<audioForceMusicTrigger>();
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
        return data;
    }
}