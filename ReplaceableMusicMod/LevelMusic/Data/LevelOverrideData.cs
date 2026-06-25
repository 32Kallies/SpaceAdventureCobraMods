using System;
using System.Collections.Generic;
using CobraSoundReplacer.API;
using MusicReplacer.NewMusicSystem;
using Newtonsoft.Json;

namespace MusicReplacer.LevelMusic.Data;

[Serializable]
public class LevelOverrideData
{
    [JsonProperty]
    private int defaultMusic;
    [JsonProperty]
    private int arenaMusic;
    [JsonProperty]
    private Dictionary<string, int> triggerReplacements;
    
    [JsonProperty]
    private string customDefaultMusic;
    [JsonProperty]
    private string customArenaMusic;
    [JsonProperty]
    private Dictionary<string, string> customTriggerReplacements;
    
    [JsonProperty]
    private bool isDefaultMusicCustom;
    [JsonProperty]
    private bool isArenaMusicCustom;
    [JsonProperty]
    private Dictionary<string, bool> isTriggerReplacementCustom;

    /*
    public Dictionary<string, int> GetTriggerReplacements()
    {
        triggerReplacements ??= new Dictionary<string, int>();
        return triggerReplacements;
    }*/
    
    public IReadOnlyDictionary<string, int> BuildTriggerReplacementsDictionary()
    {
        triggerReplacements ??= new Dictionary<string, int>();
        customTriggerReplacements ??= new Dictionary<string, string>();
        isTriggerReplacementCustom ??= new Dictionary<string, bool>();

        var replacements = new Dictionary<string, int>();
        var hashes = new HashSet<string>();
        foreach (var replacement in triggerReplacements)
        {
            hashes.Add(replacement.Key);
        }
        foreach (var replacement in customTriggerReplacements)
        {
            hashes.Add(replacement.Key);
        }

        foreach (var hash in hashes)
        {
            int clip = AccessTriggerMusic(hash).GetEClip();
            replacements.Add(hash, clip);
        }
        
        return replacements;
    }

    public bool HasTriggerReplacement(string hash)
    {
        return TryGetTriggerReplacement(hash, out _);
    }
    
    public bool TryGetTriggerReplacement(string hash, out int clip)
    {
        bool custom = isTriggerReplacementCustom != null && isTriggerReplacementCustom.GetValueOrDefault(hash, false);
        if (custom)
        {
            if (customTriggerReplacements != null && customTriggerReplacements.TryGetValue(hash, out var replacementName))
            {
                clip = NewMusicLoader.GetEClipForCustomSound(replacementName);
                return clip != 0;
            }

            clip = 0;
            return false;
        }

        if (triggerReplacements != null && triggerReplacements.TryGetValue(hash, out clip))
        {
            return clip != 0;
        }

        clip = 0;
        return false;
    }

    public Wrapper AccessDefaultMusic() => Wrapper.CreateForAmbientMusic(this);
    public Wrapper AccessArenaMusic() => Wrapper.CreateForArenaMusic(this);
    public Wrapper AccessTriggerMusic(string hash) => Wrapper.CreateForTrigger(this, hash);

    public void ClearTriggerReplacements()
    {
        triggerReplacements?.Clear();
        customTriggerReplacements?.Clear();
        isTriggerReplacementCustom?.Clear();
    }

    public bool HasAnyTriggerReplacements()
    {
        return triggerReplacements is { Count: > 0 } || customTriggerReplacements is { Count: > 0 };
    }
    
    public class Wrapper
    {
        private readonly Category _category;
        private readonly LevelOverrideData _source;
        private string _triggerHash;

        private Wrapper(Category category, LevelOverrideData source)
        {
            _category = category;
            _source = source;
        }

        public static Wrapper CreateForAmbientMusic(LevelOverrideData source)
        {
            return new Wrapper(Category.Default, source);
        }
    
        public static Wrapper CreateForArenaMusic(LevelOverrideData source)
        {
            return new Wrapper(Category.Arena, source);
        }
    
        public static Wrapper CreateForTrigger(LevelOverrideData source, string hash)
        {
            return new Wrapper(Category.Trigger, source)
            {
                _triggerHash = hash
            };
        }
    
        public int GetEClip()
        {
            switch (_category)
            {
                case Category.Default:
                    if (_source.isDefaultMusicCustom)
                        return NewMusicLoader.GetEClipForCustomSound(_source.customDefaultMusic);
                    return _source.defaultMusic;
                case Category.Arena:
                    if (_source.isArenaMusicCustom)
                        return NewMusicLoader.GetEClipForCustomSound(_source.customArenaMusic);
                    return _source.arenaMusic;
                case Category.Trigger:
                    if (_source.isTriggerReplacementCustom != null && _source.isTriggerReplacementCustom[_triggerHash])
                    {
                        if (_source.customTriggerReplacements == null || _source.customTriggerReplacements.Count == 0)
                            return 0;
                        if (_source.customTriggerReplacements.TryGetValue(_triggerHash, out var replacement))
                            return NewMusicLoader.GetEClipForCustomSound(replacement);
                        return 0;
                    }
                    
                    if (_source.triggerReplacements == null || _source.triggerReplacements.Count == 0)
                        return 0;
                    return _source.triggerReplacements.GetValueOrDefault(_triggerHash);
            }
            throw new ArgumentOutOfRangeException(nameof(_category));
        }

        public void SetEClip(audioSelectionData.eCLIP clip)
        {
            SetEClip((int)clip);
        }
    
        public void SetEClip(int clip)
        {
            if (CustomSoundUtils.TryGetCustomSoundNameForEClip((audioSelectionData.eCLIP)clip, out var customSoundName))
            {
                SetCustomSound(customSoundName);
                return;
            }
            
            switch (_category)
            {
                case Category.Default:
                    _source.defaultMusic = clip;
                    _source.isDefaultMusicCustom = false;
                    return;
                case Category.Arena:
                    _source.arenaMusic = clip;
                    _source.isArenaMusicCustom = false;
                    return;
                case Category.Trigger:
                    _source.triggerReplacements ??= new Dictionary<string, int>();
                    _source.isTriggerReplacementCustom ??= new Dictionary<string, bool>();

                    _source.isTriggerReplacementCustom[_triggerHash] = false;
                    
                    if (clip != 0)
                    {
                        _source.triggerReplacements[_triggerHash] = clip;
                    }
                    else
                    {
                        _source.triggerReplacements.Remove(_triggerHash);
                    }
                    return;
            }
            throw new ArgumentOutOfRangeException(nameof(_category));
        }
        
        public void SetCustomSound(string customSoundId)
        {
            switch (_category)
            {
                case Category.Default:
                    _source.customDefaultMusic = customSoundId;
                    _source.isDefaultMusicCustom = true;
                    return;
                case Category.Arena:
                    _source.customArenaMusic = customSoundId;
                    _source.isArenaMusicCustom = true;
                    return;
                case Category.Trigger:
                    _source.customTriggerReplacements ??= new Dictionary<string, string>();
                    _source.isTriggerReplacementCustom ??= new Dictionary<string, bool>();
                    
                    _source.isTriggerReplacementCustom[_triggerHash] = true;

                    if (!string.IsNullOrEmpty(customSoundId))
                    {
                        _source.customTriggerReplacements[_triggerHash] = customSoundId;
                    }
                    else
                    {
                        _source.customTriggerReplacements.Remove(_triggerHash);
                    }
                    return;
            }
            throw new ArgumentOutOfRangeException(nameof(_category));
        }

        public void Reset()
        {
            SetEClip(0);
            SetCustomSound(null);
        }

        private enum Category
        {
            Default,
            Arena,
            Trigger
        }
    }
}