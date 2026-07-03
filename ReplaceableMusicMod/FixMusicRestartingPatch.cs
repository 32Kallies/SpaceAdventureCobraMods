using HarmonyLib;

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
}