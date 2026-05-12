using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicReplacer.ReplacementSystem;

public static class MusicProcessor
{
    private const string MusicLoadNamePrefix = "mus";
    private const string MusicEClipPrefix = "MUS";

    private static List<MusicSound> _musicSounds;
    private static Dictionary<string, MusicSound> _musicSoundsByLoadName;
    private static List<audioSelectionData.eCLIP> _musicEClips;
    private static Dictionary<audioSelectionData.eCLIP, string> _loadNameByEClip;
    private static Dictionary<audioSelectionData.eCLIP, MusicSound> _musicSoundsByEClip;

    private static Dictionary<MusicCategory, List<MusicSound>> _musicByCategory;
    
    private static readonly HashSet<string> UnusedMusic =
    [
        "mus_loop_ep03_lvl01_combat.wav",
        "mus_loop_ep03_lvl01_explorationcombat.wav",
        "mus_loop_ep03_lvl01_turtle.wav",
        "mus_loop_ingame.wav",
        "mus_loop_jd_boss.mp3",
        "mus_loop_jd_exploration.mp3",
        "mus_loop_jd_platforming.ogg",
        "mus_loop_main.mp3",
        "mus_loop_title.mp3",
        "mus_ep02_lvl03_cutscene3d_01.wav",
        "mus_ep03_lvl01_cutscene3d_01.wav"
    ];

    public static void ScrapeAllData(CAudio audio)
    {
        _musicSounds = new List<MusicSound>();
        _musicSoundsByLoadName = new Dictionary<string, MusicSound>();
        var clips = audio.AllClip;

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null || string.IsNullOrEmpty(clips[i].loadname))
            {
                Plugin.Logger.LogWarning("Invalid clip at index " + i);
                continue;
            }

            if (clips[i].loadname.StartsWith(MusicLoadNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var soundNameWithoutExtension = clips[i].loadname.Split('.').First();
                var data = new MusicSound(i, clips[i].loadname, soundNameWithoutExtension.ToUpper());
                _musicSounds.Add(data);
                _musicSoundsByLoadName.Add(soundNameWithoutExtension, data);
            }
        }

        _musicEClips = new List<audioSelectionData.eCLIP>();
        foreach (var obj in Enum.GetValues(typeof(audioSelectionData.eCLIP)))
        {
            var eClip = (audioSelectionData.eCLIP)obj;
            if (eClip.ToString().StartsWith(MusicEClipPrefix, StringComparison.OrdinalIgnoreCase))
                _musicEClips.Add(eClip);
        }
        
        _loadNameByEClip = new Dictionary<audioSelectionData.eCLIP, string>();

        foreach (var eClip in _musicEClips)
        {
            var soundName = AudioController.Audio.asd.getSoundName(eClip);
            if (string.IsNullOrEmpty(soundName))
            {
                Plugin.Logger.LogWarning("Invalid sound name for EClip: " + eClip);
                continue;
            }

            if (!_musicSoundsByLoadName.TryGetValue(soundName, out var data))
            {
                Plugin.Logger.LogWarning("Failed to find sound: " + soundName);
                continue;
            }

            data.EClip = eClip;
            _loadNameByEClip.Add(eClip, soundName);
        }

        _musicByCategory = new Dictionary<MusicCategory, List<MusicSound>>();
        foreach (var sound in _musicSounds)
        {
            var category = GetCategoryForSoundName(sound.FileName);
            if (!_musicByCategory.TryGetValue(category, out var list))
            {
                list = new List<MusicSound>();
                _musicByCategory[category] = list;
            }

            list.Add(sound);
        }
        
        _musicSoundsByEClip = new Dictionary<audioSelectionData.eCLIP, MusicSound>();
        foreach (var musicSound in _musicSounds)
        {
            _musicSoundsByEClip[musicSound.EClip] = musicSound;
        }
    }

    public static bool TryGetMusicSoundForEClip(audioSelectionData.eCLIP clip, out MusicSound sound)
    {
        return _musicSoundsByEClip.TryGetValue(clip, out sound);
    }

    public static IEnumerable<MusicSound> GetMusicForCategory(MusicCategory category)
    {
        return _musicByCategory[category];
    }
    
    public static IEnumerable<(MusicCategory category, MusicSound sound)> GetAllMusic(bool ignoreUnused)
    {
        foreach (var category in _musicByCategory)
        {
            if (ignoreUnused && category.Key == MusicCategory.Unused)
                continue;
            foreach (var music in category.Value)
            {
                yield return (category.Key, music);
            }
        }
    }
    
    public static string GetNameForCategory(MusicCategory category)
    {
        if (category == MusicCategory.SnowCliff)
        {
            return "Snow_Cliff";
        }

        if (category == MusicCategory.Unused)
        {
            return "Unused / Broken";
        }
        
        return category.ToString();
    }

    public static string GetLoadNameForEClip(audioSelectionData.eCLIP clip)
    {
        if (_loadNameByEClip.TryGetValue(clip, out var name) && !string.IsNullOrEmpty(name))
        {
            return name;
        }
        Plugin.Logger.LogWarning("Failed to find LoadName for EClip: " + clip);
        return clip.ToString();
    }
    
    public static string GetFriendlyNameForEClip(audioSelectionData.eCLIP clip)
    {
        if (_musicSoundsByEClip.TryGetValue(clip, out var music))
        {
            var finalName = music.DisplayName;
            if (MusicReplacementManager.ReplacementData.TryGetCustomSound(music.FileName, out var custom))
            {
                var customSoundName = FileManagement.GetDisplayNameForSoundPath(custom).TruncateAdvanced(16);
                finalName += $" (<color=#7F00FF>{customSoundName}</color>)";
            }
            return finalName;
        }
        
        Plugin.Logger.LogWarning("Failed to find LoadName for EClip: " + clip);
        return clip.ToString();
    }
    
    private static string TruncateAdvanced(this string value, int maxChars)
    {
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "…";
    }

    private static MusicCategory GetCategoryForSoundName(string soundName)
    {
        if (UnusedMusic.Contains(soundName))
            return MusicCategory.Unused;
        
        // Reward
        if (soundName.Contains("_reward_", StringComparison.OrdinalIgnoreCase))
            return MusicCategory.Reward;

        if (soundName.Contains("_cin_", StringComparison.OrdinalIgnoreCase))
            return MusicCategory.Unused;
        
        // Areas
        if (soundName.Contains("_base_"))
            return MusicCategory.Generic;
        if (soundName.Contains("_city_"))
            return MusicCategory.City;
        if (soundName.Contains("_pyramid_"))
            return MusicCategory.Pyramid;
        if (soundName.Contains("_cliff_"))
            return MusicCategory.Cliff;
        if (soundName.Contains("_ruins_"))
            return MusicCategory.Ruins;
        if (soundName.Contains("_sewers_"))
            return MusicCategory.Sewers;
        if (soundName.Contains("_snowcliff_"))
            return MusicCategory.SnowCliff;

        // Villains
        if (soundName.Contains("tarbeige", StringComparison.OrdinalIgnoreCase)
            || soundName.Contains("vaiken", StringComparison.OrdinalIgnoreCase)
            || soundName.Contains("zigoba", StringComparison.OrdinalIgnoreCase)
            || soundName.Contains("crystalbowie", StringComparison.OrdinalIgnoreCase)
            || soundName.Contains("sandra", StringComparison.OrdinalIgnoreCase))
            return MusicCategory.Villain;

        // Default
        return MusicCategory.Main;
    }
}