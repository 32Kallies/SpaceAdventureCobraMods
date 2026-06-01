using System.Collections.Generic;
using System.Reflection.Emit;
using CobraSoundReplacer.API;
using CobraSoundReplacer.Core;
using HarmonyLib;
using UnityEngine;

namespace CobraSoundReplacer.Patches;

[HarmonyPatch(typeof(CAudio))]
public static class CAudioPatcher
{
    [HarmonyPatch(nameof(CAudio.Init))]
    [HarmonyPostfix]
    public static void InitPatch(CAudio __instance)
    {
        Plugin.StartCoroutineOnPlugin(SoundPackRegistry.OnAudioInitialized(__instance));
    }
    
    [HarmonyPatch(nameof(CAudio.getClip))]
    [HarmonyPostfix]
    public static void GetClipPatch(CAudio __instance, string clipName, ref ushort[] __result)
    {
        if (!CustomSoundUtils.TryGetIdForCustomSound(clipName, out ushort idx))
            return;
        
        if (__result is { Length: > 0 })
            return;
        
        __result = [idx];
    }
}

[HarmonyPatch(typeof(CAudio), nameof(CAudio.play), typeof(ushort), typeof(float), typeof(float),
    typeof(CAudio.eVolumeType), typeof(bool), typeof(audioSelectionData.eCLIP))]
public static class PlayAudioByIdPatch
{
    static void Prefix(ushort idx, ref float vol)
    {
        if (PlayOriginalUtils.NextSoundPlaysOriginal)
            return;
        
        if (SoundPackRegistry.CustomSoundVolumes.TryGetValue(idx, out var customVolume))
        {
            vol *= customVolume;
        }
        else if (SoundPackRegistry.NewSoundData.TryGetValue(idx, out var newSoundData))
        {
            vol *= newSoundData.Volume;
        }
    }

    static void Postfix(ushort idx, ref float vol)
    {
        PlayOriginalUtils.OnAnySoundPlayed();
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        var audioSource = generator.DeclareLocal(typeof(AudioSource));

        // Find label for where the audio volume is set (after the only pop)
        codeMatcher.Start();
        var setVolumeLabel = generator.DefineLabel();
        var resumeTarget = codeMatcher
            .MatchForward(true, new CodeMatch(OpCodes.Pop))
            .ThrowIfInvalid("Couldn't find pop")
            .Advance(1).Instruction;
        resumeTarget.labels.Add(setVolumeLabel);

        // Find label for where the addressable sound normally loads
        codeMatcher.Start();
        var addressableLoadLabel = generator.DefineLabel();
        var addressableLoadTarget = codeMatcher
            .MatchForward(true, new CodeMatch(OpCodes.Ldstr))
            .MatchForward(true, new CodeMatch(OpCodes.Ldloc_0))
            .MatchForward(true, new CodeMatch(OpCodes.Callvirt))
            .MatchForward(true, new CodeMatch(OpCodes.Ldloc_1))
            .ThrowIfInvalid("Couldn't find start of normal addressable loading")
            .Instruction;
        addressableLoadTarget.labels.Add(addressableLoadLabel);

        codeMatcher.Start();
        codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Ldstr))
            // FIND STARTING POINT
            .ThrowIfInvalid("Could not find where the string is loaded onto the stack.")
            .MatchForward(true, new CodeMatch(OpCodes.Ldloc_0))
            .ThrowIfInvalid("Could not find the next ldloc.0.")
            .MatchForward(true, new CodeMatch(OpCodes.Callvirt))
            .ThrowIfInvalid("Could not find the AddComponent<AudioSource>() callvirt.")
            .Advance(1)
            // DUPLICATE AUDIO SOURCE REFERENCE AND SAVE TO LOCAL VARIABLE
            .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
            .ThrowIfInvalid("Failed to insert the 'dup' operation.")
            .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, audioSource.LocalIndex))
            .ThrowIfInvalid("Failed to insert the 'stloc.s 14' operation.")
            // Traverse to before the AddressableManager.GetAsset call
            .MatchForward(false, new CodeMatch(OpCodes.Ldloc_1))
            .ThrowIfInvalid("Failed to find the next ldloc.1 operation.")
            // Load the sound ID and check if the sound is overriden, and if not, jump past "SET CUSTOM SOUND"
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .ThrowIfInvalid("Failed to load the ID onto the stack.")
            .InsertAndAdvance(CodeInstruction.Call(typeof(PlayAudioByIdPatch), nameof(IsSoundOverriden)))
            .ThrowIfInvalid("Failed to insert call to check if the sound is overriden.")
            .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, addressableLoadLabel))
            .ThrowIfInvalid("Failed to insert branch jump to skip addressable load call.")
            // SET CUSTOM SOUND
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, audioSource.LocalIndex))
            .ThrowIfInvalid("Failed to insert pushing the audio source onto the stack.")
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .ThrowIfInvalid("Failed to insert loading of the sound ID.")
            .InsertAndAdvance(CodeInstruction.Call(typeof(PlayAudioByIdPatch), nameof(GetCustomSound)))
            .ThrowIfInvalid("Failed to insert call to get the custom sound clip.")
            .InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt,
                AccessTools.PropertySetter(typeof(AudioSource), nameof(AudioSource.clip))))
            .ThrowIfInvalid("Failed to insert call to set the clip.")
            // RETURN TO NORMAL OPERATION
            .InsertAndAdvance(new CodeInstruction(OpCodes.Br, setVolumeLabel))
            .ThrowIfInvalid("Failed to insert jump to after the original clip loading logic.");

        return codeMatcher.Instructions();
    }
    
    private static bool IsSoundOverriden(ushort soundId)
    {
        if (PlayOriginalUtils.NextSoundPlaysOriginal)
            return false;
        
        return SoundPackRegistry.NewSoundClips.ContainsKey(soundId) ||
               SoundPackRegistry.SoundReplacements.ContainsKey(soundId);
    }

    private static AudioClip GetCustomSound(ushort soundId)
    {
        if (!SoundPackRegistry.NewSoundClips.TryGetValue(soundId, out var clip) &&
            !SoundPackRegistry.SoundReplacements.TryGetValue(soundId, out clip))
        {
            Plugin.Logger.LogError("Failed to find custom sound clip or sound replacement by ID " + soundId);
        }

        return clip;
    }
}