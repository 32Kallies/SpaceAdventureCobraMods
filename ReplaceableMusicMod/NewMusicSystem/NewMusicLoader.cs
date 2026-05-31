using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CobraSoundReplacer.Core;
using MusicReplacer.ReplacementSystem;

namespace MusicReplacer.NewMusicSystem;

public static class NewMusicLoader
{
    private const string SoundPackName = "Custom Music Pack";
    
    private static readonly Dictionary<string, NewMusicClip> NewClips = [];
    
    public static void LoadNewMusic()
    {
        var soundClipPaths = DetectAllSoundFiles();
        var soundPack = BuildSoundPack(soundClipPaths);
        Plugin.StartCoroutineOnPlugin(RegisterSoundPackAsync(soundPack));
    }

    public static IEnumerable<NewMusicClip> GetNewMusicClips()
    {
        return NewClips.Values;
    }

    private static IEnumerator RegisterSoundPackAsync(SoundPack pack)
    {
        yield return SoundPackRegistry.RegisterSoundPack(pack, FileManagement.GetModFolder());
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
            
            NewClips.Add(id, new NewMusicClip(id, clip));
        }
    }

    private static string[] DetectAllSoundFiles()
    {
        var folder = FileManagement.GetNewMusicFolder();
        return FileManagement.GetAllSoundFilesInFolder(folder, FileManagement.GetModFolder());
    }

    private static SoundPack BuildSoundPack(IEnumerable<string> clipPaths)
    {
        var pack = new SoundPack();
        var newEClips = new List<CustomEClip>();
        var idsHashset = new HashSet<string>();
        foreach (var clip in clipPaths)
        {
            if (!idsHashset.Add(clip))
            {
                Plugin.Logger.LogWarning("Duplicate clips detected while building pack: " + SoundPackName);
                continue;
            }
            newEClips.Add(new CustomEClip
            {
                Id = clip,
                SoundName = clip
            });
        }

        pack.Enable = true;
        pack.PackName = SoundPackName;
        var newClips = newEClips.ToArray();
        pack.NewEClips = newClips;
        
        Plugin.Logger.LogMessage($"Registered new Sound Pack with {newClips.Length} new clips!");

        return pack;
    }
}