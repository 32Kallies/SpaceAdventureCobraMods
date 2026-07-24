using System;
using HarmonyLib;
using UnityEngine;

namespace MusicReplacer;

[HarmonyPatch]
public static class FixMusicRestartingPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioController), nameof(AudioController.PlayMusic))]
    public static void PlayMusicPrefix(ref bool _forcerestartmusic)
    {
        _forcerestartmusic = false;
    }

    /*
    private static float _timeCleanDeadMusicAgain;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioController), nameof(AudioController.Update))]
    public static void UpdatePostfix(AudioController __instance)
    {
        if (Time.time < _timeCleanDeadMusicAgain)
            return;
        _timeCleanDeadMusicAgain = Time.time + 1;

        var sources = AudioController.Audio.audioSrc;
        foreach (var source in sources)
        {
            if (source.type != CAudio.eVolumeType.music)
                continue;
            if (source.asrc == null)
                continue;
            if (source.asrc.clip == null ||
                source.asrc.clip.name.Equals("mus_loop_main_theme", StringComparison.OrdinalIgnoreCase))
                return;
            if (source.asrc.volume == 0)
                source.asrc.Stop();
        }
    }
    */
}