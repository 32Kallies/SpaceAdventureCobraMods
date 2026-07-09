using HarmonyLib;

namespace MusicRestartFix;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUIMainMenu), nameof(NUIMainMenu.Start))]
    public static void MainMenuStartPostfix()
    {
        KillStaleAudio();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NUILevelSelectionPanel), nameof(NUILevelSelectionPanel.Start))]
    public static void LevelSelectionPanelStartPostfix()
    {
        KillStaleAudio();
    }

    private static void KillStaleAudio()
    {
        var sources = AudioController.Audio.audioSrc;
        foreach (var source in sources)
        {
            var audioSource = source.asrc;
            bool shouldStop = audioSource.clip != null && !audioSource.clip.name.StartsWith("mus_loop_main_theme");
            if (shouldStop)
            {
                Plugin.Logger.LogDebug("Stopping unused song");
                source.asrc.Stop();
            }
        }
    }
}