using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cobra1982AnimeSoundPatches.Patches.Bosses;

[HarmonyPatch]
public static class ZigobaPatches
{
    private const string PsychogunCutsceneSound = "custom_psychogun_sound_for_cutscenes";
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayableDirector), nameof(PlayableDirector.Play), new Type[0])]
    public static void PsychogunSoundPatcher(PlayableDirector __instance)
    {
        // Check if we should patch this
        if (__instance.playableAsset == null) return;
        if (!__instance.playableAsset.name.Equals("timeline_C2_3_2_Zig_Intro_sequence_V2",
                StringComparison.OrdinalIgnoreCase)) return;
        if (__instance.playableAsset is not TimelineAsset timeline) return;

        try
        {
            UseProperPsychogunSoundForCutscene(timeline);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while modifying Zigoba cutscene sound: " + e);
        }
    }

    private static void UseProperPsychogunSoundForCutscene(TimelineAsset timeline)
    {
        Plugin.Logger.LogInfo("Patching Zigoba playable director");

        // Load sound
        if (!CobraSoundReplacer.API.CustomSoundUtils.TryGetIdForCustomSound(PsychogunCutsceneSound,
                out var soundId) || soundId == 0)
        {
            Plugin.Logger.LogError("Failed to find psychogun cutscene sound eClip");
            return;
        }

        if (soundId >= AudioController.Audio.AllClip.Length)
        {
            Plugin.Logger.LogError("Psychogun sound clip out of range");
            return;
        }

        var clipData = AudioController.Audio.AllClip[soundId];
        if (clipData == null)
        {
            Plugin.Logger.LogError("Failed to find clip data for sound ID: " + soundId);
            return;
        }

        AudioClip rawAudioClip = clipData.clip;
        if (rawAudioClip == null)
        {
            Plugin.Logger.LogError("AudioClip does not exist for sound ID: " + soundId);
            return;
        }
        
        OnPyschogunSoundLoaded(timeline, rawAudioClip);
    }

    private static void OnPyschogunSoundLoaded(TimelineAsset timeline, AudioClip clip)
    {
        if (clip == null)
        {
            Plugin.Logger.LogError("AudioClip does not exist for psychogun shot sound");
            return;
        }

        AudioPlayableAsset playableAudioAsset = ScriptableObject.CreateInstance<AudioPlayableAsset>();
        playableAudioAsset.clip = clip;
        playableAudioAsset.bufferingTime = 0.1f;

        // Replace audio
        List<TimelineClip> shootSoundClips;
        try
        {
            shootSoundClips = (timeline.m_Tracks[5] as GroupTrack)?.m_ChildTrackCache.First().m_Clips;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while locating Psychogun shot sounds for Zigoba cutscene: " + e);
            return;
        }
        
        if (shootSoundClips != null && shootSoundClips.Count != 0)
        {
            foreach (var frame in shootSoundClips)
            {
                frame.asset = playableAudioAsset;
                frame.duration = clip.length;
            }

            Plugin.Logger.LogInfo("Replaced Zigoba cutscene Psychogun shot sounds successfully");
        }
        else
        {
            Plugin.Logger.LogError("Could not find Zigoba cutscene Psychogun shot sounds to replace");
        }
    }
}