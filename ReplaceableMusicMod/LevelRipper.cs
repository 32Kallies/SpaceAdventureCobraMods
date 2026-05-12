using System;
using System.Collections.Generic;
using System.IO;
using MusicReplacer.Data;
using MusicReplacer.LevelMusic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MusicReplacer;

public static class LevelRipper
{
    private static GameLevelData _data;
    
    public static void LoadLevelData()
    {
        var modFolder = Path.GetDirectoryName(Plugin.Assembly.Location);
        var levelMusicDataFolder = Path.Combine(modFolder, "Ripped Level Data");
        var files = Directory.GetFiles(levelMusicDataFolder, "*.json");
        var dictionary = new Dictionary<int, LevelMusicData>();
        foreach (var file in files)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<LevelMusicData>(File.ReadAllText(file));
                RegenerateHashes(obj);
                dictionary.Add(obj.LevelId, obj);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }

        _data = new GameLevelData
        {
            Levels = dictionary
        };
    }

    private static void RegenerateHashes(LevelMusicData data)
    {
        foreach (var trigger in data.LevelTriggers)
        {
            trigger.Value.Hash = GenerateTriggerHash(trigger.Value.Center.ToVector3(), trigger.Value.Size.ToVector3());
        }
    }

    public static LevelMusicData GetLevelMusicData(LevelController.Level level)
    {
        var asInt = (int)level;
        return GetLevelMusicData(asInt);
    }
    
    public static LevelMusicData GetLevelMusicData(int levelId)
    {
        return _data.Levels[levelId];
    }
    
    public static void DumpCurrentLevel()
    {
        var data = GenerateLevelMusicData();
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        File.WriteAllText(Path.Combine("Level Music Data", data.LevelName + ".json"), json);
    }
    
    public static LevelMusicData GenerateLevelMusicData()
    {
        var data = new LevelMusicData();
        
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
            var dimensions = GetColliderDimensions(trigger.gameObject);
            var hash = GenerateTriggerHash(dimensions.center, dimensions.size);
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

    public static (PrimitiveType type, Vector3 center, Vector3 size) GetColliderDimensions(GameObject obj)
    {
        var box = obj.GetComponent<BoxCollider>();
        if (box != null)
        {
            return (PrimitiveType.Cube, obj.transform.position, box.size);
        }
        var sphere = obj.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            return (PrimitiveType.Sphere, obj.transform.position, Vector3.one * sphere.radius * 2f);
        }

        throw new Exception("Trigger does not have a box or sphere collider!");
    }

    public static int GenerateTriggerHash(Vector3 center, Vector3 size)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);
        int cz = Mathf.RoundToInt(center.z);

        int sx = Mathf.RoundToInt(size.x);
        int sy = Mathf.RoundToInt(size.y);
        int sz = Mathf.RoundToInt(size.z);

        return GetStableHash(cx, cy, cz, sx, sy, sz);
    }
    
    private static int GetStableHash(int cX, int cY, int cZ, int sX, int sY, int sZ)
    {
        unchecked
        {
            int hash = 17;

            hash = hash * 31 + cX.GetHashCode();
            hash = hash * 31 + cY.GetHashCode();
            hash = hash * 31 + cZ.GetHashCode();

            hash = hash * 31 + sX.GetHashCode();
            hash = hash * 31 + sY.GetHashCode();
            hash = hash * 31 + sZ.GetHashCode();

            return hash;
        }
    }
}