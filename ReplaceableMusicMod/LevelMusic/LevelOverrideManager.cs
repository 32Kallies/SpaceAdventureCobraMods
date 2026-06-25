using System;
using System.IO;
using MusicReplacer.LevelMusic.Data;
using MusicReplacer.ReplacementSystem;
using MusicReplacer.Utilities;
using Newtonsoft.Json;

namespace MusicReplacer.LevelMusic;

public static class LevelOverrideManager
{
    public static GlobalLevelOverrideData Data { get; private set; }

    internal static void Initialize()
    {
        Data = TryGetData(out var data) ? data : new GlobalLevelOverrideData();
    }
    
    public static void SaveChanges()
    {
        var path = FileManagement.GetLevelOverridesPath();
        File.WriteAllText(path, JsonConvert.SerializeObject(Data, Formatting.Indented));
    }

    private static bool TryGetData(out GlobalLevelOverrideData data)
    {
        var path = FileManagement.GetLevelOverridesPath();
        if (!File.Exists(path))
        {
            data = null;
            return false;
        }

        try
        {
            var text = File.ReadAllText(path);
            data = JsonConvert.DeserializeObject<GlobalLevelOverrideData>(text);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to load level data from path {path}: {e}");
            data = null;
            return false;
        }
        
        return true;
    }
}