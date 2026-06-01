using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CobraSoundReplacer.Core;
using MusicReplacer.ReplacementSystem;
using UnityEngine;

namespace MusicReplacer.NewMusicSystem;

public static class NewMusicLoader
{
    private const float DefaultVolume = 5.3f;

    private const string SoundPackName = "Custom Music Pack";
    
    private static readonly Dictionary<string, NewMusicClip> NewClips = [];
    
    internal static void LoadNewMusic()
    {
        var soundClipPaths = DetectAllSoundFiles();
        if (!soundClipPaths.Any())
        {
            Plugin.Logger.LogMessage("Skipping sound pack - no new custom music found");
            return;
        }
        var soundPack = BuildSoundPack(soundClipPaths);
        SoundPackRegistry.RegisterRuntimeSoundPack(soundPack, FileManagement.GetModFolder());
        Plugin.StartCoroutineOnPlugin(CacheEClipsWhenReady(soundPack));
    }

    public static IEnumerable<NewMusicClip> GetNewMusicClips()
    {
        return NewClips.Values;
    }
    
    public static int GetEClipForCustomSound(string customSoundName)
    {
        bool found = CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip(customSoundName, out var clip);
            
        if (!found)
        {
            Plugin.Logger.LogWarning("Failed to find EClip for custom sound: " + customSoundName);
            return (int)audioSelectionData.eCLIP.NONE;
        }

        return (int)clip;
    }

    private static IEnumerator CacheEClipsWhenReady(SoundPack pack)
    {
        yield return new WaitUntil(() => SoundPackRegistry.ReplacementCompleted);
        CacheCreatedEClips(pack.NewEClips.Select(e => e.Id).ToArray());
    }

    private static void CacheCreatedEClips(string[] ids)
    {
        NewClips.Clear();
        
        foreach (var id in ids)
        {
            if (!CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip(id, out var clip))
            {
                Plugin.Logger.LogWarning("Failed to get EClip for ID " + id);
                continue;
            }
            
            NewClips.Add(id, new NewMusicClip(id, id, clip));
        }
    }

    private static string GetIdForPath(string path)
    {
        return Path.GetFileNameWithoutExtension(path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Last());
    }

    private static IEnumerable<string> DetectAllSoundFiles()
    {
        var folder = FileManagement.GetNewMusicFolder();
        return FileManagement.GetAllSoundFilesInFolder(folder, FileManagement.GetModFolder()).Select(path => path.Replace('\\', '/'));
    }

    private static SoundPack BuildSoundPack(IEnumerable<string> clipPaths)
    {
        var pack = new SoundPack();
        
        var newEClips = new List<CustomEClip>();
        var newSounds = new List<NewSound>();
        
        var idsHashset = new HashSet<string>();
        
        foreach (var clipPath in clipPaths)
        {
            var id = GetIdForPath(clipPath);
            if (!idsHashset.Add(id))
            {
                Plugin.Logger.LogWarning("Duplicate clips detected while building pack: " + SoundPackName);
                continue;
            }
            newEClips.Add(new CustomEClip
            {
                Id = id,
                SoundName = id
            });
            newSounds.Add(new NewSound(id, clipPath, DefaultVolume)
            {
                VolumeType = (sbyte)CAudio.eVolumeType.music,
                Looping = true
            });
        }

        pack.Enable = true;
        pack.PackName = SoundPackName;
        var newClipsArray = newEClips.ToArray();
        pack.NewEClips = newClipsArray;
        pack.NewSounds = newSounds.ToArray();
        
        Plugin.Logger.LogMessage($"Registered new Sound Pack with {newClipsArray.Length} new clips!");

        return pack;
    }
}