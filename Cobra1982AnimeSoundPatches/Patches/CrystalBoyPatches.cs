using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cobra1982AnimeSoundPatches.Patches;

[HarmonyPatch]
public static class CrystalBoyPatches
{
    private const string FootstepSoundName = "crystal_boy_footstep";
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayableDirector), nameof(PlayableDirector.Play), new Type[0])]
    public static void FootstepSoundPatcher(PlayableDirector __instance)
    {
        // Check if should patch
        if (__instance.playableAsset == null) return;
        if (!__instance.playableAsset.name.Equals("Prefab_CS_Sequence_8_2_BossBowie_Intro",
                StringComparison.OrdinalIgnoreCase)) return;
        if (__instance.playableAsset is not TimelineAsset timeline) return;

        try
        {
            // change sound
            ModifyCrystalBoyFootstepSound(timeline);
            Playable rootPlayable = __instance.playableGraph.GetRootPlayable(0);
            rootPlayable.SetSpeed(rootPlayable.GetSpeed() * 0.5f);
            /*
            foreach (var animator in __instance.GetComponentsInChildren<Animator>(true))
            {
                animator.speed *= 0.5f;
                Plugin.Logger.LogMessage("Animator slowed down");
            }*/
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while modifying Crystal Boy footsteps: " + e);
        }
    }

    private static void ModifyCrystalBoyFootstepSound(TimelineAsset timeline)
    {
        Plugin.Logger.LogInfo("Patching Crystal Boy playable directory");

        // Load sound
        if (!CobraSoundReplacer.API.CustomSoundUtils.TryGetIdForCustomSound(FootstepSoundName,
                out var soundId) || soundId == 0)
        {
            Plugin.Logger.LogError("Failed to find Crystal Boy footstep eClip");
            return;
        }

        if (soundId >= AudioController.Audio.AllClip.Length)
        {
            Plugin.Logger.LogError("Sound clip out of range");
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

        AudioPlayableAsset playableAudioAsset = ScriptableObject.CreateInstance<AudioPlayableAsset>();
        playableAudioAsset.clip = rawAudioClip;
        playableAudioAsset.bufferingTime = 0.1f;

        // Replace audio
        List<TimelineClip> footstepSoundClips;
        try
        {
            footstepSoundClips = (timeline.m_Tracks[5] as GroupTrack)?.m_ChildTrackCache.First().m_Clips;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Exception thrown while locating Crystal Boy footstep sounds: " + e);
            return;
        }
        
        if (footstepSoundClips != null && footstepSoundClips.Count != 0)
        {
            foreach (var frame in footstepSoundClips)
            {
                frame.asset = playableAudioAsset;
            }

            Plugin.Logger.LogInfo("Replaced Crystal Boy footstep sounds successfully");
        }
        else
        {
            Plugin.Logger.LogError("Could not find Crystal Boy footstep sounds to replace");
        }
    }
}

[HarmonyPatch(typeof(NmiCrystalBowie))]
[HarmonyPatch(nameof(NmiCrystalBowie.Shoot))]
public static class NmiCrystalBoyShootSoundTranspiler
{
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool done = false;
        bool foundMummy = false;
        foreach (var instruction in instructions)
        {
            if (done)
            {
                yield return instruction;
                continue;
            }

            if (foundMummy)
            {
                yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                done = true;
                continue;
            }
            if (instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 305)
            {
                foundMummy = true;
            }
            yield return instruction;
        }
    }
}

[HarmonyPatch]
public static class PlayNewCrystalBoyTakedownPatch
{
    private const string NewTakedownSound = "new_crystal_boy_takedown";
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiCrystalBowie), nameof(NmiCrystalBowie.KillCobraIfNear))]
    private static void PatchNormalCrystalBoy(ref bool __result)
    {
        if (__result) PlayNewCrystalBoyKillSound();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NmiCrystalBowieClone), nameof(NmiCrystalBowieClone.KillCobraIfNear))]
    private static void PatchCloneCrystalBoy(ref bool __result)
    {
        if (__result) PlayNewCrystalBoyKillSound();
    }

    private static void PlayNewCrystalBoyKillSound()
    {
        if (CobraSoundReplacer.API.CustomSoundUtils.TryGetEClip(NewTakedownSound, out var clip))
        {
            AudioController.Instance.PlayEnemySound(clip);
        }
        else
        {
            Plugin.Logger.LogError("Crystal Boy takedown sound clip not found");
        }
    }
}