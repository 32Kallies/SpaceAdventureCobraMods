using System;
using System.IO;
using CobraSoundReplacer.Core;
using MusicReplacer.Utilities;
using Newtonsoft.Json;

namespace MusicReplacer.ReplacementSystem;

public static class MusicReplacementManager
{
    private const string SoundPackName = "Music Replacements Pack";
    
    public static MusicReplacementData ReplacementData { get; private set; }

    public static void Register()
    {
        ReplacementData = new MusicReplacementData();
        if (TryGetSoundPack(out var pack))
            ReplacementData.LoadFromSoundPack(pack);
    }

    public static void SaveChanges()
    {
        var path = FileManagement.GetSoundPackPath();
        var pack = ReplacementData.ConvertToSoundPack(SoundPackName);
        File.WriteAllText(path, JsonConvert.SerializeObject(pack, Formatting.Indented));
    }

    private static bool TryGetSoundPack(out SoundPack pack)
    {
        var path = FileManagement.GetSoundPackPath();
        if (!File.Exists(path))
        {
            pack = null;
            return false;
        }

        try
        {
            var text = File.ReadAllText(path);
            pack = JsonConvert.DeserializeObject<SoundPack>(text);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to load SoundPack from path {path}: {e}");
            pack = null;
            return false;
        }
        
        return true;
    }
}