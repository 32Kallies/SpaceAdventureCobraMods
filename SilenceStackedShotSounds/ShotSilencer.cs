using System;
using System.Collections.Generic;

namespace SilenceStackedShotSounds;

public static class ShotSilencer
{
    public static void Initialize()
    {
        MutedSoundHashes.Clear();
        
        foreach (var eClip in EClipsToMute)
        {
            string soundName = AudioController.Audio.asd.getSoundName(eClip);
            int hash = GetHashFromSoundClipName(soundName);
            MutedSoundHashes.Add(hash);
        }

        foreach (string moddedSound in ModdedSoundsToMute)
        {
            MutedSoundHashes.Add(GetHashFromSoundClipName(moddedSound));
        }
    }
    
    private static readonly audioSelectionData.eCLIP[] EClipsToMute = [
        audioSelectionData.eCLIP.PLY_SHOOT_PG_LAUNCHED,
        audioSelectionData.eCLIP.PLY_SHOOT_PG_CHARGEDLAUNCHED,
        audioSelectionData.eCLIP.PLR_PSYCHOGUN_CHARGED_SHOT_DESTRUCTIBLE,
        audioSelectionData.eCLIP.NMI_HIT_BY_PG_GS
    ];
    
    private static readonly string[] ModdedSoundsToMute = [
        "penetrating_shot"
    ];

    private static readonly HashSet<int> MutedSoundHashes = [];
    
    private static int GetHashFromSoundClipName(string clipName)
    {
        return clipName.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
    
    public static void QuietenSounds()
    {
        foreach (CAudio.CPlayingAudioData data in AudioController.Audio.audioSrc)
        {
            if (MutedSoundHashes.Contains(data.hashName) && data.asrc.time > Plugin.TimePadding.Value)
            {
                if (data.asrc.GetComponent<ShotSoundGradualSilencer>() == null)
                {
                    Plugin.Logger.LogDebug("Quieting sound: " + data.asrc.clip);
                    data.asrc.gameObject.AddComponent<ShotSoundGradualSilencer>()
                        .Initialize(data, Plugin.FadeDuration.Value, Plugin.TargetVolume.Value);
                }
            }
        }
    }
}