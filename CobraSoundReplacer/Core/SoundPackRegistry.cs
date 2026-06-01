using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using CobraSoundReplacer.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace CobraSoundReplacer.Core;

public static class SoundPackRegistry
{
    private static readonly Dictionary<string, RegisteredSoundPack> SoundPacks = [];

    private static SoundDatabase _soundDatabase;

    private static CAudio _cAudio;

    internal static readonly Dictionary<ushort, AudioClip> SoundReplacements = new();
    internal static readonly Dictionary<ushort, float> CustomSoundVolumes = new();
    internal static readonly Dictionary<string, ushort> NewSoundsIDs = new();
    internal static readonly Dictionary<ushort, AudioClip> NewSoundClips = new();
    internal static readonly Dictionary<ushort, NewSound> NewSoundData = new();
    internal static readonly Dictionary<string, audioSelectionData.eCLIP> CustomEClips = new();
    internal static readonly Dictionary<audioSelectionData.eCLIP, string> CustomEClipSounds = new();
    internal static readonly HashSet<audioSelectionData.eCLIP> CustomEClipSet = new();

    private static ushort _nextFreeCustomEClipIndex = 500;

    private static readonly List<RegisteredSoundPack> RuntimeSoundPacks = new();
    
    public static bool ReplacementCompleted { get; private set; }

    public static void RegisterRuntimeSoundPack(SoundPack pack, string containingFolderPath)
    {
        if (ReplacementCompleted)
        {
            Plugin.Logger.LogError("Registering custom pack after audio has already been replaced");
        }
        RuntimeSoundPacks.Add(new RegisteredSoundPack(pack, containingFolderPath));
    }

    internal static IEnumerator RegisterSoundPack(SoundPack pack, string containingFolderPath)
    {
        if (SoundPacks.ContainsKey(pack.PackName))
        {
            Plugin.Logger.LogWarning($"A sound pack is already registered by the name of '{pack.PackName}'. Skipping.");
            yield break;
        }

        var registered = new RegisteredSoundPack(pack, containingFolderPath);
        SoundPacks.Add(pack.PackName, registered);
        yield return RefreshPack(registered);
    }

    internal static IEnumerator LoadPacksAutomatically()
    {
        var pluginFolder = Paths.PluginPath;
        var allFiles = Directory.GetFiles(pluginFolder, "*." + Constants.FileExtension, SearchOption.AllDirectories);
        if (allFiles.Length == 0)
        {
            Plugin.Logger.LogWarning($"No packs found in plugin folder. Skipping.");
            yield break;
        }

        Plugin.Logger.LogInfo($"Found {allFiles.Length} sound packs to load. Loading...");

        int successes = 0;
        int disabledPacks = 0;
        foreach (var path in allFiles)
        {
            SoundPack pack;
            try
            {
                var text = File.ReadAllText(path);
                pack = JsonConvert.DeserializeObject<SoundPack>(text);
                successes++;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Exception thrown while loading sound pack at path '{path}': " + e.Message);
                continue;
            }

            if (pack.Enable)
                yield return RegisterSoundPack(pack, Path.GetDirectoryName(path));
            else
                disabledPacks++;
        }

        Plugin.Logger.LogInfo($"Loaded {successes} sound pack(s).");
        if (disabledPacks > 0)
        {
            Plugin.Logger.LogInfo($"Skipped registering {disabledPacks} pack(s) because they were not enabled.");
        }
    }

    internal static IEnumerator OnAudioInitialized(CAudio cAudio)
    {
        _cAudio = cAudio;
        _soundDatabase = new SoundDatabase(_cAudio);
        _soundDatabase.InitializeDatabase();

        foreach (var pack in SoundPacks)
        {
            yield return RefreshPack(pack.Value);
        }

        foreach (var runtimePack in RuntimeSoundPacks)
        {
            yield return RefreshPack(runtimePack);
        }

        // Register new custom sounds into cAudio:
        
        var allClips = _cAudio.AllClip;
        var newArray = new CAudio.CAudioClip[allClips.Length + NewSoundClips.Count];
        allClips.CopyTo(newArray, 0);
        int i = 0;
        foreach (var newSound in NewSoundsIDs)
        {
            var data = NewSoundData[newSound.Value];
            newArray[allClips.Length + i] = new CAudio.CAudioClip
            {
                clip = NewSoundClips[newSound.Value],
                loadname = newSound.Key,
                type = (CAudio.eVolumeType)data.VolumeType,
                loop = data.Looping
            };
            i++;
        }

        _cAudio.AllClip = newArray;
        
        Plugin.Logger.LogMessage("All audio loaded");
        
        ReplacementCompleted = true;
    }

    private static IEnumerator RefreshPack(RegisteredSoundPack pack)
    {
        if (_cAudio == null)
        {
            yield break;
        }

        if (!pack.InitializedEver)
        {
            yield return InitializeSoundPack(pack);
        }
    }

    private static IEnumerator InitializeSoundPack(RegisteredSoundPack pack)
    {
        if (pack.Pack.NewSounds != null)
        {
            foreach (var newSound in pack.Pack.NewSounds)
            {
                yield return AddNewSound(newSound.ClipName,
                    Path.Combine(pack.ContainingFolderPath, newSound.FilePath), newSound);
            }   
        }

        if (pack.Pack.SoundReplacements != null)
        {
            foreach (var replacement in pack.Pack.SoundReplacements)
            {
                yield return SetSoundReplacement(replacement.OriginalFileName,
                    Path.Combine(pack.ContainingFolderPath, replacement.ReplacementFilePath), replacement.Volume);
            }   
        }

        if (pack.Pack.NewEClips != null)
        {
            foreach (var eClip in pack.Pack.NewEClips)
            {
                RegisterEClip(eClip.Id, eClip.SoundName);
            }
        }

        pack.InitializedEver = true;
    }

    private static IEnumerator SetSoundReplacement(string originalFileName, string replacementFilePath, float volume)
    {
        if (!_soundDatabase.TryGetIndexFromFileName(originalFileName, out var index))
        {
            Plugin.Logger.LogWarning($"Failed to find original clip by name '{originalFileName}'. Skipping.");
            yield break;
        }

        var result = new TaskResult<AudioClip>();
        var loadedClip = AudioLoadingUtils.LoadAudioClipFromPath(replacementFilePath, result);
        yield return loadedClip;
        if (!result.Success)
        {
            Plugin.Logger.LogWarning($"Failed to load sound file at path '{replacementFilePath}'.");
            yield break;
        }

        var clip = result.GetResult();
        if (clip == null)
        {
            Plugin.Logger.LogWarning($"Clip from path '{replacementFilePath}' is nulL!.");
            yield break;
        }
        
        SoundReplacements[index] = clip;
        CustomSoundVolumes[index] = volume;
    }
    
    private static IEnumerator AddNewSound(string clipName, string filePath, NewSound soundData)
    {
        var newIndex = _soundDatabase.GetNewReservedIndex();
        
        var result = new TaskResult<AudioClip>();
        var loadedClip = AudioLoadingUtils.LoadAudioClipFromPath(filePath, result);
        yield return loadedClip;
        if (!result.Success)
        {
            Plugin.Logger.LogWarning($"Failed to load sound file at path '{filePath}'.");
            yield break;
        }

        var clip = result.GetResult();
        if (clip == null)
        {
            Plugin.Logger.LogWarning($"Clip from path '{filePath}' is nulL!.");
            yield break;
        }
        
        NewSoundsIDs.Add(clipName, newIndex);
        NewSoundClips.Add(newIndex, clip);
        NewSoundData.Add(newIndex, soundData);
    }

    private static void RegisterEClip(string id, string soundName)
    {
        var eClip = (audioSelectionData.eCLIP)_nextFreeCustomEClipIndex++;
        CustomEClips.Add(id, eClip);
        CustomEClipSet.Add(eClip);
        CustomEClipSounds.Add(eClip, soundName);
    }

    private class RegisteredSoundPack
    {
        public SoundPack Pack { get; }
        public string ContainingFolderPath { get; }
        public bool InitializedEver { get; set; }

        public RegisteredSoundPack(SoundPack pack, string containingFolderPath)
        {
            Pack = pack;
            ContainingFolderPath = containingFolderPath;
        }
    }
}