using CobraSoundReplacer.Core;

namespace CobraSoundReplacer.API;

public static class CustomSoundUtils
{
    public static bool TryGetIdForCustomSound(string clipName, out ushort id)
    {
        return SoundPackRegistry.NewSoundsIDs.TryGetValue(clipName, out id);
    }
    
    public static bool TryGetEClip(string eClipId, out audioSelectionData.eCLIP eClip)
    {
        return SoundPackRegistry.CustomEClips.TryGetValue(eClipId, out eClip);
    }

    public static bool IsEClipCustom(audioSelectionData.eCLIP clip)
    {
        return SoundPackRegistry.CustomEClipSet.Contains(clip);
    }
    
    public static bool TryGetCustomSoundNameForEClip(audioSelectionData.eCLIP clip, out string soundName)
    {
        return SoundPackRegistry.CustomEClipSounds.TryGetValue(clip, out soundName);
    }
}