using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MusicReplacer.ReplacementSystem;

public static class FileManagement
{
    private const string SoundsFolderName = "Custom Music";
    private const string SoundPackName = "CustomMusicData.soundreplacements";
    private const string LevelOverridesName = "LevelOverrides.json";
    private static string _cachedSoundsFolderPath;

    private static string GetCustomSoundsFolder()
    {
        if (string.IsNullOrEmpty(_cachedSoundsFolderPath))
        {
            _cachedSoundsFolderPath = Path.Combine(GetModFolder(), SoundsFolderName);
        }

        if (!Directory.Exists(_cachedSoundsFolderPath))
        {
            Directory.CreateDirectory(_cachedSoundsFolderPath);
        }

        return _cachedSoundsFolderPath;
    }

    public static string GetSoundPackPath()
    {
        return Path.Combine(GetModFolder(), SoundPackName);
    }
    
    public static string GetLevelOverridesPath()
    {
        return Path.Combine(GetModFolder(), LevelOverridesName);
    }

    private static string GetModFolder()
    {
        return Path.GetDirectoryName(Plugin.Assembly.Location);
    }
    
    public static void OpenCustomSoundsFolder()
    {
        OpenFolderInExplorer(GetCustomSoundsFolder());
    }
    
    private static void OpenFolderInExplorer(string path)
    { 
        Process.Start("explorer.exe", path);
    }

    public static string GetFullPathOfCustomSound(string pathToCustomSound)
    {
        return Path.Combine(GetModFolder(), pathToCustomSound);
    }
    
    public static string GetFullPathOfCustomSound(MusicSound sound)
    {
        if (!MusicReplacementManager.ReplacementData.TryGetCustomSound(sound, out var customSoundPath))
        {
            throw new NullReferenceException("Failed to find custom sound path");
        }
        return GetFullPathOfCustomSound(customSoundPath);
    }

    public static string[] GetAllCustomSounds()
    {
        var soundsFolder = GetCustomSoundsFolder();
        var rootFolder = GetModFolder();

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".wav", ".wave", ".ogg", ".ogv", ".aiff", ".aif", ".aifc"
        };

        return Directory.GetFiles(soundsFolder, "*.*", SearchOption.AllDirectories)
            .Where(f => allowedExtensions.Contains(Path.GetExtension(f)))
            .Select(f => Path.GetRelativePath(rootFolder, f))
            .ToArray();
    }

    public static string GetDisplayNameForSoundPath(string pathToSound)
    {
        return pathToSound.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Last();
    }
}